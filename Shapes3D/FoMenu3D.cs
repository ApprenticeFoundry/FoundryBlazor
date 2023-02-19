
using IoBTMessage.Models;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoMenu3D : FoGlyph3D, IFoMenu
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

    public FoMenu3D(string name) : base(name,"Grey")
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
        if ( _layout.Matches("V"))
            LayoutHorizontal();
        else
            LayoutVertical();
            
        return this;
    }

    public FoMenu3D LayoutHorizontal(int width=95, int height=40)
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

    public FoMenu3D LayoutVertical(int width=95, int height=40)
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




}
