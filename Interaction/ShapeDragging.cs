using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;


public class ShapeDragging : ShapeHovering
{
    private bool isDraggingShapes = false;


    public ShapeDragging(
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
        DragArea = panZoomService.HitRectStart(args);
        var findings = hitTestService?.FindGlyph(DragArea);
        var selected = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top
        return selected != null;
        //return selectionService.Selections().Count > 0;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        isDraggingShapes = false;

        DragArea = panZoomService.HitRectStart(args);
        var findings = hitTestService?.FindGlyph(DragArea);
        var hitShape = findings?.LastOrDefault(); 
        hitShape?.OnShapeClick(ClickStyle.MouseDown, args);


        selectedShape = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top
        if ( selectedShape != null ) 
        {
            selectionService.MouseStartDrag();
        }       

        if (selectedShape != null)
        {
            isDraggingShapes = true;
        }
        else if ( hitShape != null && !hitShape.IsSelected )
        {
            selectionService.ClearAll();
            selectionService.AddItem(hitShape);
            isDraggingShapes = true;
        } 
        else {
            selectionService.ClearAll();
        }

        //$"Mouse Down {isDraggingShapes}".WriteLine(ConsoleColor.Green);
        return true;
    }
    public override bool MouseUp(CanvasMouseArgs args)
    {
        selectedShape?.OnShapeClick(ClickStyle.MouseUp, args);
        isDraggingShapes = false;
        drawing.SetInteraction(InteractionStyle.ShapeHovering);
        selectionService?.MouseDropped();
        return true;
    }

    public override bool MouseMove(CanvasMouseArgs args)
    {
        if (isDraggingShapes) {
            //$"MouseMove isDraggingShapes".WriteLine(ConsoleColor.Green);
            DragArea = panZoomService.HitRectStart(args);
            var move = panZoomService.MouseDeltaMovement();

            drawing.MoveSelectionsBy(move.X, move.Y);
        }
        else {
            base.MouseMove(args); // this should hover        
        }


        return true;
    }

}