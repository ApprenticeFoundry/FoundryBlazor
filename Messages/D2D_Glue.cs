using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Glue : D2D_Base
{
    public string TargetId { get; set; }
    public string SourceId { get; set; }
    public string PayloadType { get; set; }
    public D2D_Glue()
    {
        SourceId = TargetId = PayloadType = "";
    }

    public D2D_Glue(FoGlue2D glue)
    {
        var (source, target) = glue;
        Name = glue.Name;
        TargetId = target.GetGlyphId();
        SourceId = source.GetGlyphId();
        PayloadType = glue.GetType().Name;
    }
}
