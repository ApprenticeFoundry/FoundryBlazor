

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

public class FoMenu3D : FoGlyph3D, IFoMenu, IShape3D
{
    private string _layout = "H";

    public string DisplayText()
    {
        return Name;
    }

    public List<IFoButton> Buttons()
    {
        return GetMembers<FoButton3D>()?.Select(item => item as IFoButton).ToList() ?? new List<IFoButton>();
    }

    public FoMenu3D(string name) : base(name, "Grey")
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
    }



    public FoMenu3D Clear()
    {
        GetMembers<FoButton3D>()?.Clear();
        return this;
    }

    public FoMenu3D ToggleLayout()
    {
        if (_layout.Matches("V"))
            LayoutHorizontal();
        else
            LayoutVertical();

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

    public bool Menu3D(Scene ctx)
    {
        var buttons = new List<Button>();
        GetSlot<FoButton3D>()?.ForEach((item) =>
        {
            var button = new Button(item.Name, item.Name)
            {
                OnClick = (Button btn) => Console.WriteLine("Clicked Button1")
            };
            buttons.Add(button);
        });

        var height = buttons.Count * 0.22;
        var menu = new PanelMenu
        {
            Buttons = buttons,
            Height = height,
            Width = 1
        };

        ctx.Add(menu);
        return true;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {

        var result = Menu3D(ctx);
        return result;
    }

}
