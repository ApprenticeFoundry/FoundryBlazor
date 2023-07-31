using Blazor.Extensions.Canvas.Canvas2D;
using IoBTMessage.Units;

namespace FoundryBlazor.Shape;

public class FoScale2D
{
    public Length Drawing { get; set; } = new Length(1.0, "cm");  //cm
    public Length World { get; set; } = new Length(1.0, "m");  //m

    public string Display ()
    {
        var result = World / Drawing;
        return $"{World} = {Drawing} scale {result}:1";
    }



    public double ScaleToWorld()
    {
        var result = World / Drawing;
        return result;
    }
    public double ScaleToDrawing()
    {
        var result = Drawing / World;
        return result;
    }

    public double PixelToDrawing(int pixels)
    {
        var result = Drawing.FromPixels(pixels);
        return result;
    }
}

public class FoHorizontalRuler2D
{
    public bool IsVisible { get; set; } = false;
    public FoScale2D Scale { get; set; } 
    public FoPage2D Page { get; set; }


    public  FoHorizontalRuler2D(FoScale2D scale2D, FoPage2D page2D)
    {
        Scale = scale2D;
        Page = page2D;
    }
    public async Task DrawRuler(Canvas2DContext ctx, Length step, bool major)
    {
        if ( !IsVisible ) return;
        
        await ctx.SaveAsync();

        var dStep = step.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dir = Page.ScaleAxisX;
        var inc = Scale.World.Value();
        var dZero = Page.ZeroPointX.AsPixels() + dMargin;
        var dWidth = Page.PageWidth.AsPixels() + dMargin;
        var dHalf = dMargin / 2.0;

        var background = "Orange";
        var hash = "White";
        await ctx.SetFillStyleAsync(background);
        await ctx.FillRectAsync(dMargin, dHalf, dWidth, dHalf);

        await ctx.SetLineWidthAsync(1);
        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Bottom);

        if (!major){
            await ctx.SetFontAsync("8 px Segoe UI");
        } else {
            await ctx.SetFontAsync("Bold 22 px Segoe UI");
        }

        int i = -1 * (int)(dir * dZero / dStep);
        var left = dMargin;
        var right = dWidth;

        await ctx.SetStrokeStyleAsync(hash);
        var x = left; //left;
        while (x <= right)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dHalf);
            await ctx.LineToAsync(x, dMargin);
            await ctx.StrokeAsync();          
            x += dir * dStep;
        }

        await ctx.SetFillStyleAsync("Black");
        x = left; //left;
        while (x <= right)
        {
            var cnt = inc * i;
            await ctx.FillTextAsync($"{cnt:F1}", x, dMargin - 5);

            x += dir * dStep;
            i++;
        }

        await ctx.RestoreAsync();
    }
}


public class FoVerticalRuler2D
{
    public bool IsVisible { get; set; } = false;
    public FoScale2D Scale { get; set; }
    public FoPage2D Page { get; set; }


    public FoVerticalRuler2D(FoScale2D scale2D, FoPage2D page2D)
    {
        Scale = scale2D;
        Page = page2D;
    }

    public async Task DrawRuler(Canvas2DContext ctx, Length step, bool major)
    {
        if ( !IsVisible ) return;

        await ctx.SaveAsync();

        var dStep = step.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dir = Page.ScaleAxisY;
        var inc = Scale.World.Value();
        var dZero = Page.ZeroPointY.AsPixels() + dMargin;
        var dHeight = Page.PageHeight.AsPixels() + dMargin;
        var dHalf = dMargin / 2.0;

        var background = "Orange";
        var hash = "White";
        await ctx.SetFillStyleAsync(background);
        await ctx.FillRectAsync(dHalf, dMargin, dHalf, dHeight);

        await ctx.SetLineWidthAsync(1);
        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Bottom);

        if (!major)
        {
            await ctx.SetFontAsync("8 px Segoe UI");
        }
        else
        {
            await ctx.SetFontAsync("Bold 22 px Segoe UI");
        }

        int i = -(int)(dir* dZero / dStep);
        var top = dMargin;
        var bottom = dHeight;

        await ctx.SetStrokeStyleAsync(hash);
        var y = bottom; //bottom;
        while (y >= top)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dHalf, y);
            await ctx.LineToAsync(dMargin, y);
            await ctx.StrokeAsync();
            y += dir * dStep;
        }

        await ctx.SetFillStyleAsync("Black");
        y = bottom; //bottom;
        while (y >= top)
        {
            var cnt = inc * i;
            await ctx.FillTextAsync($"{cnt:F1}", dHalf+15, y);
            y += dir * dStep;
            i++;
        }

        await ctx.RestoreAsync();
    }
}


