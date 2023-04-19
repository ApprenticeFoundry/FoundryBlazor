using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

using BlazorComponentBus;

using FoundryBlazor.Canvas;

namespace FoundryBlazor.Shape;

public enum InteractionStyle
{
    ReadOnly,
    PagePanAndZoom,
    ShapeSelection,
    ShapeHovering,
    ShapeDragging,
    ShapeResizing,
    ShapeCreating,
    ShapeConnecting,
    ModelLinking,
    UserExtension,
}

public interface IBaseInteraction
{
    bool MouseDown(CanvasMouseArgs args);
    bool MouseUp(CanvasMouseArgs args);
    bool MouseMove(CanvasMouseArgs args);
    bool MouseIn(CanvasMouseArgs args);
    bool MouseOut(CanvasMouseArgs args);
    void Abort();
    Task RenderDrawing(Canvas2DContext ctx, int tick);
    bool IsDefaultTool(CanvasMouseArgs args);

}
public class BaseInteraction : FoComponent, IBaseInteraction
{
    public int Priority { get; set; } = 0;
    protected Rectangle dragArea;
    protected FoGlyph2D? selectedShape;
    
    protected List<FoGlyph2D>? lastHover = null;

    protected FoDrawing2D drawing;
    protected IPageManagement pageManager;
    protected ComponentBus pubsub;
    protected IHitTestService hitTestService;
    protected IPanZoomService panZoomService;
    protected ISelectionService selectionService;

    public BaseInteraction(
            FoDrawing2D draw,
            ComponentBus pub,
            IPanZoomService panzoom,
            ISelectionService select,
            IPageManagement manager,
            IHitTestService hitTest
        ): base()
        {
            drawing = draw;
            pubsub = pub;
            hitTestService = hitTest;
            selectionService = select;
            panZoomService = panzoom;
            pageManager = manager;
        }



    public Action<Canvas2DContext, FoGlyph2D>? OnHover { get; set; } = async (ctx, obj) =>
    {
        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(5);
        await ctx.SetStrokeStyleAsync("Red");
        await ctx.StrokeRectAsync(0, 0, obj.Width, obj.Height);

        await ctx.RestoreAsync();
    };

    public Action<Canvas2DContext, FoGlyph2D>? OnSubHover { get; set; } = async (ctx, obj) =>
    {
        await ctx.SaveAsync();

        await ctx.SetLineWidthAsync(5);
        await ctx.SetStrokeStyleAsync("Blue");
        await ctx.StrokeRectAsync(0, 0, obj.Width, obj.Height);

        await ctx.RestoreAsync();
    };

    public virtual void Abort()
    {     
    }

    public virtual async Task RenderDrawing(Canvas2DContext ctx, int tick)
    {
        await Task.CompletedTask;
    }

    public virtual bool IsDefaultTool(CanvasMouseArgs args)
    {
        return false;
    }

    public virtual bool MouseDown(CanvasMouseArgs args)
    {
        return false;
    }
    public virtual bool MouseUp(CanvasMouseArgs args)
    {
        selectionService.MouseDropped();
        return false;
    }
    public virtual bool MouseMove(CanvasMouseArgs args)
    {
        return false;
    }
    public virtual bool MouseIn(CanvasMouseArgs args)
    {
        return false;
    }
    public virtual bool MouseOut(CanvasMouseArgs args)
    {
        return false;
    }
}