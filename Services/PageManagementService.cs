
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;


namespace FoundryBlazor.Shape;

public interface IPageManagement : IRender
{
    List<FoGlyph2D> FindShapes(string GlyphId);
    List<FoGlyph2D> ExtractShapes(string GlyphId);

    FoPage2D CurrentPage();
    FoPage2D SetCurrentPage(FoPage2D page);
    FoPage2D AddPage(FoPage2D page);
    FoPage2D RemovePage(FoPage2D page);
    FoPage2D? FindPage(string name);

    List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true);
    List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true);


    int PageCount();

    List<FoPage2D> GetAllPages();

    FoPage2D SetPageSize(double width, double height, string units);

    FoPage2D SetPageLandscape();
    FoPage2D SetPagePortrait();


    List<FoGlyph2D> CollectSelections();
    List<FoGlyph2D> Selections();
    List<FoGlyph2D> DeleteSelections();
    List<FoGlyph2D> DeleteSelectionsWithAnimations();
    void PageMoveBy(int dx, int dy);
    void SelectionsMoveBy(int dx, int dy);
    void SelectionsRotateBy(double da);
    void SelectionsZoomBy(double factor);
    T AddShape<T>(T value) where T : FoGlyph2D;
    T Duplicate<T>(T value) where T : FoGlyph2D;
    U MorphTo<T, U>(T value) where T : FoGlyph2D where U : FoGlyph2D;
    T? GroupSelected<T>() where T : FoGroup2D;
}


public class PageManagementService : FoComponent, IPageManagement
{

    private readonly ISelectionService _selectService;

    private FoPage2D? _page;
    public FoPage2D ActivePage
    {
        get
        {
            if (_page?.IsActive != true)
                $"Get Active Page {_page?.Key} is broken".WriteInfo();

            return _page!;
        }
    }

    public PageManagementService(
        ISelectionService sel)
    {
        _selectService = sel;
        CurrentPage();
    }



    public int PageCount()
    {
        return Members<FoPage2D>().Count;
    }

    public List<FoPage2D> GetAllPages()
    {
        return Members<FoPage2D>();
    }



    public FoPage2D SetPageSize(double width, double height, string units)
    {
        var page = CurrentPage();
        page.SetPageSize(width, height, units);
        return page;
    }

    public FoPage2D SetPageAxisX(int scale, double loc, string units)
    {
        var page = CurrentPage();
        page.SetPageAxisX(scale, loc, units);
        return page;
    }

    public FoPage2D SetPageAxisY(int scale, double loc, string units)
    {
        var page = CurrentPage();
        page.SetPageAxisY(scale, loc, units);
        return page;
    }
    public FoPage2D SetPageLandscape()
    {
        var page = CurrentPage();
        page.SetPageLandscape();
        return page;
    }
    public FoPage2D SetPagePortrait()
    {
        var page = CurrentPage();
        page.SetPagePortrait();
        return page;
    }

    public List<FoGlyph2D> DeleteSelections()
    {
        var list = new List<FoGlyph2D>();
        Selections().ForEach(shape =>
        {
            if (shape.IsSelected)
            {
                list.Add(shape);
                shape.MarkSelected(false);
                ExtractShapes(shape.GlyphId);
                shape.UnglueAll();
            }
        });
        return list;
    }

    public List<FoGlyph2D> DeleteSelectionsWithAnimations()
    {
        var list = new List<FoGlyph2D>();
        Selections().ForEach(shape =>
        {
            if (shape.IsSelected)
            {
                list.Add(shape);
                shape.MarkSelected(false);
                shape.AnimatedResizeTo(0, 0).OnComplete(() =>
                {
                    ExtractShapes(shape.GlyphId);
                    shape.UnglueAll();
                });
            }
        });
        return list;
    }

    public List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true)
    {
        Slot<FoPage2D>().ForEach(item => item.CollectImages(list, deep));
        return list;
    }
    public List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true)
    {
        Slot<FoPage2D>().ForEach(item => item.CollectVideos(list, deep));
        return list;
    }

    public List<FoGlyph2D> CollectSelections()
    {
        _selectService.ClearAll();
        var list = new List<FoGlyph2D>();
        Slot<FoPage2D>().ForEach(item => item.CollectSelected(list));
        _selectService.AddRange(list);
        return list;
    }

    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        return CurrentPage().ExtractShapes(GlyphId);
    }

    public List<FoGlyph2D> FindShapes(string GlyphId)
    {
        return CurrentPage().FindShapes(GlyphId);
    }






    public T AddShape<T>(T value) where T : FoGlyph2D
    {
        var found = ActivePage.AddShape(value);
        return found!;

    }

    public FoPage2D CurrentPage()
    {
        if (ActivePage == null)
        {
            var found = Members<FoPage2D>().Where(page => page.IsActive).FirstOrDefault();
            if (found == null)
            {
                found = new FoPage2D("Page-1", 300, 200, "RED");
                $"CurrentPage CREATING new page {found.Key}".WriteNote();
                AddPage(found);
            }
            return SetCurrentPage(found);
        }

        return ActivePage;
    }
    public FoPage2D SetCurrentPage(FoPage2D page)
    {
        if (_page == page && _page.IsActive)
            return _page;

        Slot<FoPage2D>().ForEach(item => item.IsActive = false);
        _page = page;
        _page.IsActive = true;

        //force refresh of hit testing
        FoGlyph2D.ResetHitTesting(true);
        return _page;
    }

    public FoPage2D AddPage(FoPage2D page)
    {
        var found = Members<FoPage2D>().Where(item => item == page).FirstOrDefault();
        if (found == null)
        {
            Slot<FoPage2D>().Add(page);

            //$"AddPage new page {page.Name}".WriteLine(ConsoleColor.White);
        }
        return page;
    }

    public FoPage2D RemovePage(FoPage2D page)
    {
        var found = Members<FoPage2D>().Where(item => item == page).FirstOrDefault();
        if (found != null)
        {
            Slot<FoPage2D>().Remove(found);
            if (found == _page)
            {
                found = Members<FoPage2D>().FirstOrDefault();
                SetCurrentPage(found!);
            }
        }
        return page;
    }

    public FoPage2D? FindPage(string name)
    {
        var found = Members<FoPage2D>().Where(item => item.Key.Matches(name)).FirstOrDefault();
        return found;
    }

    public T Duplicate<T>(T value) where T : FoGlyph2D
    {
        var body = CodingExtensions.Dehydrate<T>(value, false);
        var shape = CodingExtensions.Hydrate<T>(body, false);

        shape.Key = "";
        shape.GlyphId = "";

        //SRS write a method to duplicate actions
        shape.ShapeDraw = value.ShapeDraw;
        shape.DoOnOpenCreate = value.DoOnOpenCreate;
        shape.DoOnOpenEdit = value.DoOnOpenEdit;

        AddShape<T>(shape);
        return shape;
    }

    public U MorphTo<T, U>(T value) where T : FoGlyph2D where U : FoGlyph2D
    {
        var body = CodingExtensions.Dehydrate<T>(value, false);
        var shape = CodingExtensions.Hydrate<U>(body, false);

        shape.Key = "";
        shape.GlyphId = "";

        AddShape<U>(shape);
        return shape;
    }



    public List<FoGlyph2D> Selections()
    {
        return _selectService.Selections();
    }

    public void PageMoveBy(int dx, int dy)
    {
        var page = CurrentPage();
        page.MoveBy(dx, dy);
        page.Smash(false);
    }

    public void SelectionsMoveBy(int dx, int dy)
    {
        _selectService.MoveBy(dx, dy);
    }

    public void SelectionsRotateBy(double da)
    {
        _selectService.RotateBy(da);
    }

    public void SelectionsZoomBy(double factor)
    {
        _selectService.ZoomBy(factor);
    }

    public T? UngroupSelected<T>() where T : FoGroup2D
    {
        return null;
    }

    public T? GroupSelected<T>() where T : FoGroup2D
    {
        var first = _selectService.Selections().FirstOrDefault();
        if (first == null) return null;

        $"GroupSelected {first}".WriteNote();

        if (Activator.CreateInstance(typeof(T)) is not T group) return null;

        Rectangle rect = first.HitTestRect();
        _selectService.Selections().ForEach(item =>
        {
            rect = Rectangle.Union(rect, item.HitTestRect());
            //$"Rect {rect.X} {rect.Y} {rect.Width} {rect.Height}".WriteLine(ConsoleColor.White);
        });

        group.ResizeTo(rect.Width, rect.Height);
        var pt = group.PinLocation();
        group.MoveTo(rect.X + pt.X, rect.Y + pt.Y);
        this.AddShape<T>(group);

        group.CaptureSelectedShapes<FoShape1D>(CurrentPage());
        group.CaptureSelectedShapes<FoGroup2D>(CurrentPage());
        group.CaptureSelectedShapes<FoShape2D>(CurrentPage());
        group.CaptureSelectedShapes<FoText2D>(CurrentPage());

        _selectService.ClearAll();
        return group;
    }

    public void ClearMenu2D<U>(string name) where U : FoMenu2D
    {
        var menu = CurrentPage().Find<U>(name);
        menu?.Clear();
    }




    public virtual async Task Draw(Canvas2DContext ctx, int tick)
    {
        await CurrentPage().Draw(ctx, tick);
    }

    public virtual bool RenderDeepDetailed(Canvas2DContext ctx, int tick)
    {
        return false;
    }
    
    public async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        var page = CurrentPage();

        //await page.RenderNoItems(ctx, tick++);
        await page.RenderDetailed(ctx, tick++, deep);
        return true;
    }

    public async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        var page = CurrentPage();
        await page.RenderConcise(ctx, scale, region);

        return true;
    }



}