using System.Linq;
using Dof.Hybrid;
using Dof.Prototyping;
using Dof.UI;
using Dof.Units;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// First playable slice: hold the sector against a hostile push. Your squad
/// starts at the south end; after a short delay the hostiles attack-move
/// toward you. Win by wiping them out, lose if your squad falls.
/// Greybox only: units and terrain come from the engine's prototyping kit
/// until real art exists.
/// </summary>
public partial class Skirmish : Lab
{
    protected override string LabId => "skirmish";

    public const int PlayerTeam = 0;
    public const int HostileTeam = 1;

    [Export] public int SquadSize { get; set; } = 5;
    [Export] public int HostileCount { get; set; } = 7;
    [Export] public double HostileAdvanceDelay { get; set; } = 8.0;

    public enum Outcome { InProgress, Victory, Defeat }
    public Outcome State { get; private set; } = Outcome.InProgress;
    public GameModeManager Modes { get; private set; } = null!;

    private Label _status = null!;
    private double _clock;
    private bool _hostilesAdvancing;

    public override void _Ready()
    {
        Prototype.BuildBattlefield(this, size: 180f, coverCount: 30, seed: 42);

        for (var i = 0; i < SquadSize; i++)
            Spawn($"Alpha-{i + 1}", PlayerTeam, new Color(0.25f, 0.5f, 0.95f), new Vector3(-8 + i * 4, 0.1f, 45), 0f);

        for (var i = 0; i < HostileCount; i++)
            Spawn($"Hostile-{i + 1}", HostileTeam, new Color(0.9f, 0.3f, 0.2f), new Vector3(-12 + i * 4, 0.1f, -50), Mathf.Pi);

        Modes = new GameModeManager { Name = "GameModes", PlayerTeam = PlayerTeam };
        AddChild(Modes);
        AddInfoPanel("Command your squad, press F to pilot a selected unit, F/Esc to return. Survive the push.");

        var hud = new CanvasLayer { Name = "SkirmishHud" };
        _status = new Label
        {
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            OffsetLeft = -300,
            OffsetRight = 300,
            OffsetTop = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _status.AddThemeFontSizeOverride("font_size", 22);
        hud.AddChild(new CommandHud());
        hud.AddChild(_status);
        AddChild(hud);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (State != Outcome.InProgress) return;
        _clock += delta;

        if (!_hostilesAdvancing && _clock >= HostileAdvanceDelay)
        {
            _hostilesAdvancing = true;
            foreach (var hostile in UnitRegistry.OnTeam(GetTree(), HostileTeam))
                hostile.Issue(new AttackMoveCommand(new Vector3(hostile.GlobalPosition.X, 0, 45)));
        }

        var friends = UnitRegistry.OnTeam(GetTree(), PlayerTeam).Count();
        var hostiles = UnitRegistry.OnTeam(GetTree(), HostileTeam).Count();

        if (hostiles == 0) State = Outcome.Victory;
        else if (friends == 0) State = Outcome.Defeat;

        _status.Text = State switch
        {
            Outcome.Victory => "SECTOR SECURED",
            Outcome.Defeat => "SQUAD LOST",
            _ when !_hostilesAdvancing => $"Hostiles advancing in {HostileAdvanceDelay - _clock:0}s   Squad {friends}   Hostiles {hostiles}",
            _ => $"Hold the sector   Squad {friends}   Hostiles {hostiles}",
        };
    }

    private void Spawn(string name, int team, Color color, Vector3 position, float yaw)
    {
        var unit = Prototype.CreateTrooper(name, team, color);
        unit.Position = position;
        unit.Rotation = new Vector3(0, yaw, 0);
        AddChild(unit);
    }
}
