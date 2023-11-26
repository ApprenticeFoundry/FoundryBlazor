
using BlazorComponentBus;
using FoundryBlazor.Shared;


namespace FoundryBlazor.Shape;


public class ShapeResizing : ShapeHovering
{

    private bool isResizingShape = false;


    public ShapeResizing(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pubsub,
            ToolManagement tools
        ) : base(priority, cursor, draw, pubsub, tools)
    {
        Style = ToolManagement.InteractionStyle<ShapeResizing>();
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top
        if (args.CtrlKey && selectedShape is IImage2D)
            return true;

        return false;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {

        isResizingShape = false;

        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top


        if (args.CtrlKey && selectedShape is IImage2D)
        {
            //adjust the drag ares to upper left corner of the box 
            isResizingShape = true;

            DragArea = GetPanZoomService().HitRectTopLeft(args, selectedShape.HitTestRect());
            return true;
        }

        return true;
    }

    public override bool MouseUp(CanvasMouseArgs args)
    {

        if (isResizingShape && selectedShape != null)
        {
            var newSize = GetPanZoomService().HitRectContinue(args, DragArea);
            selectedShape.ResizeToBox(newSize);
        }

        isResizingShape = false;
        SetInteraction<ShapeHovering>();
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {

        if (isResizingShape && selectedShape != null)
        {
            var newSize = GetPanZoomService().HitRectContinue(args, DragArea);
            selectedShape.ResizeToBox(newSize);
        }
        else
        {
            base.MouseMove(args); // this should hover    
        }


        return true;
    }

}