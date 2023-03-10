namespace FoundryBlazor.Shape;

public interface ISelectionService
{
    void ClearAll();
    void ClearAllWhen(bool apply);
    FoGlyph2D AddItem(FoGlyph2D item);
    List<FoGlyph2D> AddRange(List<FoGlyph2D> list);
    void MoveTo(int x, int y);
    void MoveBy(int dx, int dy);
    void RotateBy(double da);
    void ZoomBy(double factor);
    List<FoGlyph2D> Selections();
}

public class SelectionService : ISelectionService
{
    public List<FoGlyph2D> Members { get; set; } = new();

    public List<FoGlyph2D> Selections()
    {
        return Members;
    }
    public void ClearAllWhen(bool apply)
    {
        //"ClearAllWhen".WriteLine(ConsoleColor.Green);   
        if ( apply ) ClearAll();
    }
    public void ClearAll()
    {
        //"ClearAll".WriteLine(ConsoleColor.Green);

        Members.ForEach(item => item.IsSelected = false);
        Members.Clear();
    }
    public List<FoGlyph2D> AddRange(List<FoGlyph2D> list)
    {
        list.ForEach(item => AddItem(item));
        return list;
    }

    public FoGlyph2D AddItem(FoGlyph2D item)
    {
        if (Members.IndexOf(item) == -1)
            Members.Add(item);

        item.MarkSelected(true);
        return item;
    }
    public void MoveTo(int x, int y)
    {
        Members.ForEach(item => item.MoveTo(x, y));
    }
    public void MoveBy(int dx, int dy)
    {
        Members.ForEach(item => item.MoveBy(dx, dy));
    }



    public void RotateBy(double da)
    {
        Members.ForEach(item => item.RotateBy(da));
    }

    public void ZoomBy(double factor)
    {
        Members.ForEach(item => item.ZoomBy(factor));
    }
}