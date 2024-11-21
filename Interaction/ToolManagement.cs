
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;

public interface IToolManagement :IBaseInteraction
{
    T AddToolType<T>(int priority, string cursor, IDrawing draw, ComponentBus pubsub) where T : BaseInteraction;
    T AddInteraction<T>(string style, T interaction) where T : BaseInteraction;
    void CreateInteractions(IDrawing draw, ComponentBus pubsub);
    void SetInteraction<T>() where T : BaseInteraction;
    IBaseInteraction GetInteraction();

}



public class ToolManagement : IToolManagement
{
    protected List<BaseInteraction> interactionRules = new();
    protected Dictionary<string, BaseInteraction> interactionLookup = new();
    protected string ToolType = InteractionStyle<BaseInteraction>();
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

        var style = interact.ToolType;
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
        var rules = new List<BaseInteraction>()
        {
            {new PagePanAndZoom(1010, "pointer", draw, pubsub, this)},
            {new ShapeMenu(90,"default", draw, pubsub, this)},
            {new ShapeConnecting(80,"default", draw, pubsub, this)},
            {new ShapeResizing(70,"nwse-resize", draw, pubsub, this)},
            {new ShapeDragging(50,"grab", draw, pubsub, this)},
            {new ShapeSelection(40,"default", draw, pubsub, this)},
            {new ShapeHovering(30,"move", draw, pubsub, this)},
            {new BaseInteraction(0,"default", draw, pubsub, this)},
        };

        interactionLookup = new Dictionary<string, BaseInteraction>();
        rules.ForEach(rule =>
        {
            interactionLookup.Add(rule.ToolType, rule);
        });
        interactionRules = interactionLookup.Values.OrderByDescending(r => r.Priority).ToList();

        lastInteraction = null;
    }

    public T AddToolType<T>(int priority, string cursor, IDrawing draw, ComponentBus pubsub) where T : BaseInteraction
    {
        var style = InteractionStyle<T>();
        var item = Activator.CreateInstance(typeof(T), priority, cursor, draw, pubsub, this);
        return AddInteraction<T>(style, (T)item!);
    }

    public T AddInteraction<T>(string style, T interaction) where T : BaseInteraction
    {
        interactionLookup.Add(style, interaction);
        interactionRules = interactionLookup.Values.OrderByDescending(r => r.Priority).ToList();
        lastInteraction = interactionRules.LastOrDefault();

        //$"SetInteraction {interactionStyle}".WriteSuccess();
        return interaction;
    }
    public void SetInteraction<T>() where T : BaseInteraction
    {
        var style = ToolManagement.InteractionStyle<T>();
        SetInteraction(style);
    }
    public void SetInteraction(string style)
    {
        if (ToolType == style) 
            return;

        //$"SetInteraction Changed from {ToolType} to {style}".WriteSuccess();

        lastInteraction?.Abort();
        ToolType = style;
        lastInteraction = interactionLookup[ToolType];
        lastInteraction.SetActive();
    }

    public IBaseInteraction GetInteraction()
    {
        lastInteraction ??= interactionLookup[ToolType];
        //$"GetInteraction {lastInteraction.GetToolType()}".WriteSuccess();
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

    public string GetToolType()
    {
        return ToolType;
    }

    public void SetActive()
    {
        
    }
}
