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
    private TextPanel? Panel { get; set; }

    public string DisplayText()
    {
        return Name;
    }

    public FoPanel3D() { }
    public FoPanel3D(string name) : base(name, "Grey")
    {
    }

    public List<FoPanel3D> Panels()
    {
        return Members<FoPanel3D>().ToList();
    }

    public List<FoPathway3D> Connections()
    {
        return Members<FoPathway3D>().ToList();
    }

    public virtual FoPanel3D Clear()
    {
        TextLines.Clear();
        return this;
    }

    public TextPanel EstablishPanel3D()
    {
        if (Panel != null) return Panel;

        Panel = new TextPanel()
        {
            TextLines = TextLines,
            Height = Height,
            Width = Width,
            Color = Color,
            Position = Position ?? new Vector3(0, 0, 0),
            Pivot = Pivot ?? new Vector3(0, 0, 0),
            Rotation = Rotation ?? new Euler(0, 0, 0),
        };
        return Panel;
    }


    private List<TextPanel> ChildPanels()
    {
        return Panels().Select((item) => item.EstablishPanel3D()).ToList();
    }

    private List<Mesh> ChildMeshes()
    {
        return Connections().Select((item) => item.EstablishPathway3D()).ToList();
    }

    public bool PanelGroup3D(Scene ctx)
    {
        var panel = new PanelGroup()
        {
            TextLines = TextLines,
            Height = Height,
            Width = Width,
            Color = Color,
            Position = Position ?? new Vector3(0, 0, 0),
            Pivot = Pivot ?? new Vector3(0, 0, 0),
            Rotation = Rotation ?? new Euler(0, 0, 0),
            TextPanels = ChildPanels(),
            Meshes = ChildMeshes()
        };

        ctx.Add(panel);
        return true;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        $"RenderPanel {Name} {Position?.X} {Position?.Y}  {Position?.Z}".WriteNote();
        // foreach (var connection in Connections())
        // {
        //     connection.Render(ctx, tick, fps, deep);
        // }
        var result = PanelGroup3D(ctx);
        return result;
    }

}
