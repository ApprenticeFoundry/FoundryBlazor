using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Menus;
using FoundryBlazor.Extensions;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;

namespace FoundryBlazor.Shape;

public class FoPanel3D : FoGlyph3D, IShape3D
{
    public Vector3? Position { get; set; }
    public Vector3? Pivot { get; set; }
    public Euler? Rotation { get; set; }
    public List<string> TextLines { get; set; } = new();

    public string DisplayText()
    {
        return Name;
    }

    //public FoPanel3D() : base() { }

    public FoPanel3D(string name) : base(name, "Grey")
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
    }


    public List<FoPanel3D> Panels()
    {
        return Members<FoPanel3D>().ToList();
    }

    public List<FoShape1D> Connections()
    {
        return Members<FoShape1D>().ToList();
    }

    public virtual FoPanel3D Clear()
    {
        TextLines.Clear();
        return this;
    }

    public bool DrawPanel3D(Scene ctx)
    {

        var panel = new TextPanel()
        {
            TextLines = TextLines,
            Height = Height,
            Width = Width,
            Color = Color,
            Position = Position ?? new Vector3(0, 0, 0),
            Pivot = Pivot ?? new Vector3(0, 0, 0),
            Rotation = Rotation ?? new Euler(0, 0, 0),
        };

        ctx.Add(panel);
        return true;
    }

    public bool Draw3DConnections(Scene ctx)
    {
        var radius = 0.15f;
        int pixels = 100;

        var z = -3;
        foreach (var connection in Connections())
        {

            var X1 = connection.StartX / pixels;
            var Y1 = connection.StartY / pixels;
            var X2 = connection.FinishX / pixels;
            var Y2 = connection.FinishY / pixels;

            var positions = new List<Vector3>() {
                new Vector3(X1, Y1, z),
                new Vector3(X2, Y2, z)
            };

            var tube = new Mesh
            {
                Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: radius, path: positions),
                Position = new Vector3(0, 0, 0),
                Material = new MeshStandardMaterial()
                {
                    Color = "yellow"
                }
            };
            ctx.Add(tube);
        }

        return true;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        $"RenderPanel {Name} {Position?.X} {Position?.Y}  {Position?.Z}".WriteNote();
        foreach (var panel in Panels())
        {
            panel.Render(ctx, tick, fps, true);
        }
        Draw3DConnections(ctx);
        var result = DrawPanel3D(ctx);
        return result;
    }

}
