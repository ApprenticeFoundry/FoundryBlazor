
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Canvas;


namespace FoundryBlazor.Shape;

public class ShapeSelection : ShapeHovering
{


    private bool isFenceSelecting = false;


    public ShapeSelection(
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
        return true;
    }  

    public override void Abort()
    {     
        isFenceSelecting = false;
    }

    public override async Task RenderDrawing(Canvas2DContext ctx, int tick)
    {
        if (isFenceSelecting)
        {
            await ctx.BeginPathAsync();
            await ctx.SetLineDashAsync(new float[] { 50, 10 });
            await ctx.SetLineWidthAsync(3);
            await ctx.SetStrokeStyleAsync("White");
            var rect = panZoomService.TransformRect(dragArea);
            await ctx.StrokeRectAsync(rect.X, rect.Y, rect.Width, rect.Height);
            await ctx.StrokeAsync();
        }
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        isFenceSelecting = false;
        var mustClear = args.ShiftKey == false;

        
        dragArea = panZoomService.HitRectStart(args);
        var findings = pageManager?.FindGlyph(dragArea);

        var hitShape = findings?.LastOrDefault();
        hitShape?.OnClick(ClickStyle.MouseDown);
        
        selectedShape = findings?.Where(item => item.IsSelected).LastOrDefault(); 
        if ( hitShape != null) 
        {
            if ( !hitShape.IsSelected )
            {
                selectionService?.ClearAllWhen(mustClear);
                selectionService?.AddItem(hitShape);

                //Restart this interaction in Drag Shape mode
                drawing.SetInteraction(InteractionStyle.ShapeDragging);
                var interact = drawing.GetInteraction();
                interact.MouseDown(args);
            }
        } else {

            isFenceSelecting = true;
            selectionService?.ClearAllWhen(mustClear);
        }


        // this is a fence select

        //$"ShapeSelection Mouse Down ".WriteLine(ConsoleColor.Green);
        return true;
    }

    public override bool MouseUp(CanvasMouseArgs args)
    {
        if (isFenceSelecting)
        {
            dragArea = panZoomService.Normalize(dragArea);

            var findings = pageManager?.FindGlyph(dragArea);
            if (findings != null)
            {
                //anything that intersects
                //selectionService?.AddRange(findings);
                
                //only findings that are totally inside the fence
                foreach (var item in findings)
                {
                    if ( dragArea.Contains(item.Rect()) )
                        selectionService?.AddItem(item);  
                }
            }
        }

        //dragArea = panZoomService.HitRectStart(args);
        isFenceSelecting = false;
        //$"ShapeSelection Mouse Up ".WriteLine(ConsoleColor.Green);
        drawing.SetInteraction(InteractionStyle.ShapeHovering);
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {
        //SendUserMove(args, true);

        if (isFenceSelecting)
        {
            dragArea = panZoomService.HitRectContinue(args, dragArea);
        }
        else if ( selectionService.Selections().Count > 0 )
        {
            dragArea = panZoomService.HitRectStart(args);
            var move = panZoomService.Movement();

            drawing.MoveSelectionsBy(move.X, move.Y);
        }
        else
            base.MouseMove(args); // this should hover

        //$"ShapeSelection Mouse Move ".WriteLine(ConsoleColor.Green);
        return true;
    }

}