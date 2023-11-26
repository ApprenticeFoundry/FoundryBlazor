
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;

public interface IInteractionManager
{
    IBaseInteraction SelectInteractionByRuleFor(CanvasMouseArgs args);
    void SetInteraction<T>() where T : BaseInteraction;
}



public class InteractionManager : IInteractionManager
{
    protected List<BaseInteraction> interactionRules;
    protected Dictionary<string, BaseInteraction> interactionLookup;
    protected string Style = BaseInteraction.InteractionStyle<BaseInteraction>();
    private IBaseInteraction? lastInteraction;


    private IHitTestService HitTestService { get; set; }
    private IPanZoomService PanZoomService { get; set; }
    private ISelectionService SelectionService { get; set; }
    private IPageManagement PageService { get; set; }

    public InteractionManager (
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
            {new PagePanAndZoom(1010, "pointer", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new MentorConstruction(105, "default", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new MoShapeLinking(100, "default", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new ShapeMenu(90,"default", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new ShapeConnecting(80,"default", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new ShapeResizing(70,"nwse-resize", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new ShapeDragging(50,"grab", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new ShapeSelection(40,"default", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new ShapeHovering(30,"move", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
            {new BaseInteraction(0,"default", draw, pubsub, PanZoomService, SelectionService, PageService, HitTestService)},
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
        var style = BaseInteraction.InteractionStyle<T>();
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
}
