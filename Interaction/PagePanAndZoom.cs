using FoundryBlazor.Canvas;
using BlazorComponentBus;


namespace FoundryBlazor.Shape;


public class PagePanAndZoom : BaseInteraction
{
    private bool isDraggingPage = false;

    public PagePanAndZoom(
            InteractionStyle style,
            int priority,
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ): base(style,priority,draw,pubsub,panzoom,select,manager,hitTest)
    {
    }
    
    public override void Abort()
    {     
        isDraggingPage = false;
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        return args.CtrlKey && args.ShiftKey;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        isDraggingPage = true;
        return true;
    }
    public override bool MouseUp(CanvasMouseArgs args)
    {
        isDraggingPage = false;
        drawing.SetInteraction(InteractionStyle.ShapeHovering);
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {
        if ( isDraggingPage )
            drawing.MovePanBy(args.MovementX, args.MovementY);

        return true;
    }

}