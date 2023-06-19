
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
}

public class HitTestService : IHitTestService
{
    private FoPage2D Page { get; set; }
    private QuadTree<FoGlyph2D> Tree { get; set; }

    private readonly List<Rectangle> PreviousSearches = new();
    private readonly IScaledCanvas _scaled;
    private readonly IPanZoomService _panzoom;
    public HitTestService(IScaledCanvas scaled, IPanZoomService panzoom)
    {
        _scaled = scaled;
        _panzoom = panzoom;
        Page = new FoPage2D("dummy", "White");
        var rect = Page.Rect();
        Tree = Tree != null ? Tree.Clear(true) : new QuadTree<FoGlyph2D>(rect);
        Tree.Reset(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public Rectangle Rect()
    {
        //make sure any shape on the canvas is in the hittest
        var canvas = _scaled.Rect();
        canvas = _panzoom.AntiScaleRect(canvas);
        return canvas;
    }
    public List<FoGlyph2D> RefreshTree(FoPage2D page)
    {
        Page = page;
        var rect = Page.Rect();

        Tree = Tree != null ? Tree.Clear(true) : new QuadTree<FoGlyph2D>(rect);
        Tree.Reset(rect.X, rect.Y, rect.Width, rect.Height);

        Page.InsertShapesToQuadTree(Tree);

        return AllShapesEverywhere();
    }

    public bool InsertRange(List<FoGlyph2D> list)
    {
        if (Tree != null)
            list.ForEach(child => Tree.Insert(child));
        return Tree != null;
    }

    public bool Insert(FoGlyph2D glyph)
    {
        Tree?.Insert(glyph);
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