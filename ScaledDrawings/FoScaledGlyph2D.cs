

using Blazor.Extensions.Canvas.Canvas2D;
using IoBTMessage.Units;
using System.Drawing;

namespace FoundryBlazor.Shape;

public class FoScaledGlyph2D : FoComponent, IGlyph2D, IRender
{
    public Length PinX = new(30, "mm");
    public Length PinY = new(30, "mm");
    public Angle Angle = new(0);
    
    public Length Width = new(30,"mm");
    public Length Height = new(20, "mm");

    public FoScaledGlyph2D(string name) : base(name)
    {
    }

    public Task Draw(Canvas2DContext ctx, int tick)
    {
        throw new NotImplementedException();
    }

    public bool IsSmashed()
    {
        throw new NotImplementedException();
    }

    public FoGlyph2D MarkSelected(bool selected)
    {
        throw new NotImplementedException();
    }

    public Rectangle Rect()
    {
        throw new NotImplementedException();
    }

    public Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        throw new NotImplementedException();
    }
}




