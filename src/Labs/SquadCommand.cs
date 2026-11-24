using System.Linq;
using Dof.Cameras;
using Dof.Command;
using Dof.Prototyping;
using Dof.Units;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// RTS control with no piloting. Exercises the command system's extension
/// points: <see cref="FollowCommand"/> is a game-defined order, and
/// <see cref="SquadSelection"/> changes what right-clicking a friendly does.
/// </summary>
public partial class SquadCommand : Lab
{
    protected override string LabId => "squad";

    public CameraDirector Director { get; private set; } = null!;
    public SquadSelection Selection { get; private set; } = null!;

    public override void _Ready()
    {
        Prototype.BuildBattlefield(this, size: 160f, coverCount: 26, seed: 11);

        for (var i = 0; i < 8; i++)
        {
            var unit = Prototype.CreateTrooper($"Squad-{i + 1}", 0, new Color(0.25f, 0.5f, 0.95f));
            unit.Position = new Vector3(-7 + (i % 4) * 4, 0.1f, 30 + (i / 4) * 4);
            AddChild(unit);
        }
        for (var i = 0; i < 4; i++)
        {
            var dummy = Prototype.CreateTrooper($"Dummy-{i + 1}", 1, new Color(0.9f, 0.3f, 0.2f));
            dummy.Position = new Vector3(-6 + i * 4, 0.1f, -40);
            dummy.AutoEngageWhenIdle = false; // target practice, not a fight
            AddChild(dummy);
        }

        Director = new CameraDirector { Name = "CameraDirector" };
        AddChild(Director);
        Director.ShowStrategy(new Vector3(0, 0, 30), instant: true);

        var overlay = new CanvasLayer { Name = "Overlay" };
        Selection = new SquadSelection { Name = "Selection", PlayerTeam = 0 };
        overlay.AddChild(Selection);
        AddChild(overlay);
        Selection.SelectionChanged += UpdateReadout;
        Selection.OrderIssued += (_, _) => UpdateReadout();

        AddInfoPanel("LMB select / drag   RMB move   RMB on enemy attack   RMB on friendly FOLLOW (lab-defined)   Ctrl+RMB attack-move   Shift queue   H stop");
        UpdateReadout();
    }

    private void UpdateReadout()
    {
        var orders = Selection.Selected
            .Where(u => IsInstanceValid(u) && u.IsAlive)
            .GroupBy(u => u.Commands.Current?.Name ?? "idle")
            .Select(g => $"{g.Key} x{g.Count()}");
        SetReadout($"selected {Selection.Selected.Count}   orders: {string.Join(", ", orders)}");
    }

    public override void _PhysicsProcess(double delta) => UpdateReadout();
}

/// <summary>Right-clicking a friendly unit orders the selection to follow it.</summary>
public partial class SquadSelection : SelectionManager
{
    protected override UnitCommand? CreateUnitOrder(Unit unit, Unit clicked) =>
        clicked.Team == unit.Team
            ? (clicked == unit ? null : new FollowCommand(clicked))
            : base.CreateUnitOrder(unit, clicked);
}

/// <summary>
/// Game-defined order: keep within a few metres of a leader until it dies.
/// Built entirely on the public UnitCommand API.
/// </summary>
public sealed class FollowCommand(Unit leader, float distance = 3f) : UnitCommand
{
    public Unit Leader { get; } = leader;
    public override string Name => "Follow";
    public override Vector3? Destination => GodotObject.IsInstanceValid(Leader) ? Leader.GlobalPosition : null;

    public override bool Execute(Unit unit, double delta, ref UnitIntent intent)
    {
        if (!GodotObject.IsInstanceValid(Leader) || !Leader.IsAlive) return true;
        SteerTowards(unit, Leader.GlobalPosition, distance, ref intent);
        return false; // following never finishes on its own
    }
}
