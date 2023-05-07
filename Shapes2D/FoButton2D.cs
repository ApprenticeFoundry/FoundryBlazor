using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using System.Xml.Linq;


namespace FoundryBlazor.Shape;



public class FoButton2D : FoGlyph2D, IFoButton
{
    public Action? OnClick;
    private int countdown = 0;
    public bool ComputeResize { get; set; } = false;

    private string text = "";
    public string Text { get { return this.text; } set { this.text = AssignText(value, text); } }

    private string fontsize = "30";
    public string FontSize { get { return this.fontsize; } set { this.fontsize = AssignText(value, fontsize); } }

    private string font = "Segoe UI";
    public string Font { get { return this.font; } set { this.font = AssignText(value, font); } }

    public string TextColor { get; set; } = "White";

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

    protected string AssignText(string newValue, string oldValue)
    {
        if (newValue != oldValue)
        {
            //ComputeResize = true;
        }

        return newValue;
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

    public FoButton2D(string command, Action action) : base(command, "Orange")
    {
        FontSize = "20";
        Font = "Segoe UI";
        Text = command;
        OnClick = action;
        ResizeTo(75, 50);

        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {
        //$"FoText2D Draw {Text} {Width} {Height}   ".WriteLine(ConsoleColor.DarkMagenta);   

        await ctx.FillRectAsync(0, 0, Width, Height);

        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync(TextColor);
        await ctx.FillTextAsync(Text, LeftX() + 1, TopY() + 1);

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
