using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;


public class ShapeMenu : ShapeDragging
{

    public ShapeMenu(
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
        var menu = findings?.LastOrDefault() as FoMenu2D;
        return menu != null;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        dragArea = panZoomService.HitRectStart(args);
        var findings = pageManager?.FindGlyph(dragArea);

        var menu = findings?.LastOrDefault() as FoMenu2D;
        menu?.OnShapeClick(ClickStyle.MouseDown, args);
        if (menu?.MouseHit(args)) 
            return true;

        return base.MouseDown(args);
    }

}