using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Create : D2D_Base
{
    public string TargetId { get; set; }
    public string Payload { get; set; }
    public string PayloadType { get; set; }
    public D2D_Create()
    {
        TargetId = Payload = PayloadType = "";
    }
    public D2D_Create(FoGlyph2D glyph)
    {
        TargetId = glyph.GlyphId;
        Payload = StorageHelpers.Dehydrate(glyph, false);
        PayloadType = glyph.GetType().Name;
    }

}
