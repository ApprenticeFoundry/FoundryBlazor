using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Move : D2D_Base
{
    public string TargetId { get; set; }
    public string PayloadType { get; set; }
    public int PinX { get; set; }
    public int PinY { get; set; }
    public double Angle { get; set; }
    public D2D_Move()
    {
        TargetId = PayloadType = "";
        PinX = PinY = 0;
        Angle = 0.0;
    }

    public D2D_Move(FoGlyph2D glyph)
    {
        TargetId = glyph.GlyphId;
        PayloadType = glyph.GetType().Name;
        PinX = glyph.PinX;
        PinY = glyph.PinY;
        Angle = glyph.Angle;
    }
}
