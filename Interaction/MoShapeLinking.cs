
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Shared;



namespace FoundryBlazor.Shape;


public class MoShapeLinking : ShapeHovering
{

    protected FoDragTarget2D? dragTarget;
    protected List<FoCompound2D>? lastHoverTarget = null;


    public MoShapeLinking(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pubsub,
            ToolManagement tools
        ) : base(priority, cursor, draw, pubsub, tools)
    {
        ToolType = ToolManagement.InteractionStyle<MoShapeLinking>();
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);
        selectedShape = findings?.LastOrDefault(); // get one on top
        if (args.CtrlKey && selectedShape is FoCompound2D)
            return true;

        return false;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {

        if (args.CtrlKey && selectedShape is FoCompound2D SourceShape)
        {
            //var shapeC = pageManager?.MorphTo<FoCompound2D,FoDragTarget2D>(shapeA);
            var pageManager = GetPageService();
            var selectionService = GetSelectionService();

            dragTarget = new FoDragTarget2D(15, 15, "Yellow");
            dragTarget.MoveTo(SourceShape.PinX, SourceShape.PinY);
            pageManager?.AddShape<FoDragTarget2D>(dragTarget);

            var connector = new FoShape1D(SourceShape, dragTarget, 5, dragTarget.Color);
            pageManager?.AddShape<FoShape1D>(connector);
            dragTarget.Connector = connector;

            selectionService?.ClearAll();
            selectionService?.AddItem(dragTarget);
        }

        return true;
    }

    public override bool MouseUp(CanvasMouseArgs args)
    {
        if (dragTarget != null && selectedShape != null)
        {
            var hits = GetPanZoomService().HitRectStart(args);

            lastHoverTarget = GetHitTestService().FindGlyph(hits).Where(child => child is FoCompound2D && child != selectedShape).Cast<FoCompound2D>().ToList();
            var TargetShape = lastHoverTarget.LastOrDefault();

            //delete the dragTarget and it's connector
            dragTarget.Connector!.UnglueAll();
            //dragTarget.UnglueAll();
            GetPageService().ExtractShapes(dragTarget.Connector!.GlyphId);
            GetPageService().ExtractShapes(dragTarget.GlyphId);

            if (TargetShape != null)
            {
                TargetShape.ApplyLayout = true;  //set this when data is pushed
                var shapeB = new FoShape1D(selectedShape, TargetShape, 8, "Green");
                GetPageService().AddShape<FoShape1D>(shapeB);
            }


            lastHoverTarget?.ForEach(child => child.HoverDraw = null);
            selectedShape = null;
            dragTarget = null;
        }
         var selectionService = GetSelectionService();
        selectionService?.MouseDropped();
        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {

        var panZoomService = GetPanZoomService();
        var loc = panZoomService.HitRectStart(args);
        var move = panZoomService.MouseDeltaMovement();

        //$"Mouse Move {loc.X}  {loc.Y}".WriteLine();


        if (dragTarget != null)
        {
            lastHoverTarget?.ForEach(child => child.HoverDraw = null);
            lastHoverTarget = GetHitTestService().FindGlyph(loc).Where(child => child is FoCompound2D && child != selectedShape).Cast<FoCompound2D>().ToList();

            lastHoverTarget?.ForEach(child => child.HoverDraw = OnHoverTarget);

            dragTarget?.MoveBy(move.X, move.Y);

            return true;
        }

        //base.MouseMove(args); // this should hover

        return true;
    }
    public Action<Canvas2DContext, FoGlyph2D>? OnHoverTarget { get; set; } = async (ctx, obj) =>
    {
        var thickness = 20;
        var half = thickness / -2;

        await ctx.SaveAsync();

        //await ctx.SetLineDashAsync(new float[] { 10, 10 });
        await ctx.SetLineWidthAsync(thickness);
        await ctx.SetStrokeStyleAsync("Orange");
        await ctx.StrokeRectAsync(half, half, obj.Width + thickness, obj.Height + thickness);

        await ctx.RestoreAsync();
    };
}