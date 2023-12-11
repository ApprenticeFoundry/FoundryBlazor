
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;


public class FoPathway3D : FoGlyph3D, IPipe3D
{
    public List<Vector3> Path { get; set; } = new();
    public Vector3? Position { get; set; }
    public Vector3? Pivot { get; set; }
    public Euler? Rotation { get; set; }
    public double Radius { get; set; } = 0.025;
    private Mesh? Tube { get; set; }

    public FoPathway3D(string name) : base(name, "Grey")
    {
        Color = "pink";
    }

    public Mesh EstablishPathway3D()
    {
        if (Tube != null) return Tube;

        Tube = new Mesh
        {
            Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: Radius, path: Path),
            Material = new MeshStandardMaterial()
            {
                Color = Color
            }
        };
        return Tube;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        $"RenderPathway {Key}".WriteNote();
        ctx.Add(EstablishPathway3D());
        return true;
    }

}