using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Glue : D2D_Base
{
    public string TargetId { get; set; }
    public string BodyId { get; set; }
    public string SourceId { get; set; }
    public string PayloadType { get; set; }
    public D2D_Glue()
    {
        SourceId = TargetId = BodyId = PayloadType = "";
    }

    public D2D_Glue(FoGlue2D glue)
    {
        var (source, target, body) = glue;
        Name = glue.Key;
        TargetId = target.GetGlyphId();
        BodyId = body.GetGlyphId();
        SourceId = source.GetGlyphId();
        PayloadType = glue.GetType().Name;
    }
}
