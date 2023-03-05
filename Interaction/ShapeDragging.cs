using BlazorComponentBus;
using FoundryBlazor.Canvas;

namespace FoundryBlazor.Shape;


public class ShapeDragging : ShapeHovering
{
    private bool isDraggingShapes = false;


    public ShapeDragging(
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ): base(draw,pubsub,panzoom,select,manager,hitTest)
    {
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        var findings = pageManager?.FindGlyph(dragArea);
        var selected = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top
        return selected != null;
        //return selectionService.Selections().Count > 0;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        isDraggingShapes = false;

        dragArea = panZoomService.HitRectStart(args);
        var findings = pageManager?.FindGlyph(dragArea);
        var hitShape = findings?.LastOrDefault(); 
        hitShape?.OnClick(ClickStyle.MouseDown);

        selectedShape = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top

        if (selectedShape != null)
        {
            isDraggingShapes = true;
        }
        else if ( hitShape != null && !hitShape.IsSelected )
        {
            selectionService.ClearAll();
            selectionService?.AddItem(hitShape);
            isDraggingShapes = true;
        } 
        else {
            selectionService.ClearAll();
        }

        //$"Mouse Down {isSelecting}".WriteLine(ConsoleColor.Green);
        return true;
    }
    public override bool MouseUp(CanvasMouseArgs args)
    {
        selectedShape?.OnClick(ClickStyle.MouseUp);
        isDraggingShapes = false;
        drawing.SetInteraction(InteractionStyle.ShapeHovering);
        return true;
    }

    public override bool MouseMove(CanvasMouseArgs args)
    {
        if (isDraggingShapes) {
            dragArea = panZoomService.HitRectStart(args);
            var move = panZoomService.Movement();

            drawing.MoveSelectionsBy(move.X, move.Y);
        }
        else {
            base.MouseMove(args); // this should hover        
        }


        return true;
    }

}