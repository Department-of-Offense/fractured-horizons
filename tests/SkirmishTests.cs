using System.Linq;
using System.Threading.Tasks;
using CommandEngine.Core;
using CommandEngine.Testing;
using CommandEngine.Units;
using Godot;

namespace FracturedHorizons.Tests;

public class SkirmishTests
{
    private static async Task<Skirmish> Load(TestContext ctx)
    {
        var scene = GD.Load<PackedScene>("res://scenes/Main.tscn").Instantiate<Skirmish>();
        ctx.Add(scene);
        await ctx.PhysicsFrames(2);
        return scene;
    }

    [Test]
    public async Task MainSceneLoadsWithBothSides(TestContext ctx)
    {
        var skirmish = await Load(ctx);
        Check.Equal(skirmish.SquadSize, UnitRegistry.OnTeam(ctx.Tree, Skirmish.PlayerTeam).Count(), "player squad");
        Check.Equal(skirmish.HostileCount, UnitRegistry.OnTeam(ctx.Tree, Skirmish.HostileTeam).Count(), "hostiles");
        Check.Equal(Skirmish.Outcome.InProgress, skirmish.State, "state at start");
    }

    [Test]
    public async Task HostilesAdvanceAfterDelay(TestContext ctx)
    {
        var skirmish = await Load(ctx);
        var hostile = UnitRegistry.OnTeam(ctx.Tree, Skirmish.HostileTeam).First();
        await ctx.Until(() => hostile.Commands.Current is AttackMoveCommand, 60 * 10, "hostiles to receive attack-move");
    }

    [Test]
    public async Task EveryPilotViewWorksInTheMainScene(TestContext ctx)
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
