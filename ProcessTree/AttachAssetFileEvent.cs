

namespace FoundryBlazor.Shape;
public class AttachAssetFileEvent 
{
    public  string? AssetGuid { get; set; }
    public  string? TargetGuid { get; set; }

    public  FoGlyph2D? AssetShape { get; set; }
    public  FoGlyph2D? TargetShape { get; set; }
}
