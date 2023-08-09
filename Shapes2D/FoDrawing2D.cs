
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using IoBTMessage.Extensions;
using System.Drawing;
using IoBTMessage.Units;

namespace FoundryBlazor.Shape;



public interface IDrawing : IRender
{
    bool SetCurrentlyRendering(bool value, int tick);
    bool SetCurrentlyProcessing(bool value);
    void SetCanvasPixelSize(int width, int height);

    Size TrueCanvasSize();
    Rectangle TransformRect(Rectangle rect);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);



    List<FoPage2D> GetAllPages();
    List<FoImage2D> GetAllImages();
    List<FoVideo2D> GetAllVideos();

    FoPage2D SetCurrentPage(FoPage2D page);
    FoPanZoomWindow PanZoomWindow();

    Task RenderDrawing(Canvas2DContext ctx, int tick, double fps);
    void SetPreRenderAction(Func<Canvas2DContext, int, Task> action);
    void SetPostRenderAction(Func<Canvas2DContext, int, Task> action);
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
    List<FoGlyph2D> Selections();
    List<FoGlyph2D> DeleteSelections();
    Rectangle Rect();
}

public class FoDrawing2D : FoGlyph2D, IDrawing
{
    
    public bool ShowStats { get; set; } = false;
    private int TrueCanvasWidth = 0;
    private int TrueCanvasHeight = 0;

    private Rectangle UserWindowRect { get; set; } = new Rectangle(0, 0, 1500, 400);

    private string UserID = "";
    private InputStyle InputStyle = InputStyle.None;
    private Dictionary<string, D2D_UserMove> OtherUserLocations { get; set; } = new();
    public Action<CanvasMouseArgs>? DoCreate { get; set; }


    public Func<Canvas2DContext, int, Task>? PreRender { get; set; }
    public Func<Canvas2DContext, int, Task>? PostRender { get; set; }
    private IPageManagement PageManager { get; set; }

    private IHitTestService HitTestService { get; set; }
    private FoPanZoomWindow? PanZoomShape { get; set; }
    private IPanZoomService PanZoomService { get; set; }
    private ISelectionService SelectionService { get; set; }


    public List<FoImage2D> AllImages = new();
    public List<FoVideo2D> AllVideos = new();
    private ComponentBus PubSub { get; set; }

    protected readonly List<BaseInteraction> interactionRules;
    protected readonly Dictionary<InteractionStyle, BaseInteraction> interactionLookup;
    protected InteractionStyle interactionStyle = InteractionStyle.ReadOnly;
    private IBaseInteraction? lastInteraction;




    //private readonly Stopwatch stopwatch = new();
    //private int lastTick = 0;
    private bool IsCurrentlyRendering = false;
    private bool IsCurrentlyProcessing = false;
    private readonly Queue<CanvasMouseArgs> MouseArgQueue = new();


    public override Rectangle Rect()
    {
        var result = new Rectangle(0, 0, TrueCanvasWidth, TrueCanvasHeight);
        return result;
    }

    public bool SetCurrentlyRendering(bool isRendering, int tick)
    {
        var oldValue = IsCurrentlyRendering;
        // if (isRendering)
        // {
        //     stopwatch.Restart();
        //     lastTick = tick;
        // }

        if (!isRendering)
        {
            // if ( tick == lastTick)
            // {
            //     stopwatch.Stop();
            //     var fps = 1000.0 / stopwatch.ElapsedMilliseconds;
            //     $"time to render is {stopwatch.ElapsedMilliseconds} ms  FPS: {fps}".WriteInfo();
            // } else {
            //     $"skipped a tick {lastTick} {tick}".WriteInfo();
            // }

            while (MouseArgQueue.Count > 0)
            {
                var args = MouseArgQueue.Dequeue();
                // $"SetCurrentlyRendering is Dequeueing {args.Topic} ".WriteSuccess(2);
                ApplyMouseArgs(args);
            }
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
                //$"SetCurrentlyProcessing Dequeueing {args.Topic} ".WriteSuccess(2);
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
        IHitTestService hittest,
        ComponentBus pubSub
        )
    {
        HitTestService = hittest;
        HitTestService.SetRectangle(Rect());
        SelectionService = select;
        PanZoomService = panzoom;
        PageManager = manager;
        PubSub = pubSub;


        interactionRules = new()
        {
            {new PagePanAndZoom(InteractionStyle.PagePanAndZoom, 11, this, pubSub, panzoom, select, manager, hittest)},
            {new MoShapeLinking(InteractionStyle.ModelLinking, 10, this, pubSub, panzoom, select, manager, hittest)},
            {new ShapeMenu(InteractionStyle.ShapeMenu, 9, this, pubSub, panzoom, select, manager, hittest)},
            {new ShapeConnecting(InteractionStyle.ShapeConnecting, 8, this, pubSub, panzoom, select, manager, hittest)},
            {new ShapeResizing(InteractionStyle.ShapeResizing, 7, this, pubSub, panzoom, select, manager, hittest)},
            {new ShapeDragging(InteractionStyle.ShapeDragging, 5, this, pubSub, panzoom, select, manager, hittest)},
            {new ShapeSelection(InteractionStyle.ShapeSelection, 4, this, pubSub, panzoom, select, manager, hittest)},
            {new ShapeHovering(InteractionStyle.ShapeHovering, 3, this, pubSub, panzoom, select, manager, hittest)},
            {new BaseInteraction(InteractionStyle.ReadOnly, 0, this, pubSub, panzoom, select, manager, hittest)},
        };

        interactionLookup = new();
        interactionRules.ForEach(x =>
        {
            interactionLookup.Add(x.Style, x);
        });

        InitSubscriptions();
        PanZoomService.SetOnComplete(() =>
        {
            //SRS refresh zoom if changed
            ResetPanZoom();
        });

        SetInteraction(InteractionStyle.ShapeHovering);
    }

    public void SetPreRenderAction(Func<Canvas2DContext, int, Task> action)
    {
        PreRender = action;
    }
    public void SetPostRenderAction(Func<Canvas2DContext, int, Task> action)
    {
        PostRender = action;
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

    public Rectangle UserWindow()
    {
        return UserWindowRect;
    }

    public Rectangle SetUserWindow(Size size)
    {
        UserWindowRect = new Rectangle(UserWindowRect.Location, size);
        return UserWindowRect;
    }
    public Rectangle SetUserWindow(Point loc)
    {
        UserWindowRect = new Rectangle(-loc.X, -loc.Y, UserWindowRect.Width, UserWindowRect.Height);
        return UserWindowRect;
    }
    public void SetUserID(string panID)
    {
        UserID = panID;
    }
    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        return PageManager.ExtractShapes(GlyphId);
    }

    public List<FoPage2D> GetAllPages()
    {
        return PageManager.GetAllPages();
    }
    public List<FoGlyph2D> DeleteSelections()
    {
        return PageManager.DeleteSelections();
    }
    public List<FoGlyph2D> DeleteSelectionsWithAnimations()
    {
        return PageManager.DeleteSelectionsWithAnimations();
    }
    public List<FoGlyph2D> Selections()
    {
        return PageManager.Selections();
    }


    public Rectangle TransformRect(Rectangle rect)
    {
        return PanZoomService.TransformRect(rect);
    }

    public void SetCanvasPixelSize(int width, int height)
    {
        TrueCanvasWidth = width;
        TrueCanvasHeight = height;
        HitTestService.SetRectangle(Rect());
    }
    public Size TrueCanvasSize()
    {
        return new Size(TrueCanvasWidth, TrueCanvasHeight);
    }

    public async Task ClearCanvas(Canvas2DContext ctx)
    {
        await ctx.ClearRectAsync(0, 0, TrueCanvasWidth, TrueCanvasHeight);
        await ctx.SetFillStyleAsync("#98AFC7");
        await ctx.FillRectAsync(0, 0, TrueCanvasWidth, TrueCanvasHeight);

        await ctx.SetStrokeStyleAsync("Black");
        await ctx.StrokeRectAsync(0, 0, TrueCanvasWidth, TrueCanvasHeight);
    }

    public FoPage2D SetCurrentPage(FoPage2D page)
    {
        PageManager.SetCurrentPage(page);
        PanZoomService.ReadFromPage(page);
        return page;
    }
    
    public void ClearAll()
    {
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

            var region = UserWindow();
            page.ComputeShouldRender(region);
        }
        catch (System.Exception ex)
        {
            $" DoCreate {action.Method.Name} {ex.Message}".WriteNote();
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
                var data = item.ImageUrl;
                // $"{data}".WriteLine();
            });
            FoImage2D.RefreshImages = false;
        }

        return AllImages;
    }


    public List<FoVideo2D> GetAllVideos()
    {
        if (FoVideo2D.RefreshVideos)
        {
            AllVideos.Clear();
            PageManager.CollectVideos(AllVideos, true);

            AllVideos.ForEach(item =>
            {
                var data = item.ImageUrl;
                $"{data}".WriteNote();
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
            PanZoomShape = new FoPanZoomWindow(PageManager, PanZoomService, HitTestService, this, "Silver");
            PanZoomShape.SizeToFit();
            PanZoomShape.IsVisible = false;

            var page = PageManager.CurrentPage();
            var pt = new Point(page.PageWidth.AsPixels() / 2, new Length(3.0, "in").AsPixels());
            PanZoomShape.MoveTo(pt.X, pt.Y);
        }
        return PanZoomShape;
    }

    public void TogglePanZoomWindow()
    {
        PanZoomWindow().IsVisible = !PanZoomWindow().IsVisible;
    }

    public void ToggleHitTestDisplay()
    {
        PageManager.ToggleHitTestRender();
    }




    public void RefreshHitTest_IfDirty()
    {
        if (FoGlyph2D.ResetHitTesting)
            PageManager.RefreshHitTesting(PanZoomWindow());

        FoGlyph2D.ResetHitTesting = false;
    }


    public virtual void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Main", new Dictionary<string, Action>()
        {

            { "Clear", () => ClearAll()},

            { "Group", () => PageManager?.GroupSelected<FoGroup2D>()},
            // { "Ungroup", () => PageManager.UngroupSelected<FoGroup2D>()},

        }, true);


        // https://coastalcreative.com/standard-paper-sizes/
        space.EstablishMenu2D<FoMenu2D, FoButton2D>("Page Size", new Dictionary<string, Action>()
        {
            // { "ANSI A (Letter)", () => PageManager.SetPageSizeInches(8.5,11)}, //8.5” x 11”
            // { "ANSI B (Tabloid)", () => PageManager.SetPageSizeInches(8.5,17)},  //11” × 17”
            // { "ANSI C", () => PageManager.SetPageSizeInches(17,22)}, //17” × 22”
            // { "ANSI D", () => PageManager.SetPageSizeInches(22,34)}, //22” × 34”
            // { "ANSI E", () => PageManager.SetPageSizeInches(34,44)}, //34” × 44”
            { "A0", () => PageManager.SetPageSize(840,1120, "mm")},
            { "A1", () => PageManager.SetPageSize(600, 840, "mm")},
            { "A2", () => PageManager.SetPageSize(420, 600, "mm")},
            { "A3", () => PageManager.SetPageSize(300, 420, "mm")},
            { "A4", () => PageManager.SetPageSize(200, 300, "mm")},
            { "Landscape", () => { PageManager.SetPageLandscape(); ResetPanZoom(); } },
            { "Portrait", () => { PageManager.SetPagePortrait(); ResetPanZoom(); } },
        }, true)
        .ToggleLayout().MoveTo(0, 80);

    }




    public bool UserWindowMovedTo(Point loc)
    {
        var region = SetUserWindow(loc);
        var page = PageManager.CurrentPage();
        page.ComputeShouldRender(region);

        //  $"UserWindowResized {rect.X} {rect.Y} {rect.Width} {rect.Height} ---".WriteLine(ConsoleColor.Blue);
        return true;
    }
    public bool UserWindowResized(Size size)
    {
        var region = SetUserWindow(size);
        var page = PageManager.CurrentPage();

        page.ComputeShouldRender(region);

        //  $"UserWindowResized {rect.X} {rect.Y} {rect.Width} {rect.Height} ---".WriteLine(ConsoleColor.Blue);
        return true;
    }

    public async Task RenderDrawing(Canvas2DContext ctx, int tick, double fps)
    {

        //skip this frame is still working 
        if (IsCurrentlyProcessing) return;

        FoGlyph2D.Animations.Update((float)0.033);

        var wasDirty = FoGlyph2D.ResetHitTesting;
        RefreshHitTest_IfDirty();

        var page = PageManager.CurrentPage();

        await ClearCanvas(ctx);

        await ctx.SaveAsync();

        var (zoom, panx, pany) = await PanZoomService.TranslateAndScale(ctx, page);

        if (PreRender != null)
            await PreRender.Invoke(ctx, tick);

        await PageManager.RenderDetailed(ctx, tick, true);

        if (PostRender != null)
            await PostRender.Invoke(ctx, tick);

        await PanZoomWindow().RenderConcise(ctx, zoom, page.Rect());

        await ctx.RestoreAsync();



        await GetInteraction().RenderDrawing(ctx, tick);

        if ( !ShowStats ) return;


        var offsetY = 60;
        var offsetX = 1400;

        await ctx.SetTextAlignAsync(Blazor.Extensions.Canvas.Canvas2D.TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);

        await ctx.SetFillStyleAsync(wasDirty ? "#FF0000" : "#000000");
        await ctx.SetFontAsync("26px Segoe UI");
        await ctx.FillTextAsync($"Foundry Canvas {UserID} {InputStyle}", offsetX, offsetY);

        await ctx.SetFontAsync("18px consolas");
        //await ctx.FillTextAsync($"zoom: {zoom:0.00} panx: {panx} panx: {pany} fps: {fps:0.00}", offsetX, offsetY + 25);
        await ctx.FillTextAsync($"fps: {fps:0.00} zoom {zoom:0.00} panx: {panx} panx: {pany}", offsetX, offsetY + 25);
        //await ctx.FillTextAsync($"{page.Name}  {ScaleDrawing.CanvasWH()} {page.DrawingWH()}", offsetX, offsetY + 50);

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
        var win = UserWindow();
        await ctx.StrokeRectAsync(-win.X + 10, -win.Y + 10, win.Width - 20, win.Height - 20);
    }

    protected bool TestRule(BaseInteraction interact, CanvasMouseArgs args)
    {
        if (interact.IsDefaultTool(args) == false)
        {
            //$"{style} No Match".WriteError();
            return false;
        }

        var style = interact.Style;
        //$"{style} Match".WriteSuccess();
        SetInteraction(style);
        return true;
    }

    protected virtual IBaseInteraction SelectInteractionByRuleFor(CanvasMouseArgs args)
    {
        foreach (var rule in interactionRules)
        {
            if (TestRule(rule, args))
                return GetInteraction();
        }
        SetInteraction(InteractionStyle.ReadOnly);
        return GetInteraction();
    }

    private void ApplyMouseArgs(CanvasMouseArgs args)
    {
        if (args == null) return;

        try
        {
            SetCurrentlyProcessing(true);
            // call IsDefaultTool method on each interaction to
            // determine what is the right interaction for this case?

            var isEventHandled = (args.Topic) switch
            {
                ("ON_MOUSE_DOWN") => SelectInteractionByRuleFor(args).MouseDown(args),
                ("ON_MOUSE_MOVE") => GetInteraction().MouseMove(args),
                ("ON_MOUSE_UP") => GetInteraction().MouseUp(args),
                ("ON_MOUSE_IN") => GetInteraction().MouseIn(args),
                ("ON_MOUSE_OUT") => GetInteraction().MouseOut(args),
                _ => false
            };
        }
        catch (Exception ex)
        {
            $" {args.Topic} {ex.Message}".WriteNote();
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
                $" {args.Topic} {ex.Message}".WriteNote();
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
                 $" {args.Topic} {ex.Message}".WriteNote();
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
                $" {args.Topic} {ex.Message}".WriteNote();
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
                $" {args.Topic} {ex.Message}".WriteNote();
            }
        });
    }


    public bool WheelChange(CanvasWheelChangeArgs args)
    {
        PanZoomService.ZoomWheel(args.DeltaY);
        PanZoomService.WriteToPage(PageManager.CurrentPage());
        //$"Wheel change: {args.DeltaX}, {args.DeltaY}, {args.DeltaZ}".WriteLine(ConsoleColor.Yellow);
        return true;
    }

    public bool MovePanBy(int dx, int dy)
    {
        PanZoomService.PanBy(dx, dy);
        UserWindowMovedTo(PanZoomService.Pan());

        PanZoomService.WriteToPage(PageManager.CurrentPage());

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
        object success = (args.Code, args.AltKey, args.CtrlKey, args.ShiftKey) switch
        {
            ("ArrowUp", false, true, true) => MovePanBy(0, -move * 10),
            ("ArrowDown", false, true, true) => MovePanBy(0, move * 10),
            ("ArrowLeft", false, true, true) => MovePanBy(-move * 10, 0),
            ("ArrowRight", false, true, true) => MovePanBy(move * 10, 0),

            ("ArrowUp", false, false, _) => MoveSelectionsBy(0, -move),
            ("ArrowDown", false, false, _) => MoveSelectionsBy(0, move),
            ("ArrowLeft", false, false, _) => MoveSelectionsBy(-move, 0),
            ("ArrowRight", false, false, _) => MoveSelectionsBy(move, 0),

            ("ArrowUp", true, false, false) => ZoomSelectionBy(1.25),
            ("ArrowDown", true, false, false) => ZoomSelectionBy(0.75),

            ("KeyG", true, false, false) => PageManager?.GroupSelected<FoGroup2D>() != null,
            ("KeyR", true, false, false) => RotateSelectionsBy(30),
            ("KeyL", true, false, false) => LayoutSelections(),
            ("KeyO", true, false, false) => OpenEdit(),
            ("KeyC", true, false, false) => OpenCreate(),

            ("KeyD", false, true, false) => DuplicateSelections(),
            ("Insert", false, false, false) => DuplicateSelections(),
            ("Delete", false, false, false) => DeleteSelections(),
            ("Delete", false, true, false) => DeleteSelectionsWithAnimations(),
            _ => false
        };
        return success != null;
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
