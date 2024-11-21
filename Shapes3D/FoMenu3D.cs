
using System.Linq;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using BlazorThreeJS.Menus;

namespace FoundryBlazor.Shape;

public class FoMenu3D : FoPanel3D, IFoMenu
{


    public List<IFoButton> Buttons()
    {
        return GetMembers<FoButton3D>()?.Select(item => item as IFoButton).ToList() ?? new List<IFoButton>();
    }

    public FoMenu3D(string name) : base(name)
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
    }



    public override FoMenu3D Clear()
    {
        GetMembers<FoButton3D>()?.Clear();
        return this;
    }


    public FoMenu3D LayoutHorizontal(int width = 95, int height = 40)
    {
        return this;
    }

    public FoMenu3D LayoutVertical(int width = 95, int height = 40)
    {
        return this;
    }

    public bool DrawMenu3D(Scene scene)
    {
        var buttons = Buttons().Select((item) =>
        {
            var text = item.DisplayText();
            var button = new Button(text, text)
            {
                OnClick = (Button btn) => Console.WriteLine("Clicked Button1")
            };
            return button;
        }).ToList();

        Width = 1;
        Height = buttons.Count * 0.22;
        var menu = new PanelMenu()
        {
            Buttons = buttons,
            Height = Height,
            Width = Width,
            Position = Position ?? new Vector3(0, 0, 0),
        };

        scene.AddChild(menu);
        return true;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        var result = DrawMenu3D(ctx);
        return result;
    }

}
