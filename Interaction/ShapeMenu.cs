using BlazorComponentBus;
 
using FoundryBlazor.Extensions;
using FoundryBlazor.Shared;

namespace FoundryBlazor.Shape;


public class ShapeMenu : ShapeDragging
{

    public ShapeMenu(
            InteractionStyle style,
            int priority,
            string cursor,
            FoDrawing2D draw,
            ComponentBus pubsub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ) : base(style, priority, cursor, draw, pubsub, panzoom, select, manager, hitTest)
    {
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        DragArea = panZoomService.HitRectStart(args);
        var findings = hitTestService?.FindGlyph(DragArea);
        var menu = findings?.LastOrDefault() as FoMenu2D;
        return menu != null;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        DragArea = panZoomService.HitRectStart(args);
        var findings = hitTestService?.FindGlyph(DragArea);

        var menu = findings?.LastOrDefault() as FoMenu2D;
        menu?.OnShapeClick(ClickStyle.MouseDown, args);
        if (menu?.MouseHit(args) == true)
            return true;

        return base.MouseDown(args);
    }

}