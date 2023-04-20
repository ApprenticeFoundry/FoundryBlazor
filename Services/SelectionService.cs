using BlazorComponentBus;

namespace FoundryBlazor.Shape;


public interface ISelectionService
{
    ISelectionService ClearAll();
    ISelectionService ClearAllWhen(bool apply);
    FoGlyph2D AddItem(FoGlyph2D item);
    List<FoGlyph2D> AddRange(List<FoGlyph2D> list);
    void MouseDropped();
    void MouseReselect();
    ISelectionService MoveTo(int x, int y);
    ISelectionService MoveBy(int dx, int dy);
    ISelectionService RotateBy(double da);
    ISelectionService ZoomBy(double factor);
    List<FoGlyph2D> Selections();
}

public class SelectionService : ISelectionService
{
    protected ComponentBus PubSub;
    public List<FoGlyph2D> Members { get; set; } = new();

    public SelectionService(ComponentBus pubsub)
    {
        PubSub = pubsub;
    }

    public List<FoGlyph2D> Selections()
    {
        return Members;
    }

    public ISelectionService ClearAllWhen(bool apply)
    {
        //"ClearAllWhen".WriteLine(ConsoleColor.Green);   
        if ( apply ) ClearAll();
        return this;
    }

    public ISelectionService ClearAll()
    {
        //"ClearAll".WriteLine(ConsoleColor.Green);
        PubSub.Publish<SelectionChanged>(SelectionChanged.Cleared(Members));

        Members.ForEach(item => item.MarkSelected(false));
        Members.Clear();
        return this;
    }

    public List<FoGlyph2D> AddRange(List<FoGlyph2D> list)
    {
        list.ForEach(item => {
            item.MarkSelected(true);
            if (Members.IndexOf(item) == -1)
                Members.Add(item);
        });
        PubSub.Publish<SelectionChanged>(SelectionChanged.Changed(Members));
        return list;
    }

    public FoGlyph2D AddItem(FoGlyph2D item)
    {
        item.MarkSelected(true);
        if (Members.IndexOf(item) == -1)
            Members.Add(item);

        PubSub.Publish<SelectionChanged>(SelectionChanged.Changed(Members));
        return item;
    }
    public void MouseReselect()
    {
        if ( Members.Count > 0)
            PubSub.Publish<SelectionChanged>(SelectionChanged.Reselected(Members));
    }
    public void MouseDropped()
    {
        if ( Members.Count > 0)
            PubSub.Publish<SelectionChanged>(SelectionChanged.Dropped(Members));
    }

    public ISelectionService MoveTo(int x, int y)
    {
        Members.ForEach(item => item.MoveTo(x, y));
        return this;
    }
    public ISelectionService MoveBy(int dx, int dy)
    {
        Members.ForEach(item => item.MoveBy(dx, dy));
        return this;
    }

    public ISelectionService RotateBy(double da)
    {
        Members.ForEach(item => item.RotateBy(da));
        return this;
    }

    public ISelectionService ZoomBy(double factor)
    {
        Members.ForEach(item => item.ZoomBy(factor));
        return this;
    }
}