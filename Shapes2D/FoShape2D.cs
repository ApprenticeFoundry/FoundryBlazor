using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public class FoShape2D : FoGlyph2D, IShape2D
{

    public FoShape2D() : base()
    {
        ShapeDraw = DrawRect;
    }

    public FoShape2D(int width, int height, string color) : base("", width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawRect;
    }

    public FoShape2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawRect;
    }



    public override List<FoHandle2D> GetHandles() 
    {
        if ( !this.HasSlot<FoHandle2D>()) 
        {
            Add<FoHandle2D>(new FoHandle2D("UL", LeftX(), TopY(), "Green"));
            Add<FoHandle2D>(new FoHandle2D("UR", RightX(), TopY(), "Green"));
            Add<FoHandle2D>(new FoHandle2D("LL", LeftX(), BottomY(), "Green"));
            Add<FoHandle2D>(new FoHandle2D("LR", RightX(), BottomY(), "Green"));
        }
        var result = this.Members<FoHandle2D>();
        return result;
    }


}
