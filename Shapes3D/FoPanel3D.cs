using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Menus;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoPanel3D : FoGlyph3D, IShape3D
{
    public Vector3? Position { get; set; }
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
        };

        ctx.Add(panel);
        return true;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        $"RenderPanel {Name} {Position?.X} {Position?.Y}  {Position?.Z}".WriteNote();
        foreach (var panel in Panels())
        {
            panel.Render(ctx, tick, fps, true);
        }
        var result = DrawPanel3D(ctx);
        return result;
    }

}
