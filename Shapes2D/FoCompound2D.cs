using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
 
using FoundryBlazor.Shared;

namespace FoundryBlazor.Shape;

public class FoCompound2D : FoGlyph2D, IShape2D
{
    public bool ApplyLayout = false;

    public FoCompound2D() : base()
    {
    }

    public FoCompound2D(int width, int height, string color) : base("", width, height, color)
    {
    }

    public override List<T> CollectMembers<T>(List<T> list, bool deep=true)
    {
        var members = GetMembers<T>();
        if ( members != null)
            list.AddRange(members);

        return list;
    }
    public bool MouseHit(CanvasMouseArgs args) 
    {
        var pt = new Point(args.OffsetX - LeftEdge(), args.OffsetY - TopEdge());
        var found = Members<FoButton2D>().Where(item => item.HitTestRect().Contains(pt)).FirstOrDefault();
        if ( found != null) {

            found.MarkSelected(true);
            //force a redraw after every command is executed
            return true;
        }
        return false;
    }

    public override bool LocalMouseHover(CanvasMouseArgs args, Rectangle loc, Action<Canvas2DContext, FoGlyph2D>? OnHover) 
    {
        Members<FoButton2D>().ForEach(child => child.ClearHoverDraw());
        var pt = new Point(args.OffsetX - LeftEdge(), args.OffsetY - TopEdge());
        var found = Members<FoButton2D>().Where(item => item.HitTestRect().Contains(pt)).FirstOrDefault();
        if ( found != null && OnHover != null) {
            found.SetHoverDraw(OnHover);
            return true;
        }

        return false;
    }

    public async IAsyncEnumerable<FoCompoundValue2D> ComputeTextSize(Canvas2DContext ctx)
    {
        foreach (var item in Members<FoCompoundValue2D>())
        {
            await item.ComputeSize(ctx);
            yield return item;
        }
    }

    private async Task DoLayout(Canvas2DContext ctx) 
    {
        //"DoLayout".WriteLine(ConsoleColor.DarkMagenta);

        IAsyncEnumerable<FoCompoundValue2D> shapes = ComputeTextSize(ctx);

        await foreach (var item in shapes)
        {
            item.Height += item.Margin;
            item.Width += item.Margin;
            //$"DoMeasure {item.Width}  {item.Height}".WriteLine(ConsoleColor.DarkMagenta);
        }

        Refresh(true);
    }

    public void Refresh(bool animation=false)
    {
        ApplyLayout = false;

        var list = new List<FoGlyph2D>();
        list.AddRange(Members<FoCompoundValue2D>());
        list.AddRange(Members<FoImage2D>());
        list.AddRange(Members<FoText2D>());

        LayoutVertical(list, animation);
    }

    private FoCompound2D LayoutVertical(List<FoGlyph2D> list, bool animation=false)
    {
        var x = 0;
        var y = 0;
        var w = Width;  // min size
                        
        //"LayoutVertical Start".WriteLine(ConsoleColor.DarkMagenta);
        list.ForEach(item =>
        {
            item.IsVisible = true;

            if ( !animation ) item.MoveTo(x, y);
            else item.AnimatedMoveTo(x, y);
            
            w = Math.Max(w, item.Width);
            y += item.Height + 2;
        });

        if ( !animation ) ResizeTo(w, y);
        else AnimatedResizeTo(w, y);

        list.ForEach(item =>
        {
            item.Width = Width;
            if ( item is FoImage2D )
                item.AnimatedGrowFromZero();
        });

        //"LayoutVertical Finish".WriteLine(ConsoleColor.DarkMagenta);

        return this;
    }

    public Action<Canvas2DContext, FoCompound2D>? DrawModelText;

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {
        await ctx.FillRectAsync(0, 0, Width, Height);

        DrawModelText?.Invoke(ctx, this);

        if (DrawModelText == null )
        {
            await ctx.SaveAsync();
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);

            //"normal bold 80px sans-serif"
            var FontSpec = $"normal bold 80px sans-serif";
            await ctx.SetFontAsync(FontSpec);

            await ctx.SetFillStyleAsync("White");
            await ctx.FillTextAsync(Key,LeftX()+1,TopY()+1);
            await ctx.RestoreAsync();
        }
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
         if ( CannotRender() ) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        if ( ApplyLayout)
            await DoLayout(ctx);


        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        HoverDraw?.Invoke(ctx, this);
        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if ( deep ) 
        {
            GetMembers<FoCompoundValue2D>()?.ForEach( async item => await item.RenderDetailed(ctx, tick, deep));  
            GetMembers<FoImage2D>()?.ForEach( async item => await item.RenderDetailed(ctx, tick, deep));    
            GetMembers<FoText2D>()?.ForEach( async item => await item.RenderDetailed(ctx, tick, deep));    
            GetMembers<FoButton2D>()?.ForEach( async item => await item.RenderDetailed(ctx, tick, deep));   
        }
        await ctx.RestoreAsync();
        return true;
    }

}
