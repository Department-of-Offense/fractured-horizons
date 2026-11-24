using Dof.Cameras;
using Dof.Core;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// Cameras with nothing else: no units, no combat. A drone flies through a
/// corridor, a low tunnel and a pillar field so every follow view and the
/// spring arm get exercised in tight spaces.
/// </summary>
public partial class CameraLab : Lab
{
    protected override string LabId => "camera";

    public CameraDirector Director { get; private set; } = null!;
    public Drone Drone { get; private set; } = null!;

    public override void _Ready()
    {
        LabWorld.Sky(this);
        LabWorld.Ground(this, 120f);
        var wall = new Color(0.45f, 0.42f, 0.38f);

        // Corridor: two long walls 3 m apart.
        LabWorld.Block(this, new Vector3(-1.5f, 2, -20), new Vector3(0.4f, 4, 30), wall);
        LabWorld.Block(this, new Vector3(1.5f, 2, -20), new Vector3(0.4f, 4, 30), wall);
        // Low tunnel: a roof over the corridor's far end.
        LabWorld.Block(this, new Vector3(0, 2.6f, -30), new Vector3(3.4f, 0.4f, 10), wall);
        // Pillar field.
        for (var x = -3; x <= 3; x++)
            for (var z = 0; z < 4; z++)
                LabWorld.Block(this, new Vector3(14 + x * 3, 3, -8 - z * 5), new Vector3(0.8f, 6, 0.8f), wall);

        Drone = new Drone { Name = "Drone", Position = new Vector3(0, 1.5f, 2) };
        AddChild(Drone);

        Director = new CameraDirector { Name = "CameraDirector" };
        AddChild(Director);
        Director.Follow(Drone, 1);
        Input.MouseMode = Input.MouseModeEnum.Captured;

        AddInfoPanel("WASD fly   Space/C up/down   mouse look   V cycle view   Tab strategy view (WASD pans)   wheel zoom");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            if (key.Keycode == Key.V && Director.Subject is not null) Director.CycleFollowView();
            if (key.Keycode == Key.Tab) ToggleStrategy();
        }
        if (Director.Subject is not null) Drone.HandleLook(@event);
    }

    public void ToggleStrategy()
    {
        if (Director.Subject is null)
        {
            Director.Follow(Drone, Director.FollowViewIndex);
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
        {
            Director.ShowStrategy(Drone.GlobalPosition, Drone.LookYaw);
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        Drone.Piloted = Director.Subject is not null;
    }

    public override void _Process(double delta)
    {
        var arm = Director.Active is FollowRig rig ? $"   arm {rig.ArmFraction:P0}" : "";
        SetReadout($"view: {Director.Active.Name}{arm}   drone {Drone.GlobalPosition:F1}");
    }
}

/// <summary>A free-flying camera subject. Not a unit: proves cameras only need ICameraSubject.</summary>
public partial class Drone : CharacterBody3D, ICameraSubject
{
    public float Speed { get; set; } = 8f;
    public bool Piloted { get; set; } = true;

    public Node3D Body => this;
    public float EyeHeight => 0.2f;
    public float LookYaw { get; set; }
    public float LookPitch { get; set; }

    private MeshInstance3D _mesh = null!;

    public override void _Ready()
    {
        CoreActions.EnsureDefaults();
        InputDefaults.Keys("lab_up", Key.Space);
        InputDefaults.Keys("lab_down", Key.C);

        AddChild(new CollisionShape3D { Shape = new SphereShape3D { Radius = 0.35f } });
        _mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(0.7f, 0.2f, 0.7f) },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.9f, 0.6f, 0.15f) },
        };
        AddChild(_mesh);
        MotionMode = MotionModeEnum.Floating;
    }

    public void HandleLook(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            LookYaw -= motion.Relative.X * 0.0025f;
            LookPitch = Mathf.Clamp(LookPitch - motion.Relative.Y * 0.0025f, -1.4f, 1.4f);
        }
    }

    public void SetBodyHidden(bool hidden) =>
        _mesh.CastShadow = hidden ? GeometryInstance3D.ShadowCastingSetting.ShadowsOnly : GeometryInstance3D.ShadowCastingSetting.On;

    public override void _PhysicsProcess(double delta)
    {
        if (!Piloted)
        {
            Velocity = Vector3.Zero;
            return;
        }
        var input = Input.GetVector(CoreActions.MoveLeft, CoreActions.MoveRight, CoreActions.MoveForward, CoreActions.MoveBack);
        var vertical = Input.GetAxis("lab_down", "lab_up");
        Velocity = (new Basis(Vector3.Up, LookYaw) * new Vector3(input.X, 0, input.Y) + Vector3.Up * vertical) * Speed;
        Rotation = new Vector3(0, LookYaw, 0);
        MoveAndSlide();
    }
}
