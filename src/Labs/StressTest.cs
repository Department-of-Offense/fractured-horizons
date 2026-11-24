using System.Linq;
using Dof.Hybrid;
using Dof.Prototyping;
using Dof.UI;
using Dof.Units;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// Every system at scale: two armies attack-move through each other. Watch
/// frame and physics times as unit counts grow. Override the count with
/// `-- --units=200` (per side).
/// </summary>
public partial class StressTest : Lab
{
    protected override string LabId => "stress";

    [Export] public int UnitsPerSide { get; set; } = 120;

    public GameModeManager Modes { get; private set; } = null!;

    public override void _Ready()
    {
        foreach (var arg in OS.GetCmdlineUserArgs())
            if (arg.StartsWith("--units=") && int.TryParse(arg["--units=".Length..], out var n)) UnitsPerSide = n;

        Prototype.BuildBattlefield(this, size: 260f, coverCount: 40, seed: 3);
        Spawn(0, new Color(0.25f, 0.5f, 0.95f), z: 70f, faceYaw: 0f);
        Spawn(1, new Color(0.9f, 0.3f, 0.2f), z: -70f, faceYaw: Mathf.Pi);

        Modes = new GameModeManager { Name = "GameModes", PlayerTeam = 0 };
        AddChild(Modes);
        var hud = new CanvasLayer { Name = "Hud" };
        hud.AddChild(new CommandHud { ShowHealthBars = UnitsPerSide <= 60 });
        AddChild(hud);

        // Both armies advance through each other's start line.
        Callable.From(() =>
        {
            foreach (var unit in UnitRegistry.All(GetTree()))
                unit.Issue(new AttackMoveCommand(new Vector3(unit.GlobalPosition.X, 0, -unit.GlobalPosition.Z)));
        }).CallDeferred();

        AddInfoPanel($"{UnitsPerSide} per side. Command or pilot as in Skirmish. Launch with -- --units=N to change.");
    }

    private void Spawn(int team, Color color, float z, float faceYaw)
    {
        var columns = Mathf.CeilToInt(Mathf.Sqrt(UnitsPerSide * 2));
        for (var i = 0; i < UnitsPerSide; i++)
        {
            var unit = Prototype.CreateTrooper($"{(team == 0 ? "Blue" : "Red")}-{i + 1}", team, color);
            unit.Position = new Vector3((i % columns - columns / 2f) * 2.5f, 0.1f, z + (i / columns) * 2.5f * Mathf.Sign(z));
            unit.Rotation = new Vector3(0, faceYaw, 0);
            AddChild(unit);
        }
    }

    public override void _Process(double delta)
    {
        var blue = UnitRegistry.OnTeam(GetTree(), 0).Count();
        var red = UnitRegistry.OnTeam(GetTree(), 1).Count();
        var fps = Performance.GetMonitor(Performance.Monitor.TimeFps);
        var frame = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000;
        var physics = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000;
        SetReadout($"blue {blue}   red {red}   {fps:0} fps   process {frame:0.0} ms   physics {physics:0.0} ms");
    }
}
