using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Menus;
using FoundryBlazor.Extensions;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Core;

namespace FoundryBlazor.Shape;

public class FoPanel3D : FoGlyph3D, IShape3D
{

    public List<string> TextLines { get; set; } = new();
    private TextPanel? TextPanel { get; set; }
    private PanelGroup? PanelGroup { get; set; }

    public string DisplayText()
    {
        return Key;
    }

    public FoPanel3D() { }
    public FoPanel3D(string name) : base(name, "Grey")
    {
    }

    public bool HasPanels()
    {
        return GetMembers<FoPanel3D>()?.Count > 0;
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
        Members<FoPanel3D>().Clear();
        Members<FoPathway3D>().Clear();
        return this;
    }

    public TextPanel EstablishPanel3D()
    {
        if (TextPanel != null) return TextPanel;

        TextPanel = new TextPanel()
        {
            TextLines = TextLines,
            Height = Height,
            Width = Width,
            Color = Color,
            Position = Position ?? new Vector3(0, 0, 0),
            Pivot = Pivot ?? new Vector3(0, 0, 0),
            Rotation = Rotation ?? new Euler(0, 0, 0),
        };
        return TextPanel;
    }

    public PanelGroup EstablishGroup3D()
    {
        if (PanelGroup != null) return PanelGroup;

        PanelGroup = new PanelGroup()
        {
            TextLines = TextLines,
            Height = Height,
            Width = Width,
            Color = Color,
            Position = Position ?? new Vector3(0, 0, 0),
            Pivot = Pivot ?? new Vector3(0, 0, 0),
            Rotation = Rotation ?? new Euler(0, 0, 0),
            TextPanels = ChildPanels(),
            Meshes = ChildConnections()
        };
        return PanelGroup;
    }


    private List<TextPanel> ChildPanels()
    {
        return Panels().Select((item) => item.EstablishPanel3D()).ToList();
    }

    private List<Object3D> ChildConnections()
    {
        return Connections().Select((item) => item.EstablishPathway3D()).ToList();
    }

    public bool UpdateTextLines(List<string> lines)
    {
        TextLines = lines;
        //"Update label text".WriteSuccess();
        if (TextPanel != null)
        {
            TextPanel.TextLines = TextLines;
            return true;
        }

        return false;
    }



    public override bool Render(Scene scene, int tick, double fps, bool deep = true)
    {
        //$"RenderPanel {Name} {Position?.X} {Position?.Y}  {Position?.Z}".WriteNote();

        if (IsVisible)
        {
            TextPanel = EstablishPanel3D();
            scene.AddChild(TextPanel);
        }
        else
        {
            if (TextPanel != null)
                scene.RemoveChild(TextPanel);
            TextPanel = null;
        }

        if (HasPanels())
        {
            PanelGroup = EstablishGroup3D();
            scene.AddChild(PanelGroup);
        }
        else
        {
            if (PanelGroup != null)
                scene.RemoveChild(PanelGroup);
            PanelGroup = null;
        }
        return true;
    }

}
