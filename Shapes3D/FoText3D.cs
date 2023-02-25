using BlazorThreeJS.Labels;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;

namespace FoundryBlazor.Shape;

public class FoText3D : FoGlyph3D
{

    private string text = "";
    public string Text { get { return this.text; } set { this.text = CreateDetails(AssignText(value, text)); } }
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

    protected string CreateDetails(string text)
    {
        Details = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        return text;
    }

    protected string AssignText(string newValue, string oldValue)
    {
        if (newValue != oldValue)
        {
            //ComputeResize = true;
        }

        return newValue;
    }   
     
    public FoText3D CreateTextAt(string text, double x, double y, double z)
    {
        Position = new FoVector3D(x, y, z);
        Text = text;
        return this;
    }
    public FoText3D CreateTextAt(string text, double x, double y, double z, string units)
    {
        Position = new FoVector3D(x, y, z) { units = units };
        Text = text;
        return this;
    }

    public override FoVector3D GetPosition()
    {
        if (Position == null) return base.GetPosition();

        var result = Position;
        return result;
    }
    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        var text = Text ?? "LabelText";
        //only in BlazorThreeJS
        var label = new LabelText(text)
        {
            Color = "Yellow",
            Position = GetPosition().AsVector3()
        };
        ctx.Add(label);
        return true;
    }


}