
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
 
using FoundryBlazor.Extensions;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;

public class ShapeSelection : ShapeHovering
{


    //private bool isFenceSelecting = false;


    public ShapeSelection(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pubsub,
            ToolManagement tools
        ) : base(priority, cursor, draw, pubsub, tools)
    {
        ToolType = ToolManagement.InteractionStyle<ShapeSelection>();
    }
    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        return true;
    }

    public override void Abort()
    {
        GetPanZoomService().SetFenceSelecting(false);
    }

    public override async Task RenderDrawing(Canvas2DContext ctx, int tick)
    {
        if (GetPanZoomService().IsFenceSelecting())
        {
            await ctx.BeginPathAsync();
            await ctx.SetLineDashAsync(new float[] { 50, 10 });
            await ctx.SetLineWidthAsync(3);
            await ctx.SetStrokeStyleAsync("White");
            var rect = GetPanZoomService().TransformRect(DragArea);
            await ctx.StrokeRectAsync(rect.X, rect.Y, rect.Width, rect.Height);
            await ctx.StrokeAsync();
        }
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteSuccess();

        var panZoomService = GetPanZoomService();
        var selectionService = GetSelectionService();

        panZoomService.SetFenceSelecting(false);
        var mustClear = args.ShiftKey == false;


        DragArea = panZoomService.HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);

        var hitShape = findings?.LastOrDefault();
        hitShape?.OnShapeClick(ClickStyle.MouseDown, args);

        selectedShape = findings?.Where(item => item.IsSelected).LastOrDefault();
        if (hitShape != null)
        {
            if (!hitShape.IsSelected)
            {
                selectionService?.ClearAllWhen(mustClear);
                selectionService?.AddItem(hitShape);
                selectionService?.MouseFirstSelected();

                //Restart this interaction in Drag Shape mode
                SetInteraction<ShapeDragging>();
                drawing.Tools().MouseDown(args);
            }
            else
            {
                selectionService?.MouseReselect();
            }
        }
        else
        {
            panZoomService.SetFenceSelecting(true);
            selectionService?.ClearAllWhen(mustClear);
        }


        // this is a fence select

        //$"ShapeSelection Mouse Down ".WriteLine(ConsoleColor.Green);
        return true;
    }

    public override bool MouseUp(CanvasMouseArgs args)
    {
        var panZoomService = GetPanZoomService();
        if (panZoomService.IsFenceSelecting())
        {
            DragArea = panZoomService.Normalize(DragArea);

            var findings = GetHitTestService().FindGlyph(DragArea);
            if (findings != null)
            {
                //anything that intersects
                //selectionService?.AddRange(findings);

                //only findings that are totally inside the fence
                foreach (var item in findings)
                {
                    if (GetDragArea().Contains(item.HitTestRect()))
                        GetSelectionService().AddItem(item);
                }
            }
        }

        GetPanZoomService().SetFenceSelecting(false);
        //$"ShapeSelection Mouse Up ".WriteLine(ConsoleColor.Green);
        SetInteraction<ShapeHovering>();
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {

        var panZoomService = GetPanZoomService();
        if (panZoomService.IsFenceSelecting())
        {
            DragArea = panZoomService.HitRectContinue(args, DragArea);
        }
        else if (GetSelectionService().Selections().Count > 0)
        {
            DragArea = panZoomService.HitRectStart(args);
            var move = panZoomService.MouseDeltaMovement();

            drawing.MoveSelectionsBy(move.X, move.Y);
        }
        else
            base.MouseMove(args); // this should hover

        //$"ShapeSelection Mouse Move ".WriteLine(ConsoleColor.Green);
        return true;
    }

}