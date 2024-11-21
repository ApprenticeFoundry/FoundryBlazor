using BlazorThreeJS.Core;
using BlazorThreeJS.Labels;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shape;

public class FoText3D : FoGlyph3D, IShape3D
{

    private LabelText? Label { get; set; }

    public double FontSize { get; set; } = 0.5;


    private string _text = "";
    public string Text
    {
        get { return this._text; }
        set { this._text = CreateDetails(AssignText(value, _text)); }
    }

    public List<string>? Details { get; set; }

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
            return base.GetPosition(x, y, z);

        var result = Position;
        return result;
    }

    public override string GetTreeNodeTitle()
    {
        return $"{base.GetTreeNodeTitle()} {Text}";
    }

    public override bool Render(Scene scene, int tick, double fps, bool deep = true)
    {
        var text = Text ?? "LabelText";
        Label = new LabelText(text)
        {
            Uuid = GetGlyphId(),
            Name = GetName(),
            Color = Color ?? "Yellow",
            Position = GetPosition(),
            FontSize = FontSize,
        };
        scene.AddChild(Label);

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