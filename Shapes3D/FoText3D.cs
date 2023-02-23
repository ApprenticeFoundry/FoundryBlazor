using BlazorThreeJS.Labels;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;

namespace FoundryBlazor.Shape;

public class FoText3D : FoGlyph3D
{

    public string? Text { get; set; }
    public List<string>? Details { get; set; }
    public FoVector3D? Position { get; set; }

    public FoText3D() : base()
    {
    }
    public FoText3D(string name) : base(name)
    {
    }
    public FoText3D(string name, string color) : base(name, color)
    {
    }
    public FoText3D CreateTextAt(string text, double x, double y, double z)
    {
        Position = new FoVector3D(x, y, z);
        Text = text;
        return this;
    }
    // public override BufferGeometry GetGeometry()
    // {
    //     if (Label == null) return base.GetGeometry();

    //     var box = Label.boundingBox;
    //     var result = new BoxGeometry((float)box.width, (float)box.height, (float)box.depth);

    //     return result;
    // }

    public override FoVector3D GetPosition()
    {
        if (Position == null) return base.GetPosition();

        var result = Position;
        return result;
    }
    public override void Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        var text = Text ?? "LabelText";
        //only in BlazorThreeJS
        var label = new LabelText(text)
        {
            Color = "Yellow",
            Position = GetPosition().AsVector3()
        };
        ctx.Add(label);
    }

    internal FoText3D CreateTextAt(string text, double xLoc, double yLoc, double zLoc, string units)
    {
        throw new NotImplementedException();
    }
}