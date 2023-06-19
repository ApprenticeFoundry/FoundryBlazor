
using System.Drawing;
using System.Linq;

using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;



public class FoScaledPage2D : FoScaledGlyph2D, IFoPage2D
{


    public FoScaledPage2D(string name) : base(name)
    {

    }

    public int DrawingHeight()
    {
        throw new NotImplementedException();
    }

    public int DrawingMargin()
    {
        throw new NotImplementedException();
    }

    public int DrawingWidth()
    {
        throw new NotImplementedException();
    }

    public void SetScale(ScaledCanvas scale)
    {
        throw new NotImplementedException();
    }
}
