
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Message;

//D2D means drawing to drawing
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
        TargetId = glyph.GetGlyphId();
        Payload = CodingExtensions.Dehydrate(glyph, false);
        PayloadType = glyph.GetType().Name;
    }

}
