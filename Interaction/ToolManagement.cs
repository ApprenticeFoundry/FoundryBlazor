
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;

public interface IToolManagement :IBaseInteraction
{
    void CreateInteractions(IDrawing draw, ComponentBus pubsub);
    void SetInteraction<T>() where T : BaseInteraction;
    IBaseInteraction GetInteraction();

    
    void MouseDropped();

}



public class ToolManagement : IToolManagement
{
    protected List<BaseInteraction> interactionRules = new();
    protected Dictionary<string, BaseInteraction> interactionLookup = new();
    protected string Style = InteractionStyle<BaseInteraction>();
    private IBaseInteraction? lastInteraction;


    private IHitTestService HitTestService { get; set; }
    private IPanZoomService PanZoomService { get; set; }
    private ISelectionService SelectionService { get; set; }
    private IPageManagement PageService { get; set; }

    public ToolManagement (
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hittest
        )
    {
        HitTestService = hittest;
        PanZoomService = panzoom;
        SelectionService = select;
        PageService = manager;
    }

    public static string InteractionStyle<T>() where T : IBaseInteraction
    {
        return typeof(T).Name;
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
    
    public virtual IBaseInteraction SelectInteractionByRuleFor(CanvasMouseArgs args)
    {
        foreach (var rule in interactionRules)
        {
            if (TestRule(rule, args))
                return GetInteraction();
        }
        SetInteraction<BaseInteraction>();
        return GetInteraction();
    }

    public void CreateInteractions(IDrawing draw, ComponentBus pubsub)
    {
       // https://www.w3schools.com/cssref/pr_class_cursor.php
        interactionRules = new List<BaseInteraction>()
        {
            {new PagePanAndZoom(1010, "pointer", draw, pubsub, this)},
            {new MentorConstruction(105, "default", draw, pubsub, this)},
            {new MoShapeLinking(100, "default", draw, pubsub, this)},
            {new ShapeMenu(90,"default", draw, pubsub, this)},
            {new ShapeConnecting(80,"default", draw, pubsub, this)},
            {new ShapeResizing(70,"nwse-resize", draw, pubsub, this)},
            {new ShapeDragging(50,"grab", draw, pubsub, this)},
            {new ShapeSelection(40,"default", draw, pubsub, this)},
            {new ShapeHovering(30,"move", draw, pubsub, this)},
            {new BaseInteraction(0,"default", draw, pubsub, this)},
        };

        interactionLookup = new Dictionary<string, BaseInteraction>();
        interactionRules.ForEach(rule =>
        {
            interactionLookup.Add(rule.Style, rule);
        });

        lastInteraction = interactionRules[0];
    }

    public void AddInteraction(string style, BaseInteraction interaction)
    {
        interactionLookup.Add(style, interaction);

        //$"SetInteraction {interactionStyle}".WriteSuccess();
    }
    public void SetInteraction<T>() where T : BaseInteraction
    {
        var style = ToolManagement.InteractionStyle<T>();
        SetInteraction(style);
    }
    public void SetInteraction(string style)
    {
        if (Style == style) return;
        lastInteraction?.Abort();
        Style = style;
        lastInteraction = null;

        //$"SetInteraction {interactionStyle}".WriteSuccess();
    }

    public IBaseInteraction GetInteraction()
    {
        lastInteraction ??= interactionLookup[Style];
        return lastInteraction;
    }

    public string GetCursor()
    {
        return GetInteraction().GetCursor();
    }

    public Task RenderDrawing(Canvas2DContext ctx, int tick)
    {
        return GetInteraction().RenderDrawing(ctx, tick);
    }

    public bool MouseDown(CanvasMouseArgs args)
    {
        return SelectInteractionByRuleFor(args).MouseDown(args);
    }

    public bool MouseUp(CanvasMouseArgs args)
    {
        return GetInteraction().MouseUp(args);
    }

    public bool MouseMove(CanvasMouseArgs args)
    {
        return GetInteraction().MouseMove(args);
    }

    public void MouseDropped()
    {
        SelectionService.MouseDropped();
    }

    public void Abort()
    {
         GetInteraction().Abort();
    }

    public bool IsDefaultTool(CanvasMouseArgs args)
    {
        return GetInteraction().IsDefaultTool(args);
    }

    public IPageManagement GetPageService()
    {
        return PageService;
    }

    public IHitTestService GetHitTestService()
    {
        return HitTestService;
    }

    public IPanZoomService GetPanZoomService()
    {
        return PanZoomService;
    }

    public ISelectionService GetSelectionService()
    {
        return SelectionService;
    }
}
