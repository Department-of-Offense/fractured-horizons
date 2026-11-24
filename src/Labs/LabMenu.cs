using System.Linq;
using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// Entry point: pick a lab. `godot --path . -- --lab=range` jumps straight
/// into one (ids are in <see cref="LabCatalog"/>).
/// </summary>
public partial class LabMenu : Control
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        var jump = OS.GetCmdlineUserArgs().FirstOrDefault(a => a.StartsWith("--lab="))?["--lab=".Length..];
        if (jump is not null && LabCatalog.All.FirstOrDefault(l => l.Id == jump) is { } lab)
        {
            Callable.From(() => GetTree().ChangeSceneToFile(lab.Scene)).CallDeferred();
            return;
        }

        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var background = new ColorRect { Color = new Color(0.07f, 0.08f, 0.1f) };
        AddChild(background);
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var column = new VBoxContainer { AnchorLeft = 0.5f, AnchorRight = 0.5f, OffsetLeft = -380, OffsetRight = 380, OffsetTop = 60 };
        column.AddThemeConstantOverride("separation", 14);
        AddChild(column);

        var title = new Label { Text = "FRACTURED HORIZONS", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 34);
        column.AddChild(title);
        column.AddChild(new Label
        {
            Text = "Systems lab: proving ground for the reusable godot-systems modules",
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(1, 1, 1, 0.7f),
        });

        foreach (var info in LabCatalog.All)
        {
            var button = new Button
            {
                Text = $"{info.Title}    [{info.Systems}]\n{info.Purpose}",
                Alignment = HorizontalAlignment.Left,
                CustomMinimumSize = new Vector2(0, 64),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            var scene = info.Scene;
            button.Pressed += () => GetTree().ChangeSceneToFile(scene);
            column.AddChild(button);
        }

        column.AddChild(new Label { Text = "Systems: " + SystemsVersion(), Modulate = new Color(1, 1, 1, 0.5f) });
    }

    private static string SystemsVersion()
    {
        using var file = FileAccess.Open("res://SYSTEMS_VERSION", FileAccess.ModeFlags.Read);
        return file is null ? "unknown" : file.GetLine();
    }
}
