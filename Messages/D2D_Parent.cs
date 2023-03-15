using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_Parent : D2D_Base
{
    public string ParentId { get; set; }
    public string ChildId { get; set; }

    public D2D_Parent()
    {
        ChildId = ParentId =  "";
    }

    public D2D_Parent(FoGlyph2D child, FoGlyph2D parent)
    {

        ChildId = child.GetGlyphId();
        ParentId = parent.GetGlyphId();
    }
}
