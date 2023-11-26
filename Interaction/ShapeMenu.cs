using BlazorComponentBus;
 
using FoundryBlazor.Extensions;
using FoundryBlazor.Shared;

namespace FoundryBlazor.Shape;


public class ShapeMenu : ShapeDragging
{

    public ShapeMenu(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pubsub,
            ToolManagement tools
        ) : base(priority, cursor, draw, pubsub, tools)
    {
        ToolType = ToolManagement.InteractionStyle<ShapeMenu>();
    }

    public override bool IsDefaultTool(CanvasMouseArgs args)
    {
        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);
        var menu = findings?.LastOrDefault() as FoMenu2D;
        return menu != null;
    }

    public override bool MouseDown(CanvasMouseArgs args)
    {
        //$"Mouse Down {args.OffsetX} {args.OffsetY}, {args.AltKey} ".WriteLine(ConsoleColor.Green);

        DragArea = GetPanZoomService().HitRectStart(args);
        var findings = GetHitTestService().FindGlyph(DragArea);

        var menu = findings?.LastOrDefault() as FoMenu2D;
        menu?.OnShapeClick(ClickStyle.MouseDown, args);
        if (menu?.MouseHit(args) == true)
            return true;

        return base.MouseDown(args);
    }

}