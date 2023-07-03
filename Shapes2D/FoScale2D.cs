using Blazor.Extensions.Canvas.Canvas2D;
using IoBTMessage.Units;

namespace FoundryBlazor.Shape;

public class FoScale2D
{
    public Length Drawing { get; set; } = new Length(1.0, "cm");  //cm
    public Length World { get; set; } = new Length(1.0, "m");  //cm

    public string Display ()
    {
        var result = World / Drawing;
        return $"{World} = {Drawing} scale {result}:1";
    }

    public double Scale()
    {
        var result = World / Drawing;
        return result;
    }

}

public class FoHorizontalRuler2D
{
    public FoScale2D Scale { get; set; } 
    public FoPage2D Page { get; set; }

    public  FoHorizontalRuler2D(FoScale2D scale2D, FoPage2D page2D)
    {
        Scale = scale2D;
        Page = page2D;
    }
    public async Task DrawRuler(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dWidth = Page.PageWidth.AsPixels() + dMargin;
        var dHalf = dMargin / 2.0;

        var background = "Orange";
        await ctx.SetFillStyleAsync(background);
        await ctx.FillRectAsync(dMargin, dHalf, dWidth, dHalf);

        await ctx.SetLineWidthAsync(1);
        await ctx.SetFontAsync("8 px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Bottom);


        var x = dMargin; //left;
        int i = 0;
        var cnt = 0.0;
        while (x <= dWidth)
        {
            if (i % 10 != 0) {
                await ctx.SetStrokeStyleAsync("White");
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(x, dHalf);
                await ctx.LineToAsync(x, dMargin);
                await ctx.StrokeAsync();
                await ctx.SetFillStyleAsync("Black");
                await ctx.FillTextAsync($"{cnt:F1}", x, dMargin-5);
            }
            x += dMinor;
            cnt += 0.1;
            i++;
        }
        await ctx.SetFontAsync("Bold 22 px Segoe UI");

        x = dMargin; //left;
        cnt = 0.0;
        while (x <= dWidth)
        {
            await ctx.SetStrokeStyleAsync("Black");
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin - 10);
            await ctx.LineToAsync(x, dMargin);
            await ctx.StrokeAsync();
            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync($"{cnt:F1}", x, dMargin - 10);
            x += dMajor;
            cnt += 1.0;
        }

        await ctx.RestoreAsync();
    }
}


public class FoVerticalRuler2D
{
    public FoScale2D Scale { get; set; }
    public FoPage2D Page { get; set; }

    public FoVerticalRuler2D(FoScale2D scale2D, FoPage2D page2D)
    {
        Scale = scale2D;
        Page = page2D;
    }
    public async Task DrawRuler(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dHeight = Page.PageHeight.AsPixels() + dMargin;
        var dHalf = dMargin / 2.0;

        var background = "Orange";
        await ctx.SetFillStyleAsync(background);
        await ctx.FillRectAsync(dHalf, dMargin, dHalf, dHeight);

        await ctx.SetLineWidthAsync(1);
        await ctx.SetFontAsync("8 px Segoe UI");
        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);


        var y = dMargin; //top;
        int i = 0;
        var cnt = 0.0;
        while (y <= dHeight)
        {
            if (i % 10 != 0)
            {
                await ctx.SetStrokeStyleAsync("White");
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(dHalf,y);
                await ctx.LineToAsync(dMargin,y);
                await ctx.StrokeAsync();
                await ctx.SetFillStyleAsync("Black");
                await ctx.FillTextAsync($"{cnt:F1}", dHalf+15, y);
            }
            y += dMinor;
            cnt += 0.1;
            i++;
        }
        await ctx.SetFontAsync("Bold 22 px Segoe UI");

        y = dMargin; //top;
        cnt = 0.0;
        while (y <= dHeight)
        {
            await ctx.SetStrokeStyleAsync("Black");
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin - 10,y);
            await ctx.LineToAsync(dMargin,y);
            await ctx.StrokeAsync();
            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync($"{cnt:F1}", dHalf+10, y);
            y += dMajor;
            cnt += 1.0;
        }

        await ctx.RestoreAsync();
    }
}
