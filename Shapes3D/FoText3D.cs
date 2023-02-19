using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Labels;
using IoBTMessage.Models;

namespace FoundryBlazor.Shape;

public class FoText3D : FoGlyph3D
{

    public UDTO_Label? Label { get; set; }

    public FoText3D(string name) : base(name)
    {
    }
    public FoText3D(string name, string color) : base(name, color)
    {
    }

    // public override BufferGeometry GetGeometry()
    // {
    //     if (Label == null) return base.GetGeometry();

    //     var box = Label.boundingBox;
    //     var result = new BoxGeometry((float)box.width, (float)box.height, (float)box.depth);

    //     return result;
    // }

    public override Vector3 GetPosition()
    {
        if (Label == null) return base.GetPosition();

        var pos = Label.position;
        var result = new Vector3((float)pos.xLoc, (float)pos.yLoc, (float)pos.zLoc);
        return result;
    }
    public override void Render(Viewer viewer, Scene ctx, int tick, double fps, bool deep = true)
    {
        var text = Label?.text ?? "Our Canvas Text";
        //only in BlazorThreeJS
        var label = new LabelText(text) { Color = "Yellow", Position = GetPosition() };
        ctx.Add(label);
    }
}