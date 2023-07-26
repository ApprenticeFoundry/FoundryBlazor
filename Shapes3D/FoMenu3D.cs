

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
        // var x = 0;
        // var y = 18;
        // Members<FoButton3D>().ForEach(item =>
        // {
        //     item.MoveTo(x, y);
        //     item.ResizeTo(width, height);
        //     x += width+5;
        // });
        // _layout = "H";
        // ResizeTo(x, height+20);
        return this;
    }

    public FoMenu3D LayoutVertical(int width = 95, int height = 40)
    {
        // var x = 3;
        // var y = 18;
        // Members<FoButton3D>().ForEach(item =>
        // {
        //     //item.MoveTo(x, y);
        //     //item.ResizeTo(width, height);
        //     y += height + 10;
        // });
        // _layout = "V";
        // //ResizeTo(width+5, y);
        return this;
    }

    public bool Menu3D(Scene ctx)
    {
        var buttons = new List<Button>() {
            new Button("BTN1", "Button 1") {
                OnClick = (Button btn)=>Console.WriteLine("Clicked Button1")
            },
            new Button("BTN2","Button 2"){
                OnClick = (Button btn)=>Console.WriteLine("Clicked Button2")
            },
            new Button("BTN3","Button 3")
        };
        var menu = new Menu
        {
            Buttons = buttons
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
