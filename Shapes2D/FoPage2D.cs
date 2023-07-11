using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;

namespace FoundryBlazor.Shape;



public class FoMenu2D : FoGlyph2D, IFoMenu, IShape2D
{
    private string _layout = "H";



    public FoMenu2D(string name) : base(name,100,50,"Grey")
    {
        ShapeDraw = DrawRect;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }


    public List<IFoButton> Buttons()
    {
        return GetMembers<FoButton2D>()?.Select(item => item as IFoButton).ToList() ?? new List<IFoButton>();
    }


    public FoMenu2D Clear()
    {
        GetMembers<FoButton2D>()?.Clear();
        return this;
    }

    public string DisplayText()
    {
        return Name;
    }


    public FoMenu2D ToggleLayout()
    {
        if ( _layout.Matches("V"))
            LayoutHorizontal();
        else
            LayoutVertical();
            
        return this;
    }

    public FoMenu2D LayoutHorizontal(int width=95, int height=40)
    {
        var x = 0;
        var y = 18;
        Members<FoButton2D>().ForEach(item =>
        {
            item.MoveTo(x, y);
            item.ResizeTo(width, height);
            x += width+5;
        });
        _layout = "H";
        ResizeTo(x, height+20);
        return this;
    }

    public FoMenu2D LayoutVertical(int width=95, int height=40)
    {
        var x = 3;
        var y = 18;
        Members<FoButton2D>().ForEach(item =>
        {
            item.MoveTo(x, y);
            item.ResizeTo(width, height);
            y += height + 10;
        });
        _layout = "V";
        ResizeTo(width+5, y);
        return this;
    }

    public bool MouseHit(CanvasMouseArgs args) 
    {
        var pt = new Point(args.OffsetX - LeftEdge(), args.OffsetY - TopEdge());
        var found = Members<FoButton2D>().Where(item => item.Rect().Contains(pt)).FirstOrDefault();
        if ( found != null) {

            found.MarkSelected(true);
            return true;
        }
        return false;
    }

    public override bool LocalMouseHover(CanvasMouseArgs args, Action<Canvas2DContext, FoGlyph2D>? OnHover) 
    {
        Members<FoButton2D>().ForEach(child => child.HoverDraw = null);
        var pt = new Point(args.OffsetX - LeftEdge(), args.OffsetY - TopEdge());
        var found = Members<FoButton2D>().Where(item => item.Rect().Contains(pt)).FirstOrDefault();
        if ( found != null) {
            found.HoverDraw = OnHover;
            return true;
        }

        return false;
    }



    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
         if ( CannotRender() ) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        HoverDraw?.Invoke(ctx, this);
        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if ( deep )
        {
            GetMembers<FoButton2D>()?.ForEach(async item => await item.RenderDetailed(ctx, tick, deep));
        }


        await ctx.RestoreAsync();
        return true;
    }


}
