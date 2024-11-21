using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
 
using FoundryBlazor.Extensions;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;



public class FoMenu2D : FoGlyph2D, IFoMenu, IShape2D
{
    private string _layout = "H";

    public FoMenu2D(string name) : base(name,100,50,"Purple")
    {
        ShapeDraw = DrawRect;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public FoMenu2D AddButton(string name, Action action) 
    {
        var button = new FoButton2D(name, action);
        Add<FoButton2D>(button);
        return this;
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
        return Key;
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
        var found = Members<FoButton2D>().Where(item => item.HitTestRect().Contains(pt)).FirstOrDefault();
        if ( found != null) {

            found.MarkSelected(true);
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
