
using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoundryBlazor.Shape;



public interface IDrawing : IRender
{

    void SetCanvasSize(int width, int height);
    Point InchesToPixelsInset(double width, double height);

    void CreateMenus(IJSRuntime js, NavigationManager nav);


    List<FoImage2D> GetAllImages();
    List<FoVideo2D> GetAllVideos();
    List<IFoMenu> CollectMenus(List<IFoMenu> list);

    FoMenu2D EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu2D;
    FoPanZoomWindow PanZoomWindow();

    Task RenderDrawing(Canvas2DContext ctx, int tick, double fps);

    void SetDoCreate(Action<CanvasMouseArgs> action);

    V? AddShape<V>(V shape) where V : FoGlyph2D;
    FoPage2D CurrentPage();
    IPageManagement Pages();
    List<FoGlyph2D> ExtractShapes(string glyphId);

    void TogglePanZoomWindow();
    void ToggleHitTestDisplay();
    bool MovePanBy(int dx, int dy);


}

public class FoDrawing2D : FoGlyph2D, IDrawing
{

    public static bool IsCurrentlyRendering { get; set; } = false;
    private InputStyle InputStyle = InputStyle.None;
    public D2D_UserMove? UserLocation { get; set; }
    public Dictionary<string, D2D_UserMove> OtherUserLocations { get; set; } = new();
    public Action<CanvasMouseArgs>? DoCreate { get; set; }


    private FoPanZoomWindow? _panZoomWindow { get; set; }

    private IPageManagement PageManager { get; set; }
    private IScaledDrawingHelpers ScaleDrawing { get; set; }
    private IHitTestService HitTestService { get; set; }
    private IPanZoomService PanZoomService { get; set; }
    private ISelectionService SelectionService { get; set; }


    public List<FoImage2D> AllImages = new();
    public List<FoVideo2D> AllVideos = new();
    private ComponentBus PubSub { get; set; }



    private readonly Dictionary<InteractionStyle, IBaseInteraction> interactionLookup;
    private InteractionStyle interactionStyle = InteractionStyle.None;
    private IBaseInteraction? lastInteraction;


    public FoDrawing2D (
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
            {InteractionStyle.None, new BaseInteraction(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.PagePanAndZoom, new PagePanAndZoom(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeHovering, new ShapeHovering(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeSelection, new ShapeSelection(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeDragging, new ShapeDragging(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeResizing, new ShapeResizing(this, pubSub, panzoom, select, manager, hittest)},
            {InteractionStyle.ShapeConnecting, new ShapeConnecting(this, pubSub, panzoom, select, manager, hittest)},
           //{InteractionStyle.AllEvents, new AllEvents(this, pubSub, panzoom, select, manager, hittest)},
        };


        InitSubscriptions();
        PanZoomService.SetOnComplete(() =>
        {
            //SRS refresh zoom if changed
            ResetPanZoom();
        });
    }

    public void SetInteraction(InteractionStyle style)
    {
        if ( interactionStyle == style) return;
        lastInteraction?.Abort();
        interactionStyle = style;
        lastInteraction = null;

        $"SetInteraction {interactionStyle}".WriteLine(ConsoleColor.Green);
    }

    public IBaseInteraction GetInteraction()
    {
        lastInteraction ??= interactionLookup[interactionStyle];
        return lastInteraction;
    }

    public List<FoGlyph2D> ExtractShapes(string GlyphId)
    {
        return PageManager.ExtractShapes(GlyphId);
    }

    public Point InchesToPixelsInset(double width, double height)
    {
        return ScaleDrawing.InchesToPixelInset(width, height);
    }

    public void SetCanvasSize(int width, int height)
    {
        ScaleDrawing.SetCanvasSize(width, height);
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
    public V? AddShape<V>(V shape) where V : FoGlyph2D
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
                item.ImageUrl.WriteLine();
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
                item.ImageUrl.WriteLine();
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
        if (_panZoomWindow == null)
        {
            _panZoomWindow = new FoPanZoomWindow(PageManager, PanZoomService, HitTestService, ScaleDrawing, "Silver");
            _panZoomWindow.SizeToFit();

            var page = PageManager.CurrentPage();
            var pt = InchesToPixelsInset(page.PageWidth / 2, 3.0);
            _panZoomWindow.MoveTo(pt.X, pt.Y);
        }
        return _panZoomWindow;
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
            { "Clear", () => PageManager?.ClearAll()},
            { "Group", () => PageManager?.GroupSelected<FoGroup2D>()},
            //{ "Ungroup", () => PageManager.UngroupSelected<FoGroup2D>()},
            //{ "Save", () => Command?.Save()},
            //{ "Restore", () => Command?.Restore()},
            { "Pan Zoom", () => TogglePanZoomWindow()},
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

        //EdgeSimulation = new MoSimulation("TVA Edge", PageManager, this, DialogService!);
        //EstablishMenu<FoMenu2D>("Simulation", EdgeSimulation.MenuItems(js), true);

        PageManager.SetPageSizeInches(34, 44);
        //PanZoomService.SetZoom(0.5);
        //PanZoomService.SetPan(75, 25);
        //PageManager.SetPageLandscape();
        //ResetPanZoom();
        //PanZoomWindow().IsVisible = false;

        // var plan = "ProcessPlan";
        // ProcessPlan = new DTARSolution(DTARRestService, this, null);
        // EstablishMenu<FoMenu2D>(plan, ProcessPlan.ProcessMenuItems(plan, js), true);

        // var treeName = "Documents";
        // DocumentTree = new DTARSolution(DTARRestService, this, null);
        // EstablishMenu<FoMenu2D>(treeName, DocumentTree.DocumentMenuItems(treeName, js), true);

        // var RefreshMenus = (string url) =>
        // {
        //     DTARRestService.SetServerUrl(url);
        //     EstablishMenu<FoMenu2D>(plan, DocumentTree.DocumentMenuItems(treeName, js), true);
        //     EstablishMenu<FoMenu2D>(treeName, DocumentTree.DocumentMenuItems(treeName, js), true);
        //     CreateCommands(js, nav);
        // };

        // EstablishMenu<FoMenu2D>("DTAR Server", new Dictionary<string, Action>()
        // {
        //     { "localhost:5001", () => RefreshMenus("https://localhost:5001")},
        //     { "rondtar", () => RefreshMenus("https://rondtar.azurewebsites.net/")},
        //     { "dtarwebdev", () => RefreshMenus("https://dtarwebdev.azurewebsites.net/")},
        // }, true);


        // var BoidCount = 10;
        // BoidSimulation = new BoidField(Command, PageManager);
        // BoidSimulation.CreateBoids(BoidCount, BoidField.RandomColor());
        // EstablishMenu<FoMenu2D>("Boid", BoidSimulation.MenuItems(),true)
        //     .LayoutVertical(100, 40).MoveTo(120, 80);


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
        //BoidSimulation?.Advance();
        

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


        tick++;

        await GetInteraction().RenderDrawing(ctx, tick);


        //this is the mouse 
        OtherUserLocations.Values.ForEach(async user =>
        {
            await ctx.SetStrokeStyleAsync("#FFFFFF");
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(user.X, user.Y);
            await ctx.LineToAsync(user.X + 20, user.Y + 15);
            await ctx.LineToAsync(user.X, user.Y + 20);
            await ctx.LineToAsync(user.X, user.Y);
            await ctx.ClosePathAsync();
            await ctx.FillAsync();

            await ctx.SetStrokeStyleAsync("#000000");
            await ctx.SetFontAsync("16px Segoe UI");
            await ctx.SetTextAlignAsync(Blazor.Extensions.Canvas.Canvas2D.TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);
            await ctx.FillTextAsync(user.PanID, user.X + 5, user.Y + 20);
        });

        var offsetY = 60;
        var offsetX = 1400;

        await ctx.SetTextAlignAsync(Blazor.Extensions.Canvas.Canvas2D.TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);

        var PanID = "NO PANID";
        await ctx.SetFillStyleAsync(wasDirty ? "#FF0000" : "#000000");
        await ctx.SetFontAsync("26px Segoe UI");
        await ctx.FillTextAsync($"Foundry Canvas {PanID} {InputStyle}", offsetX, offsetY);

        await ctx.SetFontAsync("18px consolas");
        //await ctx.FillTextAsync($"zoom: {zoom:0.00} panx: {panx} panx: {pany} fps: {fps:0.00}", offsetX, offsetY + 25);
        await ctx.FillTextAsync($"fps: {fps:0.00}", offsetX, offsetY + 25);
        await ctx.FillTextAsync($"{page.Name}  {ScaleDrawing.CanvasWH()} {page.DrawingWH()}", offsetX, offsetY + 50);

        int loc = 50;
        OtherUserLocations.Values.ForEach(async user =>
        {
            loc += 15;
            await ctx.SetTextAlignAsync(Blazor.Extensions.Canvas.Canvas2D.TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);
            await ctx.FillTextAsync($"{user.PanID} is helping", 20 + offsetX, loc);
        });

    }

    private async Task DrawUserWindow(Canvas2DContext ctx)
    {
        // draw the current window
        await ctx.SetStrokeStyleAsync("Black");
        await ctx.SetLineWidthAsync(10.0F);
        var win = ScaleDrawing.UserWindow();
        await ctx.StrokeRectAsync(-win.X + 10, -win.Y + 10, win.Width - 20, win.Height - 20);
    }

    private bool TestRule(InteractionStyle style, CanvasMouseArgs args) 
    {
        var interact = interactionLookup[style];
        if (interact.IsDefaultTool(args) == false)
        {
            $"{style} No Match".WriteLine(ConsoleColor.Red);
            return false;
        }

        $"{style} Match".WriteLine(ConsoleColor.Green);
        SetInteraction(style);
        return true;
    }

    private void SelectInteractionByRuleFor(CanvasMouseArgs args) 
    {
        if ( TestRule(InteractionStyle.PagePanAndZoom,args) ) return;

        if ( TestRule(InteractionStyle.ShapeConnecting,args) ) return;

        if ( TestRule(InteractionStyle.ShapeDragging,args) ) return;

        TestRule(InteractionStyle.ShapeSelection,args);
    }



    private void InitSubscriptions()
    {
        PubSub!.SubscribeTo<InputStyle>(style => {
            InputStyle = style;
        });

        PubSub!.SubscribeTo<CanvasMouseArgs>( args =>
        {
            try
            {
                if (IsCurrentlyRendering)
                {
                    //you should cashe the args to replayed latter
                    //when the UI is not rendering..
                    // return;
                    "is rendering ".WriteSuccess(2);
                } else
                {
                     //"is NOT rendering ".WriteSuccess(2);
                }
                // call IsDefaultTool method on each interaction to
                // determine what is the right interaction for this case?
                
                //if ( args.Topic.Matches("ON_MOUSE_DOWN"))
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
        });

       PubSub!.SubscribeTo<CanvasKeyboardEventArgs>( args =>
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

        PubSub!.SubscribeTo<CanvasWheelChangeArgs>( args =>
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

        PubSub!.SubscribeTo<CanvasResizeArgs>( args =>
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

        _panZoomWindow?.Smash();  //Anit scale and move
        return true;
    }

    public bool MoveSelectionsBy(int dx, int dy)
    {
        PageManager.SelectionsMoveBy(dx, dy);
        //PageManager.Selections().ForEach(shape => SendShapeMoved(shape));
        return true;
    }
    public bool RotateSelectionsBy(int angle)
    {
        PageManager.SelectionsRotateBy(angle);
        //PageManager.Selections().ForEach(shape => SendShapeMoved(shape));
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
                //SendShapeDestroy(shape);
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
