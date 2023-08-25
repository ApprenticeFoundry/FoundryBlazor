

namespace FoundryBlazor.Shape;

public class FoCommand2D : FoGlyph2D, IFoCommand
{
    public FoCommand2D(string name) : base(name,10,10,"Red")
    {
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public FoCommand2D Clear()
    {
        GetMembers<FoButton2D>()?.Clear();
        return this;
    }

    public List<IFoButton> Buttons()
    {
        return GetMembers<FoButton2D>()?.Select(item => item as IFoButton).ToList() ?? new List<IFoButton>();
    }
}
