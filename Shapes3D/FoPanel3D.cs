
using System.Linq;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using BlazorThreeJS.Menus;

namespace FoundryBlazor.Shape;

public class FoPanel3D : FoGlyph3D, IShape3D
{
    public Vector3? Position { get; set; }
    public List<string> TextLines { get; set; } = new();

    public string DisplayText()
    {
        return Name;
    }

    public FoPanel3D(string name) : base(name, "Grey")
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
    }



    public virtual FoPanel3D Clear()
    {
        TextLines.Clear();
        return this;
    }

    public bool TextPanel(Scene ctx)
    {

        var panel = new TextPanel
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

        var result = TextPanel(ctx);
        return result;
    }

}
