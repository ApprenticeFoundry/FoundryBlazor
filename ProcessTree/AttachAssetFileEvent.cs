

namespace FoundryBlazor.Shape;
public class AttachAssetFileEvent<T> where T : FoGlyph2D
{
    public  T? AssetFile { get; set; }
    public  T? Target { get; set; }
}
