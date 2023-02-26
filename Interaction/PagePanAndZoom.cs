using FoundryBlazor.Canvas;
using BlazorComponentBus;


namespace FoundryBlazor.Shape;


public class PagePanAndZoom : BaseInteraction
{
    private bool isDraggingPage = false;

    public PagePanAndZoom(
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ): base(draw,pubsub,panzoom,select,manager,hitTest)
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
      //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        // SendToast(UserToast.Info("the mouse info"));


        isDraggingPage = true;
        return true;
    }
    public override bool MouseUp(CanvasMouseArgs args)
    {
        isDraggingPage = false;
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {

        //$"Mouse Move {loc.X}  {loc.Y}".WriteLine();

        if ( isDraggingPage )
            drawing.MovePanBy(args.MovementX, args.MovementY);

        return true;
    }

}