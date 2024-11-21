using BlazorComponentBus;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;


public class ShapeDragging : ShapeHovering
{
    private bool isDraggingShapes = false;


    public ShapeDragging(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pubsub,
            ToolManagement tools
        ) : base(priority, cursor, draw, pubsub, tools)
    {
        ToolType = ToolManagement.InteractionStyle<ShapeDragging>();
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);
        var selected = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top
        return selected != null;
        //return selectionService.Selections().Count > 0;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"DRAGINF Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        isDraggingShapes = false;
        var selectionService = GetSelectionService();

        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService()?.FindGlyph(DragArea);
        var hitShape = findings?.LastOrDefault();
        hitShape?.OnShapeClick(ClickStyle.MouseDown, args);


        selectedShape = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top
        if (selectedShape != null)
        {
            selectionService.MouseStartDrag();
        }

        if (selectedShape != null)
        {
            isDraggingShapes = true;
        }
        else if (hitShape != null && !hitShape.IsSelected)
        {
            selectionService.ClearAll();
            selectionService.AddItem(hitShape);
            isDraggingShapes = true;
        }
        else
        {
            selectionService.ClearAll();
        }

        //$"Mouse Down {isDraggingShapes}".WriteLine(ConsoleColor.Green);
        return true;
    }
    public override bool MouseUp(CanvasMouseArgs args)
    {
        selectedShape?.OnShapeClick(ClickStyle.MouseUp, args);
        isDraggingShapes = false;
        SetInteraction<ShapeHovering>();
        GetSelectionService().MouseDropped();
        return true;
    }

    public override bool MouseMove(CanvasMouseArgs args)
    {
        if (isDraggingShapes)
        {
            var panZoomService = GetPanZoomService();
            DragArea = panZoomService.HitRectStart(args);
            var move = panZoomService.MouseDeltaMovement();
            //$"MouseMove isDraggingShapes {move.X} {move.Y}".WriteSuccess();

            drawing.MoveSelectionsBy(move.X, move.Y);
        }
        else
        {
            base.MouseMove(args); // this should hover        
        }


        return true;
    }

}