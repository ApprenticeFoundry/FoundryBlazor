
 
using BlazorComponentBus;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;


namespace FoundryBlazor.Shape;


public class ShapeHovering : BaseInteraction
{
    public ShapeHovering(
            InteractionStyle style,
            int priority,
            string cursor,
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ) : base(style, priority, cursor, draw, pubsub, panzoom, select, manager, hitTest)
    {
    }

    public override bool MouseMove(CanvasMouseArgs args)
    {
        var list = new List<ShapeHoverUIEvent>();

        lastHover?.ForEach(child =>
        {
            child.HoverDraw = null;
            list.Add(new ShapeHoverUIEvent(child));
        });
        lastHover?.ForEach(child => child.LocalMouseHover(args, null));



        var loc = panZoomService.HitRectStart(args);
        lastHover = hitTestService!.FindGlyph(loc);

        lastHover.ForEach(child =>
        {
            child.HoverDraw = OnHover;
            list.Add(new ShapeHoverUIEvent(child));
        });
        lastHover.ForEach(child => child.LocalMouseHover(args, OnSubHover));


        //list.ForEach(item => pubsub.Publish(item));    

        return true;
    }

}