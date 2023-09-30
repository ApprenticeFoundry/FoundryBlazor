
using BlazorComponentBus;
using FoundryBlazor.Shared;


namespace FoundryBlazor.Shape;


public class ShapeResizing : ShapeHovering
{

    private bool isResizingShape = false;


    public ShapeResizing(
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

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        DragArea = panZoomService.HitRectStart(args);
        var findings = hitTestService?.FindGlyph(DragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top
        if (args.CtrlKey && selectedShape is IImage2D)
            return true;

        return false;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {

        isResizingShape = false;

        DragArea = panZoomService.HitRectStart(args);
        var findings = hitTestService?.FindGlyph(DragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top


        if (args.CtrlKey && selectedShape is IImage2D)
        {
            //adjust the drag ares to upper left corner of the box 
            isResizingShape = true;

            DragArea = panZoomService.HitRectTopLeft(args, selectedShape.HitTestRect());
            return true;
        }

        return true;
    }

    public override bool MouseUp(CanvasMouseArgs args)
    {

        if (isResizingShape && selectedShape != null)
        {
            var newSize = panZoomService.HitRectContinue(args, DragArea);
            selectedShape.ResizeToBox(newSize);
        }

        isResizingShape = false;
        drawing.SetInteraction(InteractionStyle.ShapeHovering);
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {

        if (isResizingShape && selectedShape != null)
        {
            var newSize = panZoomService.HitRectContinue(args, DragArea);
            selectedShape.ResizeToBox(newSize);
        }
        else
        {
            base.MouseMove(args); // this should hover    
        }


        return true;
    }

}