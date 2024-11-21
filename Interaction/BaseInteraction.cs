using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

using BlazorComponentBus;
using FoundryBlazor.Shared;



namespace FoundryBlazor.Shape;


public interface IBaseInteraction
{
    bool MouseDown(CanvasMouseArgs args);
    bool MouseUp(CanvasMouseArgs args);
    bool MouseMove(CanvasMouseArgs args);

    void Abort();
    void SetActive();
    Task RenderDrawing(Canvas2DContext ctx, int tick);
    bool IsDefaultTool(CanvasMouseArgs args);
    string GetCursor();
    string GetToolType();

    IPageManagement GetPageService();
    IHitTestService GetHitTestService();
    IPanZoomService GetPanZoomService();
    ISelectionService GetSelectionService();

}

public class BaseInteraction : FoComponent, IBaseInteraction
{
    public static Rectangle DragArea = new();



    public int Priority { get; set; } = 0;
    public string Cursor { get; set; } = "default";
    public string ToolType { get; set; } = "none";

    public ToolManagement ToolManager { get; set; }

    protected FoGlyph2D? selectedShape;

    protected List<FoGlyph2D>? lastHover = null;

    protected IDrawing drawing;
    protected ComponentBus pubsub;


    public BaseInteraction(
            int priority,
            string cursor,
            IDrawing draw,
            ComponentBus pub,
            ToolManagement manager
        ) : base()
    {
        Priority = priority;
        Cursor = cursor;
        drawing = draw;
        pubsub = pub;
        ToolManager = manager;
        ToolType = ToolManagement.InteractionStyle<BaseInteraction>();
    }

    protected void SetInteraction<T>() where T : BaseInteraction
    {
        ToolManager.SetInteraction<T>();
    }

    public Action<Canvas2DContext, FoGlyph2D>? OnHover { get; set; } = async (ctx, obj) =>
    {
        await ctx.SaveAsync();
        await ctx.SetLineWidthAsync(8);
        await ctx.SetStrokeStyleAsync("Red");

        if ( obj is FoShape1D shape1d)
        {
            var start = shape1d.Start();
            var finish = shape1d.Finish();

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(start.X, start.Y);
            await ctx.LineToAsync(finish.X, finish.Y);
            await ctx.ClosePathAsync();
        }
        else
        {
            await ctx.StrokeRectAsync(0, 0, obj.Width, obj.Height);
        }

        await ctx.StrokeAsync();

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

    public static Rectangle GetDragArea()
    {
        return DragArea;
    }

    public virtual void Abort()
    {
    }

    public virtual void SetActive()
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
        GetSelectionService().MouseDropped();
        return false;
    }
    public virtual bool MouseMove(CanvasMouseArgs args)
    {
        return false;
    }

    public string GetCursor()
    {
        return Cursor;
    }

    public IPageManagement GetPageService()
    {
        return ToolManager.GetPageService();
    }

    public IHitTestService GetHitTestService()
    {
        return ToolManager.GetHitTestService();
    }

    public IPanZoomService GetPanZoomService()
    {
        return ToolManager.GetPanZoomService();
    }

    public ISelectionService GetSelectionService()
    {
        return ToolManager.GetSelectionService();
    }

    public string GetToolType()
    {
        return ToolType;
    }
    // public virtual bool MouseIn(CanvasMouseArgs args)
    // {
    //     return false;
    // }
    // public virtual bool MouseOut(CanvasMouseArgs args)
    // {
    //     return false;
    // }
}