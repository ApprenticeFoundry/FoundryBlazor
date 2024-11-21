
 
using BlazorComponentBus;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;


namespace FoundryBlazor.Shape;


public class ShapeHovering : BaseInteraction
{
    public ShapeHovering(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pubsub,
            ToolManagement tools
        ) : base(priority, cursor, draw, pubsub, tools)
    {
        ToolType = ToolManagement.InteractionStyle<ShapeHovering>();
    }

    public override bool MouseMove(CanvasMouseArgs args)
    {
        //var list = new List<ShapeHoverUIEvent>();
        var panzoomservice = GetPanZoomService();
        var loc = panzoomservice.HitLocation(args);

        lastHover?.ForEach(child =>
        {
            child.ClearHoverDraw();
            //list.Add(new ShapeHoverUIEvent(child));
        });
        lastHover?.ForEach(child => child.LocalMouseHover(args, loc, null));



        var hit = panzoomservice.HitRectStart(args);
        lastHover = GetHitTestService().FindGlyph(hit);

        lastHover.ForEach(child =>
        {
            child.SetHoverDraw(OnHover!);
            //list.Add(new ShapeHoverUIEvent(child));
        });
        lastHover.ForEach(child => child.LocalMouseHover(args, loc, OnSubHover));


        //SRS do you realy want to send events for every hover?
        //list.ForEach(item => pubsub.Publish(item));    

        return true;
    }

}