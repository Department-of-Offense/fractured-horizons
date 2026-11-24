using Godot;

namespace FracturedHorizons.Labs;

/// <summary>
/// Plain-Godot scenery for labs that must not depend on dof_prototyping
/// (e.g. the camera lab proves cameras work with nothing else vendored).
/// </summary>
public static class LabWorld
{
    public static void Sky(Node3D root)
    {
        root.AddChild(new WorldEnvironment
        {
            Environment = new Environment
            {
                BackgroundMode = Environment.BGMode.Sky,
                Sky = new Sky { SkyMaterial = new ProceduralSkyMaterial() },
                AmbientLightSource = Environment.AmbientSource.Sky,
                TonemapMode = Environment.ToneMapper.Filmic,
            },
        });
        root.AddChild(new DirectionalLight3D { ShadowEnabled = true, RotationDegrees = new Vector3(-50, -30, 0) });
    }

    public static StaticBody3D Block(Node3D root, Vector3 center, Vector3 size, Color color)
    {
        var body = new StaticBody3D { Position = center };
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        body.AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.85f },
        });
        root.AddChild(body);
        return body;
    }

    public static void Ground(Node3D root, float size) =>
        Block(root, new Vector3(0, -0.5f, 0), new Vector3(size, 1, size), new Color(0.32f, 0.34f, 0.3f));
}
