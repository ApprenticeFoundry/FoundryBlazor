
using FoundryBlazor.Canvas;
using BlazorComponentBus;


namespace FoundryBlazor.Shape;


public class ShapeHovering : BaseInteraction
{
    public ShapeHovering(
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ): base(draw,pubsub,panzoom,select,manager,hitTest)
    {
    }

    public override bool MouseMove(CanvasMouseArgs args)
    {

        lastHover?.ForEach(child => child.HoverDraw = null);
        //lastHover?.ForEach(child => child.LocalMouseHover(args, null));

        var loc = panZoomService.HitRectStart(args);
        lastHover = pageManager!.FindGlyph(loc);

        lastHover.ForEach(child => child.HoverDraw = OnHover);
        //lastHover.ForEach(child => child.LocalMouseHover(args, OnSubHover));
        return true;
    }

}