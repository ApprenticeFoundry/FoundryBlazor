using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Destroy : D2D_Base
{
    public string TargetId { get; set; }
    public string PayloadType { get; set; }
    public D2D_Destroy()
    {
        TargetId = PayloadType = "";
    }

    public D2D_Destroy(FoGlyph2D glyph)
    {
        TargetId = glyph.GlyphId;
        PayloadType = glyph.GetType().Name;
    }
}
