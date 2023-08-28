using BlazorThreeJS.Core;
using BlazorThreeJS.Labels;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shape;

public class FoText3D : FoGlyph3D, IShape3D
{

    private string text = "";
    private LabelText? Label { get; set; }
    public string Text { get { return this.text; } set { this.text = CreateDetails(AssignText(value, text)); } }
    public List<string>? Details { get; set; }

    public Vector3? Position { get; set; }

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
        Position = new Vector3(x, y, z);
        Text = text;
        return this;
    }


    public override Vector3 GetPosition(int x = 0, int y = 0, int z = 0)
    {
        if (Position == null) 
            return base.GetPosition(x,y,z);

        var result = Position;
        return result;
    }
    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        var text = Text ?? "LabelText";
        Label = new LabelText(text)
        {
            Color = Color ?? "Yellow",
            Position = GetPosition()
        };
        ctx.Add(Label);
        return true;
    }

    public bool UpdateText(string text)
    {
        Text = text;
        //"Update label text".WriteSuccess();
        if (Label != null)
        {
            Label.Text = Text;
            return true;
        }

        return false;
    }

    public override bool UpdateMeshPosition(double xLoc, double yLoc, double zLoc)
    {
        //"Update label position".WriteSuccess();
        if (Label != null)
        {
            Label.Position.Set(xLoc, yLoc, zLoc);
            return true;
        }

        return false;
    }

}