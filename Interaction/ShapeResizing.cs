
using BlazorComponentBus;
using FoundryBlazor.Canvas;

namespace FoundryBlazor.Shape;


public class ShapeResizing : ShapeHovering
{
 
    private bool isResizingShape = false;


    public ShapeResizing(
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
    
     public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        dragArea = panZoomService.HitRectStart(args);
        var findings = pageManager?.FindGlyph(dragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top
        if (args.CtrlKey && selectedShape is IImage2D)
            return true;

        return false;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {

        isResizingShape = false;

        dragArea = panZoomService.HitRectStart(args);
        var findings = pageManager?.FindGlyph(dragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top


        if (args.CtrlKey && selectedShape is IImage2D)
        {
            //adjust the drag ares to upper left corner of the box 
            isResizingShape = true;

            dragArea = panZoomService.HitRectTopLeft(args, selectedShape.Rect());
            return true;
        }

        return true;
    }

    public override bool MouseUp(CanvasMouseArgs args)
    {

        if (isResizingShape && selectedShape != null)
        {
            var newSize = panZoomService.HitRectContinue(args, dragArea);
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
            var newSize = panZoomService.HitRectContinue(args, dragArea);
            selectedShape.ResizeToBox(newSize);
        } else {
            base.MouseMove(args); // this should hover    
        }


        return true;
    }

}