using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using IoBTMessage.Models;

namespace FoundryBlazor.Shape;



public class FoButton2D : FoText2D, IFoButton
{
    public Action? OnClick;
    private int countdown = 0;


    public Action ClickAction()
    {
        return OnClick ?? (() => { });
    }

    public string DisplayText()
    {
        return Text;
    }

    private void DoClickAnimation(Canvas2DContext ctx, FoGlyph2D obj)
    {
        this.Color = "Black";
        if (this.countdown == 0)
        {
            this.Color = "Orange";
            this.PreDraw = null;
        }
        countdown--;
    }



    public override FoGlyph2D MarkSelected(bool value)
    {
        "You Clicked the button".WriteLine();
        countdown = 10;
        this.PreDraw = this.DoClickAnimation;

        OnClick?.Invoke();

        return base.MarkSelected(false);
    }

    public override FoGlyph2D ResizeTo(int width, int height)
    {
        ComputeResize = false;
        return base.ResizeTo(width, height);
    }

    public FoButton2D(string command, Action action) : base(command)
    {
        FontSize = "20";
        Font = "Segoe UI";
        Text = command;
        OnClick = action;
        ResizeTo(75, 50);

        ResetLocalPin((obj) => 0, (obj) => 0);
    }



    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        HoverDraw?.Invoke(ctx, this);
        PostDraw?.Invoke(ctx, this);

        await ctx.RestoreAsync();
        return true;
    }


}
