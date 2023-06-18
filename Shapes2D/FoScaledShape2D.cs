

using IoBTMessage.Units;

namespace FoundryBlazor.Shape;

public class FoScaledShape2D : FoShape2D, IShape2D
{
    public Length PinXU = new(30, "mm");
    public Length PinYU = new(30, "mm");
    public Angle AngleU = new(0);
    public Length WidthU = new(30,"mm");
    public Length HeightU = new(20, "mm");
    public FoScaledShape2D() : base()
    {
        ShapeDraw = DrawRect;
    }
    public FoScaledShape2D(string name, string color) : base(name, color)
    {
        ShapeDraw = DrawRect;
    }


}

