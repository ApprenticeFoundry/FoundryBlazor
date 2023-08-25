using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Unglue : D2D_Base
{
    public string BodyId { get; set; }
    public string TargetId { get; set; }
    public string SourceId { get; set; }
    public string PayloadType { get; set; }
    public D2D_Unglue()
    {
        SourceId = TargetId = BodyId = PayloadType = "";
    }

    public D2D_Unglue(FoGlue2D glue)
    {
        var (source, target, body) = glue;
        
        BodyId = body.GetGlyphId();
        TargetId = target.GetGlyphId();
        SourceId = source.GetGlyphId();
        PayloadType = glue.GetType().Name;
    }
}
