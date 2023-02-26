
using BlazorComponentBus;

using FoundryBlazor.Canvas;

namespace FoundryBlazor.Shape;


public class AllEvents : BaseInteraction
{
 

    private bool isDraggingPage = false;
    private bool isDraggingShapes = false;
    private bool isFenceSelecting = false;
    private bool isResizingShape = false;



    private FoDragTarget2D? dragTarget;



    public AllEvents(
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ): base(draw,pubsub,panzoom,select,manager,hitTest)
    {
    }
    
    public override bool MouseDown(CanvasMouseArgs args)
    {
      //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        // SendToast(UserToast.Info("the mouse info"));

        isDraggingPage = false;
        isDraggingShapes = false;
        isFenceSelecting = false;
        isResizingShape = false;


        if (args.CtrlKey && args.ShiftKey)
        {
            isDraggingPage = true;
            return true;
        }


        dragArea = panZoomService.HitRectStart(args);
        var findings = pageManager?.FindGlyph(dragArea);
        selectedShape = findings?.Where(item => item.IsSelected).LastOrDefault(); // get one on top


        if (args.AltKey && findings?.Count == 0)
        {
            //$"Mouse DoCreate {DoCreate}  {args.AltKey} {findings} ".WriteLine(ConsoleColor.Yellow);
            //DoCreate?.Invoke(args);
            return true;
        }

        if (args.AltKey && findings?.Count == 1 && selectedShape != null)
        {
            //adjust the drag ares to upper left corner of the box 
            isResizingShape = true;

            dragArea = panZoomService.HitRectTopLeft(args, selectedShape.Rect());
            return true;
        }

        // this is a fence select
        if (findings?.Count == 0)
        {
            selectionService?.ClearAll();
            isFenceSelecting = true;
            return true;
        }

        var menu = findings?.Where(item => item is FoMenu2D).LastOrDefault() as FoMenu2D;
        if (menu?.MouseHit(args) == true) return true;

        var videoButtons = findings?.Where(item => item is FoVideo2D).LastOrDefault() as FoVideo2D;
        if (videoButtons?.MouseHit(args) == true) return true;

        var compound = findings?.Where(item => item is FoCompound2D).LastOrDefault() as FoCompound2D;
        if (compound?.MouseHit(args) == true) return true;


        if (!args.ShiftKey)
            selectionService.ClearAll();



        if (args.MetaKey)
        {
            Console.WriteLine("MetaKey with mousedown");
        }



        if (args.CtrlKey && selectedShape is FoCompound2D SourceShape)
        {
            //var shapeC = pageManager?.MorphTo<FoCompound2D,FoDragTarget2D>(shapeA);

            dragTarget = new FoDragTarget2D(15, 15, "Yellow");
            dragTarget.MoveTo(SourceShape.PinX, SourceShape.PinY);
            pageManager?.AddShape<FoDragTarget2D>(dragTarget);

            var connector = new FoShape1D(SourceShape, dragTarget, 15, dragTarget.Color);
            pageManager?.AddShape<FoShape1D>(connector);
            dragTarget.Connector = connector;

            selectionService?.ClearAll();
            selectionService?.AddItem(dragTarget);
            return true;
        }

        if (findings?.Count >= 1)
        {
            selectionService.AddItem(findings.Last());
        }
        isDraggingShapes = selectedShape != null || findings!.Count == 1;

        //$"Mouse Down {isSelecting}".WriteLine(ConsoleColor.Green);

        return true;
    }
    public override bool MouseUp(CanvasMouseArgs args)
    {
      if (isDraggingPage)
        {
            isDraggingPage = false;
        }

        if (isFenceSelecting)
        {
            dragArea = panZoomService.Normalize(dragArea);

            var findings = pageManager?.FindGlyph(dragArea);
            if (findings != null) selectionService?.AddRange(findings);
        }

        if (isResizingShape && selectedShape != null)
        {
            var newSize = panZoomService.HitRectContinue(args, dragArea);
            selectedShape.ResizeToBox(newSize);
        }


        if (dragTarget != null && selectedShape != null)
        {
            var hits = panZoomService.HitRectStart(args);
            var TargetShape = pageManager?.FindGlyph(hits).LastOrDefault();

            //delete the dragTarget and it's connector
            dragTarget.Connector!.UnglueAll();
            //dragTarget.UnglueAll();
            pageManager?.ExtractShapes(dragTarget.Connector!.GlyphId);
            pageManager?.ExtractShapes(dragTarget.GlyphId);

            if (TargetShape != selectedShape && TargetShape is FoCompound2D target)
            {
                target.ApplyLayout = true;  //set this when data is pushed
                var shapeB = new FoShape1D(selectedShape, TargetShape, 8, "Green");
                pageManager?.AddShape<FoShape1D>(shapeB);
                // var pointer = EdgeSimulation?.ModelConnect(SelectedShape.GlyphId, TargetShape.GlyphId);
                // pointer?.ApplyExternalMethods(shapeB);
            }
            selectedShape = null;
            dragTarget = null;
        }



        isDraggingPage = false;
        isDraggingShapes = false;
        isFenceSelecting = false;
        isResizingShape = false;

        return true;
    }
    public override bool MouseMove(CanvasMouseArgs args)
    {
        //SendUserMove(args, true);

        var loc = panZoomService.HitRectStart(args);
        var move = panZoomService.Movement();

        //$"Mouse Move {loc.X}  {loc.Y}".WriteLine();

        if (isDraggingPage)
        {
            //MovePanBy(args.MovementX, args.MovementY);
            return true;
        }


        if (isFenceSelecting)
        {
            dragArea = panZoomService.HitRectContinue(args, dragArea);
            return true;
        }

        if (isResizingShape && selectedShape != null)
        {
            var newSize = panZoomService.HitRectContinue(args, dragArea);
            selectedShape.ResizeToBox(newSize);
        }


        if (dragTarget != null)
        {
            //lastHoverTarget?.ForEach(child => child.HoverDraw = null);
            //lastHoverTarget = pageManager.FindGlyph(loc);

            // lastHoverTarget?.Where(child => child is FoCompound2D && child != selectedShape)
            //     .ForEach(child => child.HoverDraw = OnHoverTarget);

            // MoveSelectionsBy(move.X, move.Y);

            return true;
        }

        if (isDraggingShapes)
        {
            //MoveSelectionsBy(move.X, move.Y);
            return true;
        }


        lastHover?.ForEach(child => child.HoverDraw = null);
        lastHover?.ForEach(child => child.LocalMouseHover(args, null));

        lastHover = pageManager!.FindGlyph(loc);
        lastHover.ForEach(child => child.HoverDraw = OnHover);
        lastHover.ForEach(child => child.LocalMouseHover(args, OnSubHover));
        return true;
    }
    public override bool MouseIn(CanvasMouseArgs args)
    {
        return true;
    }
    public override bool MouseOut(CanvasMouseArgs args)
    {
        return true;
    }
}