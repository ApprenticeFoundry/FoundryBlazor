namespace FoundryBlazor.Shape;


public enum SelectionState
{
    Cleared,
    Changed,
}
public class SelectionChanged
{
    public SelectionState State { get; set; }
    public List<FoGlyph2D>? Selections { get; set; }
    public static SelectionChanged Changed(List<FoGlyph2D> selections)
    {
        var list = new List<FoGlyph2D>();
        list.AddRange(selections);
        var result = new SelectionChanged()
        {
            State = SelectionState.Changed,
            Selections = list
        };
        return result;
    }
    public static SelectionChanged Cleared(List<FoGlyph2D> selections)
    {
        var list = new List<FoGlyph2D>();
        list.AddRange(selections);
        var result = new SelectionChanged()
        {
            State = SelectionState.Cleared,
            Selections = list
        };
        return result;
    }
}
