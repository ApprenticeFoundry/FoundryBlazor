using Blazor.Extensions.Canvas.Canvas2D;


namespace FoundryBlazor.Shape;

public class FoSVGShape2D : FoShape2D, IShape2D
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


    public FoSVGShape2D() : base()
    {
    }
    public FoSVGShape2D(string name) : base(name, "Orange")
    {
        Text = name;
    }

    public FoSVGShape2D(int width, int height, string color) : base("", width, height, color)
    {
        Text = "Hello Everyone";
    }

    protected string CreateDetails(string details = "")
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


        await ctx.RestoreAsync();
        return true;
    }

    public string GetMarkup()
    {
        string SVG = @"<path d='M52,94l-26-19l9-8l-16-10l-17-38l9-17l15,30l26,20l26-20l15-30l9,17l-17,38l-16,10l9,8l-26,19z' fill='#ddd' stroke='#aaa' /> <path d='M52,91l10-7l-10-7l-10,7zM40,83l4-11l-6-5l-9,8zM46,63l-32-23l7,16l12,7zM48,61l3-7l-46-34l7,15zM21,28l-10-22l-6,11zM58,63l32-23l-7,16l-12,7zM56,61l-3-7l46-34l-7,15z M83,28l10-22l6,11zM64,83l-4-11l6-5l9,8z' stroke-linejoin='round' stroke='#222' fill='#138' /> <circle r='5' cx='52' cy='69' stroke='#EEE' fill='#138' />";
        return SVG;
    }








}
