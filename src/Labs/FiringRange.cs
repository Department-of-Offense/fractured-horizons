using Dof.Cameras;
using Dof.Combat;
using Dof.Prototyping;
using Dof.Units;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// Units + combat + cameras without the hybrid glue: the lab wires piloting
/// by hand (BeginPilot + CameraDirector.Follow), the way a pure shooter
/// would. Targets are not units; they only implement ICombatant.
/// </summary>
public partial class FiringRange : Lab
{
    protected override string LabId => "range";

    public Unit Shooter { get; private set; } = null!;
    public PilotController Pilot { get; private set; } = null!;
    public CameraDirector Director { get; private set; } = null!;
    public int Shots { get; private set; }
    public int Hits { get; private set; }
    public int Kills { get; private set; }

    public override void _Ready()
    {
        Prototype.BuildBattlefield(this, size: 140f, coverCount: 0);

        Shooter = Prototype.CreateTrooper("Shooter", 0, new Color(0.25f, 0.5f, 0.95f));
        Shooter.Position = new Vector3(0, 0.1f, 0);
        AddChild(Shooter);

        // Lanes at 10-40 m, slightly staggered.
        for (var lane = 0; lane < 4; lane++)
        {
            var distance = 10f + lane * 10f;
            for (var i = -1; i <= 1; i++)
            {
                var target = new RangeTarget { Name = $"Target-{distance}m-{i + 1}", Position = new Vector3(i * 4f + lane, 1f, -distance) };
                target.Hit += () => Hits++;
                target.Destroyed += () => Kills++;
                AddChild(target);
            }
        }

        Director = new CameraDirector { Name = "CameraDirector" };
        AddChild(Director);

        Pilot = new PilotController { Camera = Director.Camera };
        Shooter.BeginPilot(Pilot);
        Shooter.Weapon!.Fired += (_, _, _) => Shots++;
        ShowView(1);
        Input.MouseMode = Input.MouseModeEnum.Captured;

        AddInfoPanel("WASD move   mouse aim   LMB fire   Space jump   V cycle view");
        var overlay = new CanvasLayer { Name = "Overlay" };
        overlay.AddChild(new Crosshair());
        AddChild(overlay);
    }

    public void ShowView(int index)
    {
        var rig = Director.Follow(new UnitSubject(Shooter, Pilot), index);
        Pilot.BodyFollowsLook = !rig.FreeLook;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.V }) ShowView(Director.FollowViewIndex + 1);
        Pilot.HandleInput(@event);
    }

    public override void _Process(double delta)
    {
        var accuracy = Shots == 0 ? 0f : (float)Hits / Shots;
        SetReadout($"view: {Director.Active.Name}   shots {Shots}   hits {Hits}   accuracy {accuracy:P0}   targets destroyed {Kills}");
    }

    /// <summary>
    /// Ten lines a game writes to follow its own units with dof_cameras. dof_hybrid
    /// ships an equivalent (PilotCameraSubject) for games that use it.
    /// </summary>
    private sealed class UnitSubject(Unit unit, PilotController pilot) : ICameraSubject
    {
        public Node3D Body => unit;
        public float EyeHeight => unit.EyeHeight;
        public float LookYaw => pilot.LookYaw;
        public float LookPitch => pilot.LookPitch;
        public void SetBodyHidden(bool hidden) => unit.SetBodyHidden(hidden);
    }
}

/// <summary>Centre-screen crosshair.</summary>
public partial class Crosshair : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.FullRect);
    }

    public override void _Draw()
    {
        var c = GetViewportRect().Size / 2f;
        var color = new Color(1, 1, 1, 0.9f);
        foreach (var d in new[] { Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down })
            DrawLine(c + d * 3, c + d * 10, color, 2);
    }
}

/// <summary>A destructible target that pops back up. Any node can be a combatant.</summary>
public partial class RangeTarget : StaticBody3D, ICombatant
{
    public event System.Action? Hit;
    public event System.Action? Destroyed;

    [Export] public float RespawnSeconds { get; set; } = 2f;

    public int Team => Teams.Neutral;
    public Health Health { get; private set; } = null!;

    private StandardMaterial3D _material = null!;
    private static readonly Color Fresh = new(0.95f, 0.95f, 0.9f);

    public override void _Ready()
    {
        var size = new Vector3(1f, 2f, 0.3f);
        AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        _material = new StandardMaterial3D { AlbedoColor = Fresh };
        AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = size }, MaterialOverride = _material });

        Health = new Health { Name = "Health", MaxHealth = 24f };
        AddChild(Health);
        Health.Damaged += (_, _) =>
        {
            Hit?.Invoke();
            _material.AlbedoColor = Fresh.Lerp(new Color(1f, 0.3f, 0.2f), 1f - Health.Fraction);
        };
        Health.Died += _ => OnDestroyed();
    }

    private void OnDestroyed()
    {
        Destroyed?.Invoke();
        Visible = false;
        CollisionLayer = 0;
        GetTree().CreateTimer(RespawnSeconds, processAlways: false, processInPhysics: true).Timeout += () =>
        {
            Health.Reset();
            _material.AlbedoColor = Fresh;
            Visible = true;
            CollisionLayer = 1;
        };
    }
}
