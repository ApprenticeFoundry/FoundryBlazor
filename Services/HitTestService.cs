
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;


namespace FoundryBlazor.Shape;

public interface IHitTestService
{
    bool Insert(QuadHitTarget target);
    List<FoGlyph2D> FindGlyph(Rectangle rect);
    List<FoGlyph2D> FindGlyphExclude(Rectangle rect, FoGlyph2D? exclude);
    List<QuadHitTarget> AllShapesEverywhere();
    List<QuadHitTarget> RefreshQuadTree(FoPage2D page);
    Task RenderQuadTree(Canvas2DContext ctx, bool showTracks);

    void SetCanvasSizeInPixels(int width, int height);
    List<Rectangle> GetSearches();
    QuadTree<QuadHitTarget> GetTree();

}

public class HitTestService : IHitTestService
{

    private QuadTree<QuadHitTarget>? Tree { get; set; }

    private readonly List<Rectangle> PreviousSearches = new();
    private readonly IPanZoomService _panzoom;
    private Size CanvasSize = new(100, 100);
    private Rectangle CanvasRectangle = new(50, 50, 500, 500);

    public HitTestService(
        IPanZoomService panzoom)
    {
        _panzoom = panzoom;
    }

    public void SetCanvasSizeInPixels(int width, int height)
    {
        CanvasSize.Width = width;
        CanvasSize.Height = height;
    }

    public QuadTree<QuadHitTarget> GetTree()
    {
        Tree ??= new QuadTree<QuadHitTarget>(CanvasRectangle);
        return Tree;
    }

    public QuadTree<QuadHitTarget> InitTreeRoot()
    {
        Tree = GetTree();
        Tree.Clear(true);


        var mat = _panzoom.GetMatrix();
        mat.TransformRectangle(0, 0, CanvasSize.Width, CanvasSize.Height, ref CanvasRectangle);

        //recompute the tree rect and include the pan and zoom
        Tree.Reset(CanvasRectangle.X, CanvasRectangle.Y, CanvasRectangle.Width, CanvasRectangle.Height);
        return Tree;
    }

    public List<QuadHitTarget> RefreshQuadTree(FoPage2D page)
    {
        FoGlyph2D.ResetHitTesting(false, $"RefreshQuadTree {page.Key}");
        // $"Refresh Hit Test Tree {page.Name} ".WriteSuccess();

        //this rectangle should not shrink based on pan or zoom
        var tree = InitTreeRoot();

        // $"InsertShapesToQuadTree {page.Name} ".WriteSuccess();
        page.InsertShapesToQuadTree(tree, _panzoom);

        return AllShapesEverywhere();
    }


    public List<Rectangle> GetSearches()
    {
        return PreviousSearches;
    }

    public bool Insert(QuadHitTarget target)
    {
        Tree?.Insert(target);
        return Tree != null;
    }

    public List<FoGlyph2D> FindGlyph(Rectangle rect)
    {
        rect = _panzoom.TransformRect(rect);

        if (PreviousSearches.Count > 10)
            PreviousSearches.RemoveRange(0, 6);

        PreviousSearches.Add(rect);

        List<QuadHitTarget> list = new();
        Tree?.QueryObjects(rect, ref list);

        return list.Select(obj => obj.target).Cast<FoGlyph2D>().ToList();
    }

    public List<FoGlyph2D> FindGlyphExclude(Rectangle rect, FoGlyph2D? exclude)
    {

        return FindGlyph(rect).Where(item => item != exclude).ToList();
    }


    public List<QuadHitTarget> AllShapesEverywhere()
    {
        List<QuadHitTarget> list = new();
        Tree?.GetAllObjects(ref list);
        return list;
    }

    //this context must be in pixel space,  no pan or zoom
    public async Task RenderQuadTree(Canvas2DContext ctx, bool showTracks)
    {
        //$"Searches Count {PreviousSearches.Count}".WriteLine(ConsoleColor.Red);

        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(6);
        await ctx.SetLineDashAsync(new float[] { 20, 20 });

        await GetTree().DrawQuadTree(ctx);

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