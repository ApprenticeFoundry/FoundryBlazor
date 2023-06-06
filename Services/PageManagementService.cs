
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;


namespace FoundryBlazor.Shape;

public interface IPageManagement: IRender
{

    List<FoGlyph2D> FindShapes(string GlyphId);
    List<FoGlyph2D> ExtractShapes(string GlyphId);
    List<FoGlyph2D> FindGlyph(Rectangle rect);
    List<FoGlyph2D> AllObjects();
    
    FoPage2D CurrentPage();
    FoPage2D SetCurrentPage(FoPage2D page);
    FoPage2D AddPage(FoPage2D page);

    List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true);
    List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true);

    void RefreshHitTesting(FoPanZoomWindow window);
    bool ToggleHitTestRender();


    int PageCount();


    FoPage2D SetPageSizeInches(double width, double height);
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

    private bool RenderHitTestTree = false;
    private FoPage2D ActivePage { get; set; }
    private readonly IHitTestService _hitTestService;
    private readonly ISelectionService _selectService;
    private readonly IScaledDrawing _ScaledDrawing;

    public PageManagementService(
        IHitTestService hit,
        IScaledDrawing help,
        ISelectionService sel)
    {
        _hitTestService = hit;
        _selectService = sel;
        _ScaledDrawing = help;

        ActivePage = CurrentPage();
    }



    public int PageCount()
    {
        return Members<FoPage2D>().Count;
    }



    public FoPage2D SetPageSizeInches(double width, double height)
    {
        _ScaledDrawing.SetPageSizeInches(width, height);

        var page = CurrentPage();
        _ScaledDrawing.SetPageDefaults(page);
        return page;
    }
    public FoPage2D SetPageLandscape()
    {
        var page = CurrentPage();
        _ScaledDrawing.SetPageLandscape();
        _ScaledDrawing.SetPageDefaults(page);
        return page;
    }
    public FoPage2D SetPagePortrait()
    {
        var page = CurrentPage();
        _ScaledDrawing.SetPagePortrait();
        _ScaledDrawing.SetPageDefaults(page);
        return page;
    }


    public bool ToggleHitTestRender()
    {
        RenderHitTestTree = !RenderHitTestTree;
        return RenderHitTestTree;
    }
    public void RefreshHitTesting(FoPanZoomWindow window)
    {
        _hitTestService.RefreshTree(CurrentPage());
        if (window != null)
            _hitTestService.Insert(window);
    }

    public List<FoGlyph2D> DeleteSelections()
    {
        var list = new List<FoGlyph2D>();
        Selections().ForEach(shape =>
        {
            if ( shape.IsSelected ) 
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
            if ( shape.IsSelected ) 
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

    public List<FoGlyph2D> FindGlyph(Rectangle rect)
    {
        return _hitTestService.FindGlyph(rect);
    }

    public List<FoGlyph2D> AllObjects()
    {
        return _hitTestService.AllShapesEverywhere();
    }


    public T AddShape<T>(T value) where T : FoGlyph2D
    {
        var found = ActivePage.AddShape(value);
        if ( found != null)
            _hitTestService.Insert(value);

        return found!;

    }

    public FoPage2D CurrentPage()
    {
        if (ActivePage == null)
        {
            var found = Members<FoPage2D>().Where(page => page.IsActive).FirstOrDefault();
            if (found == null)
            {
                found = new FoPage2D("Page-1", 1000, 500, "#D3D3D3");
                found.SetScaledDrawing(_ScaledDrawing);
                AddPage(found);
            }
            ActivePage = found;
            ActivePage.IsActive = true;
        }

        return ActivePage;
    }
    public FoPage2D SetCurrentPage(FoPage2D page)
    {
        ActivePage = page;
        Slot<FoPage2D>().ForEach(item => item.IsActive = false);
        ActivePage.IsActive = true;
        return ActivePage!;
    }

    public FoPage2D AddPage(FoPage2D page)
    {
        var found = Members<FoPage2D>().Where(item => item == page).FirstOrDefault();
        if (found == null)
            Slot<FoPage2D>().Add(page);
        return page;
    }

    public T Duplicate<T>(T value) where T : FoGlyph2D
    {
        var body = StorageHelpers.Dehydrate<T>(value, false);
        var shape = StorageHelpers.Hydrate<T>(body, false);

        shape.Name = "";
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
        var body = StorageHelpers.Dehydrate<T>(value, false);
        var shape = StorageHelpers.Hydrate<U>(body, false);

        shape.Name = "";
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

        $"GroupSelected {first}".WriteLine(ConsoleColor.White);

        if (Activator.CreateInstance(typeof(T)) is not T group) return null;

        Rectangle rect = first.Rect();
        _selectService.Selections().ForEach(item =>
        {
            rect = Rectangle.Union(rect, item.Rect());
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


    public async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        var page = CurrentPage();

        //await page.RenderNoItems(ctx, tick++);
        await page.RenderDetailed(ctx, tick++, deep);

        if ( RenderHitTestTree )
            await _hitTestService.RenderQuadTree(ctx,true);


        return true;
    }

    public async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        var page = CurrentPage();
        await page.RenderConcise(ctx, scale, region);

        if ( RenderHitTestTree )
            await _hitTestService.RenderQuadTree(ctx,false);
            
        return true;
    }

 
}