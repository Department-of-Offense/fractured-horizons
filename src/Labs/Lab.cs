using Dof.Debug;
using Dof.UnitDebug;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>A lab in the menu: which scene, which systems it exercises, and why it exists.</summary>
public sealed record LabInfo(string Id, string Title, string Scene, string Systems, string Purpose);

/// <summary>
/// Every lab, in menu order. Add an entry here when you add a lab (see docs/LABS.md).
/// </summary>
public static class LabCatalog
{
    public static readonly LabInfo[] All =
    {
        new("camera", "Camera Lab", "res://scenes/labs/CameraLab.tscn",
            "cameras",
            "Fly a drone through corridors and pillars. Cameras with no units at all; spring arm in tight spaces; every view."),
        new("range", "Firing Range", "res://scenes/labs/FiringRange.tscn",
            "units, combat, cameras",
            "Pilot a trooper and shoot respawning targets. Piloting without the strategy layer; damage on non-unit targets."),
        new("squad", "Squad Command", "res://scenes/labs/SquadCommand.tscn",
            "command, units, cameras",
            "RTS control only. Right-click a friendly unit to make the squad follow it: a game-defined order and click rule."),
        new("skirmish", "Skirmish", "res://scenes/labs/Skirmish.tscn",
            "hybrid, ui, command, units, combat, cameras",
            "Hold the sector. The full command-to-pilot loop, with win and lose conditions."),
        new("stress", "Stress Test", "res://scenes/labs/StressTest.tscn",
            "all, at scale",
            "Two armies of 100+ attack-moving at once. Watch frame and physics times; pilot a unit in the chaos."),
    };
}

/// <summary>
/// Base for lab scenes: F1 returns to the menu, F3 opens the debug overlay,
/// and an info panel says what the lab is testing. Labs use the systems the way a real game would, so
/// keep lab code small. If a lab needs a lot of code, that code probably
/// belongs in godot-systems.
/// </summary>
public abstract partial class Lab : Node3D
{
    public const string MenuScene = "res://scenes/Main.tscn";

    protected abstract string LabId { get; }

    private Label? _readout;

    /// <summary>The F3 overlay every lab gets. Labs can add their own sections to it.</summary>
    protected DebugOverlay Debug { get; } = new() { Name = "DebugOverlay" };

    public override void _EnterTree()
    {
        if (Debug.GetParent() is not null) return;
        Debug.AddSection(new UnitDebugSection());
        AddChild(Debug);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F1 })
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().ChangeSceneToFile(MenuScene);
        }
    }

    /// <summary>Bottom-left panel: lab title, systems under test, controls, live readout.</summary>
    protected void AddInfoPanel(string controls)
    {
        var info = System.Array.Find(LabCatalog.All, l => l.Id == LabId);
        var layer = new CanvasLayer { Name = "LabInfo", Layer = 5 };
        var box = new VBoxContainer
        {
            AnchorTop = 1, AnchorBottom = 1, OffsetLeft = 16, OffsetTop = -150, OffsetBottom = -12,
            GrowVertical = Control.GrowDirection.Begin,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        var title = new Label { Text = $"{info?.Title ?? LabId}   [systems: {info?.Systems}]   F1 lab menu   F3 debug" };
        title.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
        box.AddChild(title);
        box.AddChild(new Label { Text = controls, Modulate = new Color(1, 1, 1, 0.85f) });
        _readout = new Label { Modulate = new Color(0.7f, 1f, 0.8f) };
        box.AddChild(_readout);
        layer.AddChild(box);
        AddChild(layer);
    }

    /// <summary>Live numbers shown under the controls (call every frame if you like).</summary>
    protected void SetReadout(string text)
    {
        if (_readout is not null) _readout.Text = text;
    }
}
