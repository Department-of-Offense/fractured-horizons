using System.Linq;
using System.Threading.Tasks;
using Dof.Cameras;
using Dof.Core;
using Dof.Hybrid;
using Dof.Testing;
using Dof.Units;
using FracturedHorizons.Labs;
using Godot;

namespace FracturedHorizons.Tests;

public class LabCatalogTests
{
    [Test]
    public async Task EveryLabLoadsAndRuns(TestContext ctx)
    {
        foreach (var info in LabCatalog.All)
        {
            var packed = GD.Load<PackedScene>(info.Scene);
            Check.True(packed is not null, $"{info.Id}: scene {info.Scene} loads");
            var lab = packed!.Instantiate<Lab>();
            if (lab is StressTest stress) stress.UnitsPerSide = 20; // keep CI fast
            ctx.Add(lab);
            await ctx.PhysicsFrames(90);
            lab.QueueFree();
            await ctx.PhysicsFrames(2);
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    [Test]
    public Task LabIdsAreUnique(TestContext ctx)
    {
        var ids = LabCatalog.All.Select(l => l.Id).ToList();
        Check.Equal(ids.Count, ids.Distinct().Count(), "unique lab ids");
        return Task.CompletedTask;
    }
}

public class CameraLabTests
{
    [Test]
    public async Task DroneFliesAndEveryViewWorks(TestContext ctx)
    {
        var lab = ctx.Add(GD.Load<PackedScene>("res://scenes/labs/CameraLab.tscn").Instantiate<CameraLab>());
        await ctx.PhysicsFrames(2);
        var start = lab.Drone.GlobalPosition;

        Input.ActionPress(CoreActions.MoveForward);
        await ctx.PhysicsFrames(30);
        Input.ActionRelease(CoreActions.MoveForward);
        Check.True(lab.Drone.GlobalPosition.Z < start.Z - 1f, $"drone flew forward: {start} -> {lab.Drone.GlobalPosition}");

        for (var i = 0; i < lab.Director.FollowViews.Count; i++)
        {
            lab.Director.CycleFollowView();
            await ctx.PhysicsFrames(5);
        }
        lab.ToggleStrategy();
        Check.True(lab.Director.Active is StrategyCameraRig, "Tab switches to strategy view");
        lab.ToggleStrategy();
        Check.True(lab.Director.Active is FollowRig, "and back to following the drone");
    }
}

public class FiringRangeTests
{
    [Test]
    public async Task ShootingDestroysTargets(TestContext ctx)
    {
        var lab = ctx.Add(GD.Load<PackedScene>("res://scenes/labs/FiringRange.tscn").Instantiate<FiringRange>());
        await ctx.PhysicsFrames(2);
        lab.ShowView(0); // first person: crosshair = muzzle line
        lab.Pilot.LookYaw = 0f; // nearest lane-0 target is straight ahead
        lab.Pilot.LookPitch = 0f;
        await ctx.PhysicsFrames(30); // let the camera blend in

        Input.ActionPress(UnitActions.Fire);
        await ctx.Until(() => lab.Kills > 0, 240, "a target to be destroyed");
        Input.ActionRelease(UnitActions.Fire);
        Check.True(lab.Hits > 0 && lab.Hits <= lab.Shots, $"hits {lab.Hits} of {lab.Shots} shots");
    }
}

public class SquadCommandTests
{
    [Test]
    public async Task RightClickingAFriendlyMakesTheSquadFollow(TestContext ctx)
    {
        var lab = ctx.Add(GD.Load<PackedScene>("res://scenes/labs/SquadCommand.tscn").Instantiate<SquadCommand>());
        await ctx.PhysicsFrames(2);

        var squad = UnitRegistry.OnTeam(ctx.Tree, 0).ToList();
        var leader = squad[0];
        lab.Selection.Select(squad.Skip(1));
        lab.Selection.IssueOrder(leader.GlobalPosition, leader, append: false, attackModifier: false);
        Check.True(squad.Skip(1).All(u => u.Commands.Current is FollowCommand), "followers got the lab-defined Follow order");

        leader.Issue(new MoveCommand(leader.GlobalPosition + new Vector3(25, 0, 0)));
        await ctx.PhysicsFrames(60 * 8);
        var straggler = squad.Skip(1).Max(u => u.GlobalPosition.DistanceTo(leader.GlobalPosition));
        Check.True(straggler < 10f, $"squad kept up with the leader (furthest {straggler:0.0} m)");
    }
}

public class SkirmishTests
{
    private static async Task<Skirmish> Load(TestContext ctx)
    {
        var scene = GD.Load<PackedScene>("res://scenes/labs/Skirmish.tscn").Instantiate<Skirmish>();
        ctx.Add(scene);
        await ctx.PhysicsFrames(2);
        return scene;
    }

    [Test]
    public async Task StartsWithBothSides(TestContext ctx)
    {
        var skirmish = await Load(ctx);
        Check.Equal(skirmish.SquadSize, UnitRegistry.OnTeam(ctx.Tree, Skirmish.PlayerTeam).Count(), "player squad");
        Check.Equal(skirmish.HostileCount, UnitRegistry.OnTeam(ctx.Tree, Skirmish.HostileTeam).Count(), "hostiles");
        Check.Equal(Skirmish.Outcome.InProgress, skirmish.State, "state at start");
    }

    [Test]
    public async Task HostilesAdvanceAfterDelay(TestContext ctx)
    {
        await Load(ctx);
        var hostile = UnitRegistry.OnTeam(ctx.Tree, Skirmish.HostileTeam).First();
        await ctx.Until(() => hostile.Commands.Current is AttackMoveCommand, 60 * 10, "hostiles to receive attack-move");
    }

    [Test]
    public async Task EveryPilotViewWorks(TestContext ctx)
    {
        var skirmish = await Load(ctx);
        var unit = UnitRegistry.OnTeam(ctx.Tree, Skirmish.PlayerTeam).First();
        Check.True(skirmish.Modes.TryPilot(unit), "can pilot a squad member");
        for (var i = 0; i < 3; i++)
        {
            await ctx.PhysicsFrames(10);
            skirmish.Modes.CyclePilotView();
        }
        skirmish.Modes.ReturnToStrategy();
        Check.Equal(GameModeManager.Mode.Strategy, skirmish.Modes.CurrentMode, "back in command");
    }

    [Test]
    public async Task WipingOutHostilesWins(TestContext ctx)
    {
        var skirmish = await Load(ctx);
        foreach (var hostile in UnitRegistry.OnTeam(ctx.Tree, Skirmish.HostileTeam).ToList())
            hostile.Health.TakeDamage(1000f);
        await ctx.PhysicsFrames(2);
        Check.Equal(Skirmish.Outcome.Victory, skirmish.State, "outcome");
    }

    [Test]
    public async Task LosingTheSquadLoses(TestContext ctx)
    {
        var skirmish = await Load(ctx);
        foreach (var friend in UnitRegistry.OnTeam(ctx.Tree, Skirmish.PlayerTeam).ToList())
            friend.Health.TakeDamage(1000f);
        await ctx.PhysicsFrames(2);
        Check.Equal(Skirmish.Outcome.Defeat, skirmish.State, "outcome");
    }
}

public class StressTestTests
{
    [Test]
    public async Task ArmiesEngageAtScale(TestContext ctx)
    {
        var lab = GD.Load<PackedScene>("res://scenes/labs/StressTest.tscn").Instantiate<StressTest>();
        lab.UnitsPerSide = 40;
        ctx.Add(lab);
        await ctx.PhysicsFrames(2);
        Check.Equal(80, UnitRegistry.All(ctx.Tree).Count(), "both armies spawned");
        await ctx.Until(() => UnitRegistry.All(ctx.Tree).Count() < 80, 60 * 30, "first casualties");
    }
}
