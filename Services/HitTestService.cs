
using Blazor.Extensions.Canvas.Canvas2D;
using System.Drawing;


namespace FoundryBlazor.Shape;

public interface IHitTestService
{
    bool Insert(FoGlyph2D glyph);
    bool InsertRange(List<FoGlyph2D> list);
    List<FoGlyph2D> FindGlyph(Rectangle rect);
    List<FoGlyph2D> AllShapesEverywhere();
    List<FoGlyph2D> RefreshTree(FoPage2D page);
    Task RenderQuadTree(Canvas2DContext ctx, bool showTracks);
    void SetRectangle(Rectangle rect);
}

public class HitTestService : IHitTestService
{
    private FoPage2D Page { get; set; }
    private QuadTree<FoGlyph2D> Tree { get; set; }

    private readonly List<Rectangle> PreviousSearches = new();
    private readonly IPanZoomService _panzoom;
    private Rectangle Rect = new(0, 0, 100, 100);

    public HitTestService(IPanZoomService panzoom)
    {
        _panzoom = panzoom;
        Page = new FoPage2D("dummy", "White");
        Tree = Tree != null ? Tree.Clear(true) : new QuadTree<FoGlyph2D>(Rect);
        Tree.Reset(Rect.X, Rect.Y, Rect.Width, Rect.Height);
    }

    public void SetRectangle(Rectangle rect)
    {
        Rect.X = rect.X;
        Rect.Y = rect.Y;
        Rect.Width = rect.Width;
        Rect.Height = rect.Height;

        Tree = Tree != null ? Tree.Clear(true) : new QuadTree<FoGlyph2D>(Rect);
        Tree.Reset(Rect.X, Rect.Y, Rect.Width, Rect.Height);

    }

    public List<FoGlyph2D> RefreshTree(FoPage2D page)
    {
        Page = page;

        Tree = Tree != null ? Tree.Clear(true) : new QuadTree<FoGlyph2D>(Rect);
        Tree.Reset(Rect.X, Rect.Y, Rect.Width, Rect.Height);

        Page.InsertShapesToQuadTree(Tree, _panzoom);

        return AllShapesEverywhere();
    }

    public bool InsertRange(List<FoGlyph2D> list)
    {
        if (Tree != null)
            list.ForEach(child => Tree.Insert(child, child.Rect()));
        return Tree != null;
    }

    public bool Insert(FoGlyph2D glyph)
    {
        Tree?.Insert(glyph, glyph.Rect());
        return Tree != null;
    }

    public List<FoGlyph2D> FindGlyph(Rectangle rect)
    {
        if (PreviousSearches.Count > 10)
            PreviousSearches.RemoveRange(0, 6);

        PreviousSearches.Add(rect);
        //$"Search {rect.X} {rect.Y} {rect.Width} {rect.Height}".WriteLine(ConsoleColor.Blue);

        List<FoGlyph2D> list = new();
        Tree?.GetObjects(rect, ref list);
        //$"Found {list.Count} Searches {PreviousSearches.Count}".WriteLine(ConsoleColor.Blue);

        // PreviousSearches.ForEach(rect =>
        // {
        //     $"=> Searches {rect.X} {rect.Y} {rect.Width} {rect.Height}".WriteLine(ConsoleColor.Blue);
        // });
        return list;
    }

    public List<FoGlyph2D> AllShapesEverywhere()
    {
        List<FoGlyph2D> list = new();
        Tree?.GetAllObjects(ref list);
        return list;
    }

    public async Task RenderQuadTree(Canvas2DContext ctx, bool showTracks)
    {
        //$"Searches Count {PreviousSearches.Count}".WriteLine(ConsoleColor.Red);

        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(2);
        await ctx.SetLineDashAsync(new float[] { 20, 20 });

        await Tree.DrawQuadTree(ctx, false);

        if (showTracks)
        {
            await ctx.SetLineWidthAsync(1);
            await ctx.SetLineDashAsync(Array.Empty<float>());
            await ctx.SetStrokeStyleAsync("Blue");

            PreviousSearches.ForEach(async rect =>
            {
                //$"Render {rect.X} {rect.Y} {rect.Width} {rect.Height}".WriteLine(ConsoleColor.Blue);
                await ctx.StrokeRectAsync(rect.X, rect.Y, rect.Width, rect.Height);
            });
        }

        await ctx.RestoreAsync();
    }


}