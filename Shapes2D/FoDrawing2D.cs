
using System.Diagnostics;
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoundryBlazor.Shape;



public interface IDrawing : IRender
{
    bool SetCurrentlyRendering(bool value);
    bool SetCurrentlyProcessing(bool value);
    void SetCanvasSize(int width, int height);
    Point InchesToPixelsInset(double width, double height);
    int ToPixels(double width);
    double ToInches(int value);

    Rectangle TransformRect(Rectangle rect);
    void CreateMenus(IJSRuntime js, NavigationManager nav);


    List<FoImage2D> GetAllImages();
    List<FoVideo2D> GetAllVideos();
    List<IFoMenu> CollectMenus(List<IFoMenu> list);

    FoMenu2D EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu2D;
    FoPanZoomWindow PanZoomWindow();

    Task RenderDrawing(Canvas2DContext ctx, int tick, double fps);

    void SetDoCreate(Action<CanvasMouseArgs> action);

    V AddShape<V>(V shape) where V : FoGlyph2D;
    FoPage2D CurrentPage();
    IPageManagement Pages();
    List<FoGlyph2D> ExtractShapes(string glyphId);

    void TogglePanZoomWindow();
    void ToggleHitTestDisplay();
    bool MovePanBy(int dx, int dy);

    D2D_UserMove UpdateOtherUsers(D2D_UserMove usermove, IToast toast);
    void SetUserID(string panID);
    void ClearAll();
}

public class FoDrawing2D : FoGlyph2D, IDrawing
{
    private string UserID = "";
    private InputStyle InputStyle = InputStyle.None;
    private Dictionary<string, D2D_UserMove> OtherUserLocations { get; set; } = new();
    public Action<CanvasMouseArgs>? DoCreate { get; set; }


    private IPageManagement PageManager { get; set; }
    private IScaledDrawingHelpers ScaleDrawing { get; set; }
    private IHitTestService HitTestService { get; set; }
    private FoPanZoomWindow? PanZoomShape { get; set; }
    private IPanZoomService PanZoomService { get; set; }
    private ISelectionService SelectionService { get; set; }


    public List<FoImage2D> AllImages = new();
    public List<FoVideo2D> AllVideos = new();
    private ComponentBus PubSub { get; set; }



    protected readonly Dictionary<InteractionStyle, IBaseInteraction> interactionLookup;
    protected InteractionStyle interactionStyle = InteractionStyle.ReadOnly;
    private IBaseInteraction? lastInteraction;


    //private Stopwatch stopwatch = new();
    private bool IsCurrentlyRendering = false;
    private bool IsCurrentlyProcessing = false;
    private readonly Queue<CanvasMouseArgs> MouseArgQueue = new();
    public bool SetCurrentlyRendering(bool isRendering)
    {
        var oldValue = IsCurrentlyRendering;
        if (isRendering)
        {
            //stopwatch.Restart();
        }

        if (!isRendering)
        {
            while (MouseArgQueue.Count > 0)
            {
                var args = MouseArgQueue.Dequeue();
                //$"is Dequeueing {args.Topic} ".WriteSuccess(2);
                ApplyMouseArgs(args);
            }
        }
        if (!isRendering)
        {
            //stopwatch.Stop();
            //var fps = 1000.0 / stopwatch.ElapsedMilliseconds;
            //$"render time {stopwatch.Elapsed}  {fps}".WriteInfo();
        }
        IsCurrentlyRendering = isRendering;
        return oldValue;
    }

    public bool SetCurrentlyProcessing(bool isProcessing)
    {
        var oldValue = IsCurrentlyProcessing;
        if (!isProcessing)
        {
            while (MouseArgQueue.Count > 0)
            {
                var args = MouseArgQueue.Dequeue();
                //$"is Dequeueing {args.Topic} ".WriteSuccess(2);
                ApplyMouseArgs(args);
            }
        }
        IsCurrentlyProcessing = isProcessing;
        return oldValue;
    }

    public FoDrawing2D(
        IPanZoomService panzoom,
        ISelectionService select,
        IPageManagement manager,
        IScaledDrawingHelpers scaled,
        IHitTestService hittest,
        ComponentBus pubSub
        )
    {
        ScaleDrawing = scaled;
        HitTestService = hittest;
        SelectionService = select;
        PanZoomService = panzoom;
        PageManager = manager;
        PubSub = pubSub;



        interactionLookup = new()
        {
            {InteractionStyle.ReadOnly, new BaseInteraction(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.PagePanAndZoom, new PagePanAndZoom(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeHovering, new ShapeHovering(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeSelection, new ShapeSelection(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeDragging, new ShapeDragging(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeResizing, new ShapeResizing(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeConnecting, new ShapeConnecting(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ModelLinking, new MoShapeLinking(this, pubSub, panzoom, select, manager, hittest)},
           //{InteractionStyle.AllEvents, new AllEvents(this, pubSub, panzoom, select, manager, hittest)},
        };


        InitSubscriptions();
        PanZoomService.SetOnComplete(() =>
        {
            //SRS refresh zoom if changed
            ResetPanZoom();
        });

        SetInteraction(InteractionStyle.ShapeHovering);
    }

    public void AddInteraction(InteractionStyle style, BaseInteraction interaction)
    {
        interactionLookup.Add(style, interaction);

        //$"SetInteraction {interactionStyle}".WriteSuccess();
    }
    public void SetInteraction(InteractionStyle style)
    {
        if (interactionStyle == style) return;
        lastInteraction?.Abort();
        interactionStyle = style;
        lastInteraction = null;

        //$"SetInteraction {interactionStyle}".WriteSuccess();
    }

    public IBaseInteraction GetInteraction()
    {
        lastInteraction ??= interactionLookup[interactionStyle];
        return lastInteraction;
    }

    public void SetUserID(string panID)
    {
        UserID = panID;
    }
    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        return PageManager.ExtractShapes(GlyphId);
    }

    public Point InchesToPixelsInset(double width, double height)
    {
        return ScaleDrawing.InchesToPixelInset(width, height);
    }

    public int ToPixels(double width)
    {
        return ScaleDrawing.ToPixels(width);
    }

    public double ToInches(int value)
    {
        return ScaleDrawing.ToInches(value);
    }
    public Rectangle TransformRect(Rectangle rect)
    {
        return PanZoomService.TransformRect(rect);
    }

    public void SetCanvasSize(int width, int height)
    {
        ScaleDrawing.SetCanvasSize(width, height);
    }

    public void ClearAll()
    {
        FoGlyph2D.ResetHitTesting = true;
        CurrentPage().ClearAll();
    }
    public IPageManagement Pages()
    {
        return PageManager;
    }

    public FoPage2D CurrentPage()
    {
        var page = PageManager.CurrentPage();
        return page;
    }
    public V AddShape<V>(V shape) where V : FoGlyph2D
    {
        return PageManager.AddShape<V>(shape);
    }

    public void SetDoCreate(Action<CanvasMouseArgs> action)
    {

        try
        {
            DoCreate = action;

            var page = PageManager.CurrentPage();
            DoCreate?.Invoke(new CanvasMouseArgs()
            {
                OffsetX = page.FractionX(0.15) + 20,
                OffsetY = page.FractionY(0.15) + 20
            });

            var region = ScaleDrawing.UserWindow();
            page.ComputeShouldRender(region);
        }
        catch (System.Exception ex)
        {
            $" DoCreate {action.Method.Name} {ex.Message}".WriteLine();
        }
    }




    public List<FoImage2D> GetAllImages()
    {
        if (FoImage2D.RefreshImages)
        {
            AllImages.Clear();
            PageManager.CollectImages(AllImages, true);

            AllImages.ForEach(item =>
            {
                $"{item.ImageUrl.Substring(0, 30)}".WriteLine();
            });
            FoImage2D.RefreshImages = false;
        }

        return AllImages;
    }

    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        return PageManager.CollectMenus(list);
    }
    public List<FoVideo2D> GetAllVideos()
    {
        if (FoVideo2D.RefreshVideos)
        {
            AllVideos.Clear();
            PageManager.CollectVideos(AllVideos, true);

            AllVideos.ForEach(item =>
            {
                $"{item.ImageUrl.Substring(0, 30)}".WriteLine();
            });
            FoVideo2D.RefreshVideos = false;
        }

        return AllVideos;
    }


    public void ResetPanZoom()
    {
        PanZoomWindow().SizeToFit();
    }

    public FoPanZoomWindow PanZoomWindow()
    {
        if (PanZoomShape == null)
        {
            PanZoomShape = new FoPanZoomWindow(PageManager, PanZoomService, HitTestService, ScaleDrawing, "Silver");
            PanZoomShape.SizeToFit();

            var page = PageManager.CurrentPage();
            var pt = InchesToPixelsInset(page.PageWidth / 2, 3.0);
            PanZoomShape.MoveTo(pt.X, pt.Y);
        }
        return PanZoomShape;
    }

    public void TogglePanZoomWindow()
    {
        FoGlyph2D.ResetHitTesting = true;
        PanZoomWindow().IsVisible = !PanZoomWindow().IsVisible;
    }

    public void ToggleHitTestDisplay()
    {
        FoGlyph2D.ResetHitTesting = true;
        PageManager.ToggleHitTestRender();
    }


    public FoMenu2D EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu2D
    {
        var result = PageManager.EstablishMenu2D<T, FoButton2D>(name, menu, clear);
        return result;
    }


    public void RefreshHitTest_IfDirty()
    {
        if (FoGlyph2D.ResetHitTesting)
            PageManager.RefreshHitTesting(PanZoomWindow());

        FoGlyph2D.ResetHitTesting = false;
    }


    public void CreateMenus(IJSRuntime js, NavigationManager nav)
    {


        EstablishMenu<FoMenu2D>("Main", new Dictionary<string, Action>()
        {

            { "Clear", () => ClearAll()},
            { "Group", () => PageManager?.GroupSelected<FoGroup2D>()},
            // { "Ungroup", () => PageManager.UngroupSelected<FoGroup2D>()},

        }, true);


        // https://coastalcreative.com/standard-paper-sizes/
        EstablishMenu<FoMenu2D>("Page Size", new Dictionary<string, Action>()
        {
            { "ANSI A (Letter)", () => PageManager.SetPageSizeInches(8.5,11)}, //8.5” x 11”
            { "ANSI B (Tabloid)", () => PageManager.SetPageSizeInches(8.5,17)},  //11” × 17”
            { "ANSI C", () => PageManager.SetPageSizeInches(17,22)}, //17” × 22”
            { "ANSI D", () => PageManager.SetPageSizeInches(22,34)}, //22” × 34”
            { "ANSI E", () => PageManager.SetPageSizeInches(34,44)}, //34” × 44”
            { "Landscape", () => { PageManager.SetPageLandscape(); ResetPanZoom();} },
            { "Portrait", () => { PageManager.SetPagePortrait(); ResetPanZoom(); } },
        }, true)
        .ToggleLayout().MoveTo(0, 80);


        PageManager.SetPageSizeInches(34, 44);
        //PanZoomService.SetZoom(0.5);
        //PanZoomService.SetPan(75, 25);
        //PageManager.SetPageLandscape();
        //ResetPanZoom();
        //PanZoomWindow().IsVisible = false;
    }




    public bool UserWindowMovedTo(Point loc)
    {
        var region = ScaleDrawing.SetUserWindow(loc);
        var page = PageManager.CurrentPage();
        page.ComputeShouldRender(region);

        //  $"UserWindowResized {rect.X} {rect.Y} {rect.Width} {rect.Height} ---".WriteLine(ConsoleColor.Blue);
        return true;
    }
    public bool UserWindowResized(Size size)
    {
        var region = ScaleDrawing.SetUserWindow(size);
        var page = PageManager.CurrentPage();
        page.ComputeShouldRender(region);

        //  $"UserWindowResized {rect.X} {rect.Y} {rect.Width} {rect.Height} ---".WriteLine(ConsoleColor.Blue);
        return true;
    }

    public async Task RenderDrawing(Canvas2DContext ctx, int tick, double fps)
    {

        //skip this frame is still working 
        if ( IsCurrentlyProcessing ) return;

        FoGlyph2D.Animations.Update((float)0.033);

        var wasDirty = FoGlyph2D.ResetHitTesting;
        RefreshHitTest_IfDirty();

        var page = PageManager.CurrentPage();

        page.Color = InputStyle == InputStyle.FileDrop ? "Yellow" : "Grey";

        await ScaleDrawing.ClearCanvas(ctx);

        await ctx.SaveAsync();
        var (zoom, panx, pany) = await PanZoomService.TranslateAndScale(ctx, page);

        await PageManager.RenderDetailed(ctx, tick, true);
        await PanZoomWindow().RenderConcise(ctx, zoom, page.Rect());

        await ctx.RestoreAsync();


        await GetInteraction().RenderDrawing(ctx, tick);


        var offsetY = 60;
        var offsetX = 1400;

        await ctx.SetTextAlignAsync(Blazor.Extensions.Canvas.Canvas2D.TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);

        await ctx.SetFillStyleAsync(wasDirty ? "#FF0000" : "#000000");
        await ctx.SetFontAsync("26px Segoe UI");
        await ctx.FillTextAsync($"Foundry Canvas {UserID} {InputStyle}", offsetX, offsetY);

        await ctx.SetFontAsync("18px consolas");
        //await ctx.FillTextAsync($"zoom: {zoom:0.00} panx: {panx} panx: {pany} fps: {fps:0.00}", offsetX, offsetY + 25);
        await ctx.FillTextAsync($"fps: {fps:0.00}", offsetX, offsetY + 25);
        await ctx.FillTextAsync($"{page.Name}  {ScaleDrawing.CanvasWH()} {page.DrawingWH()}", offsetX, offsetY + 50);

        int loc = 130;

        await ctx.SetTextBaselineAsync(TextBaseline.Top);
        OtherUserLocations.Values.ForEach(async user =>
        {
            loc += 15;
            await ctx.FillTextAsync($"{user.UserID} is helping", 20 + offsetX, loc);

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(user.X, user.Y);
            await ctx.LineToAsync(user.X + 20, user.Y + 15);
            await ctx.LineToAsync(user.X, user.Y + 20);
            await ctx.LineToAsync(user.X, user.Y);
            await ctx.ClosePathAsync();
            await ctx.FillAsync();

            await ctx.FillTextAsync(user.UserID, user.X + 5, user.Y + 20);
        });


    }

    public D2D_UserMove UpdateOtherUsers(D2D_UserMove usermove, IToast toast)
    {
        var key = usermove.UserID;

        if (IsCurrentlyRendering)
        {
            if (!OtherUserLocations.ContainsKey(key)) return usermove;

            var found = OtherUserLocations[key];
            found.Active = usermove.Active;
            found.X = usermove.X;
            found.Y = usermove.Y;
            return found;
        }

        if (!OtherUserLocations.Remove(key))
            if (usermove.Active)
                toast?.Success($"{key} has joined");


        if (usermove.Active)
            OtherUserLocations.Add(key, usermove);
        else
            toast?.Info($"{key} has left");
            
        return usermove;
    }

    private async Task DrawUserWindow(Canvas2DContext ctx)
    {
        // draw the current window
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.SetLineWidthAsync(10.0F);
        var win = ScaleDrawing.UserWindow();
        await ctx.StrokeRectAsync(-win.X + 10, -win.Y + 10, win.Width - 20, win.Height - 20);
    }

    protected bool TestRule(InteractionStyle style, CanvasMouseArgs args)
    {
        var interact = interactionLookup[style];
        if (interact.IsDefaultTool(args) == false)
        {
            $"{style} No Match".WriteError();
            return false;
        }

        $"{style} Match".WriteSuccess();
        SetInteraction(style);
        return true;
    }

    protected virtual void SelectInteractionByRuleFor(CanvasMouseArgs args)
    {
        if (TestRule(InteractionStyle.PagePanAndZoom, args)) return;
        
        if (TestRule(InteractionStyle.ModelLinking, args)) return;

        if (TestRule(InteractionStyle.ShapeConnecting, args)) return;

        if (TestRule(InteractionStyle.ShapeResizing, args)) return;

        if (TestRule(InteractionStyle.ShapeDragging, args)) return;

        TestRule(InteractionStyle.ShapeSelection, args);
    }

    private void ApplyMouseArgs(CanvasMouseArgs args)
    {
        try
        {
            SetCurrentlyProcessing(true);
            // call IsDefaultTool method on each interaction to
            // determine what is the right interaction for this case?

            if (args.Topic.Matches("ON_MOUSE_DOWN"))
                SelectInteractionByRuleFor(args);


            var interact = GetInteraction();

            var isEventHandled = (args.Topic) switch
            {
                ("ON_MOUSE_DOWN") => interact.MouseDown(args),
                ("ON_MOUSE_MOVE") => interact.MouseMove(args),
                ("ON_MOUSE_UP") => interact.MouseUp(args),
                ("ON_MOUSE_IN") => interact.MouseIn(args),
                ("ON_MOUSE_OUT") => interact.MouseOut(args),
                _ => false
            };
        }
        catch (Exception ex)
        {
            $" {args.Topic} {ex.Message}".WriteLine();
        }
        finally
        {
            SetCurrentlyProcessing(false);
        }

    }


    private void InitSubscriptions()
    {
        PubSub!.SubscribeTo<InputStyle>(style =>
        {
            InputStyle = style;
        });

        PubSub!.SubscribeTo<CanvasMouseArgs>(args =>
        {
            try
            {
                if (IsCurrentlyRendering || IsCurrentlyProcessing)
                {
                    //you should cashe the args to replayed latter
                    //when the UI is not rendering..
                    MouseArgQueue.Enqueue(args);
                }
                else
                {
                    //"is rendering ".WriteSuccess(2);
                    ApplyMouseArgs(args);
                }

            }
            catch (Exception ex)
            {
                $" {args.Topic} {ex.Message}".WriteLine();
            }
        });

        PubSub!.SubscribeTo<CanvasKeyboardEventArgs>(args =>
         {
             try
             {
                 var isEventHandled = (args.Topic) switch
                 {

                     ("ON_KEY_DOWN") => KeyDown(args),
                     ("ON_KEY_UP") => KeyUp(args),
                     ("ON_KEY_PRESS") => KeyPress(args),
                     _ => false
                 };
             }
             catch (Exception ex)
             {
                 $" {args.Topic} {ex.Message}".WriteLine();
             }
         });

        PubSub!.SubscribeTo<CanvasWheelChangeArgs>(args =>
        {
            try
            {
                var isEventHandled = (args.Topic) switch
                {
                    ("ON_WHEEL_CHANGE") => WheelChange((CanvasWheelChangeArgs)args),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                $" {args.Topic} {ex.Message}".WriteLine();
            }
        });

        PubSub!.SubscribeTo<CanvasResizeArgs>(args =>
        {
            try
            {
                var isEventHandled = (args.Topic) switch
                {
                    ("ON_USERWINDOW_RESIZE") => UserWindowResized(args.size),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                $" {args.Topic} {ex.Message}".WriteLine();
            }
        });
    }


    public bool WheelChange(CanvasWheelChangeArgs args)
    {
        PanZoomService.ZoomWheel(args.DeltaY);
        //$"Wheel change: {args.DeltaX}, {args.DeltaY}, {args.DeltaZ}".WriteLine(ConsoleColor.Yellow);
        return true;
    }

    public bool MovePanBy(int dx, int dy)
    {
        PanZoomService.PanBy(dx, dy);
        UserWindowMovedTo(PanZoomService.Pan());

        PanZoomShape?.Smash(false);  //Anit scale and move
        return true;
    }

    public bool MoveSelectionsBy(int dx, int dy)
    {
        PageManager.SelectionsMoveBy(dx, dy);
        PageManager.Selections().ForEach(shape => PubSub.Publish<FoGlyph2D>(shape));
        return true;
    }
    public bool RotateSelectionsBy(int angle)
    {
        PageManager.SelectionsRotateBy(angle);
        PageManager.Selections().ForEach(shape => PubSub.Publish<FoGlyph2D>(shape));
        return true;
    }

    public bool LayoutSelections()
    {
        PageManager.Selections().ForEach(shape =>
        {
            if (shape is FoCompound2D comp)
                comp.ApplyLayout = true;
        });
        return true;
    }

    public bool ZoomSelectionBy(double factor)
    {
        PageManager?.SelectionsZoomBy(factor);
        // PageManager?.Selections().ForEach(shape => SendShapeMoved(shape));
        return true;
    }

    public override bool OpenEdit()
    {
        var item = PageManager.Selections().FirstOrDefault();
        return item?.OpenEdit() ?? false;
    }

    public override bool OpenCreate()
    {
        var item = PageManager.Selections().FirstOrDefault();
        return item?.OpenCreate() ?? false;
    }

    public bool DeleteSelections()
    {
        PageManager.Selections().ForEach(shape =>
        {
            shape.IsSelected = false;
            shape.AnimatedResizeTo(0, 0).OnComplete(() =>
            {
                PageManager.ExtractShapes(shape.GlyphId);
                shape.UnglueAll();
                PubSub.Publish<D2D_Destroy>(new D2D_Destroy(shape));
            });
        });
        return true;
    }
    public bool DuplicateSelections()
    {
        var Duplicates = new List<FoGlyph2D>();
        PageManager.Selections().ForEach(shape =>
        {
            shape.IsSelected = false;
            if (shape is FoCompound2D comp2D)
                Duplicates.Add(PageManager.Duplicate<FoCompound2D>(comp2D));
            else if (shape is FoShape2D shape2D)
                Duplicates.Add(PageManager.Duplicate<FoShape2D>(shape2D));
            else if (shape is FoImage2D image2D)
                Duplicates.Add(PageManager.Duplicate<FoImage2D>(image2D));
            else if (shape is FoText2D text2D)
                Duplicates.Add(PageManager.Duplicate<FoText2D>(text2D));
        });
        SelectionService?.ClearAll();
        SelectionService?.AddRange(Duplicates);
        SelectionService?.MoveBy(50, 50);

        return true;
    }


    public bool KeyDown(CanvasKeyboardEventArgs args)
    {
        //$"Key Down ShiftKey?: {args.ShiftKey}, AltKey?: {args.AltKey}, CtrlKey?: {args.CtrlKey}, Key={args.Key} Code={args.Code}".WriteLine(ConsoleColor.Yellow);

        var move = args.ShiftKey ? 1 : 5;
        var success = (args.Code, args.AltKey, args.ShiftKey) switch
        {
            ("ArrowUp", false, true) => MovePanBy(0, -move * 10),
            ("ArrowDown", false, true) => MovePanBy(0, move * 10),
            ("ArrowLeft", false, true) => MovePanBy(-move * 10, 0),
            ("ArrowRight", false, true) => MovePanBy(move * 10, 0),

            ("ArrowUp", false, false) => MoveSelectionsBy(0, -move),
            ("ArrowDown", false, false) => MoveSelectionsBy(0, move),
            ("ArrowLeft", false, false) => MoveSelectionsBy(-move, 0),
            ("ArrowRight", false, false) => MoveSelectionsBy(move, 0),
            ("ArrowUp", true, false) => ZoomSelectionBy(1.25),
            ("ArrowDown", true, false) => ZoomSelectionBy(0.75),

            ("KeyG", true, false) => PageManager?.GroupSelected<FoGroup2D>() != null,
            ("KeyR", true, false) => RotateSelectionsBy(30),
            ("KeyL", true, false) => LayoutSelections(),
            ("KeyO", true, false) => OpenEdit(),
            ("KeyC", true, false) => OpenCreate(),

            ("Insert", false, false) => DuplicateSelections(),
            ("Delete", false, false) => DeleteSelections(),
            _ => false
        };
        //$"result {success}".WriteLine(ConsoleColor.Red);
        return success;
    }

    public bool KeyUp(CanvasKeyboardEventArgs args)
    {
        //$"Key Up ShiftKey?: {args.ShiftKey}, AltKey?: {args.AltKey}, CtrlKey?: {args.CtrlKey}, Key={args.Key} Code={args.Code}".WriteLine(ConsoleColor.Yellow);
        return true;
    }

    public bool KeyPress(CanvasKeyboardEventArgs args)
    {
        //var msg = $"Key Press ShiftKey?: {args.ShiftKey}, AltKey?: {args.AltKey}, CtrlKey?: {args.CtrlKey}, Key={args.Key} Code={args.Code}".WriteLine(ConsoleColor.Yellow);
        return true;
    }

}
