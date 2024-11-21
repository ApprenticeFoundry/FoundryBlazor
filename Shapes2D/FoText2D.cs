using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared.SVG;

// https://goldfirestudios.com/canvasinput-html5-canvas-text-input
// https://blog.steveasleep.com/how-to-draw-multi-line-text-on-an-html-canvas-in-2021

namespace FoundryBlazor.Shape;

public class FoText2D : FoShape2D, IShape2D
{
    public bool ComputeResize { get; set; } = false;

    private string text = "";
    public string Text { get { return this.text; } set { this.text = CreateDetails(AssignText(value, text)); } }

    private string fontsize = "20";
    public string FontSize { get { return this.fontsize; } set { this.fontsize = AssignText(value, fontsize); } }

    private string font = "Segoe UI";
    public string Font { get { return this.font; } set { this.font = AssignText(value, font); } }

    public string TextColor { get; set; } = "White";
    public bool IsEditing { get; set; } = false;

    public int Margin { get; set; } = 2;
    public List<string>? Details { get; set; }
    public bool AllowResize { get; set; } = false;


    public FoText2D() : base()
    {
    }
    public FoText2D(string name) : base(name, "Orange")
    {
        Text = name;
    }

    public FoText2D(int width, int height, string color) : base("", width, height, color)
    {
        Text = "";
    }
    public FoText2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        Text = name;
    }
    public override FoDynamicRender GetDynamicRender()
    {
        foDynamicRender ??= new FoDynamicRender(typeof(Text2D), this);
        return foDynamicRender;
    }

    protected string CreateDetails(string details="")
    {

        Details = details.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        return details;
    }

    protected string AssignText(string newValue, string oldValue)
    {
        if (newValue != oldValue)
        {
            ComputeResize = true;
        }

        return newValue;
    }

    public override string GetText()
    {
        return Text;
    }
    public string FontSizeAndName()
    {
        //styles -   normal | italic | oblique | inherit
        //weights - normal | bold | bolder | lighter | 100 | 200 | 300 | 400 | 500 | 600 | 700 | 800 | 900 | inherit | auto
        // face - serif, sans-serif, cursive, fantasy, and monospace.

        //[font style] [font weight] [font size] [font face]

        //"normal bold 80px sans-serif"
        //"italic bold 24px serif"
        //"normal lighter 50px cursive"
        return $"{FontSize}px {Font}";
    }

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {
        //$"FoText2D Draw {Text} {Width} {Height}   ".WriteLine(ConsoleColor.DarkMagenta);   

        await ctx.FillRectAsync(0, 0, Width, Height);

        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);

        await ctx.SetFillStyleAsync(TextColor);
        await ctx.FillTextAsync(Text, LeftX() + 5, TopY() + 5);
    }





    // https://jenkov.com/tutorials/html5-canvas/text.html

    public async Task ComputeSize(Canvas2DContext ctx, string Fragment)
    {
        if (string.IsNullOrEmpty(Fragment))
        {
            ComputeResize = false;
            return;
        }

        var result = await ComputeMeasuredText(ctx, Fragment, FontSize, Font);
        Height = result.Height + Margin;
        Width = result.Width + Margin;
        ComputeResize = result.Failure;
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();
        await ctx.SetFontAsync(FontSizeAndName());

        if (AllowResize)
            if (IsEditing || ComputeResize)
                await ComputeSize(ctx, Text);

        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        HoverDraw?.Invoke(ctx, this);
        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
            RenderDeepDetailed(ctx, tick);
            
        await ctx.RestoreAsync();
        return true;
    }








}
