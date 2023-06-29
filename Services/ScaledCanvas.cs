using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using IoBTMessage.Units;

namespace FoundryBlazor.Shape;


public class ScaledCanvas : IScaledCanvas
{
    public int TrueCanvasWidth = 0;
    public int TrueCanvasHeight = 0;

    private Rectangle UserWindowRect { get; set; } = new Rectangle(0, 0, 1500, 400);

    public Length PageMargin { get; set; } = new Length(.50, "in");  //inches
    public Length PageWidth { get; set; } = new Length(10.0, "in");  //inches
    public Length PageHeight { get; set; } = new Length(6.00, "in");  //inches

    public ScaledCanvas()
    {
    }

    public ScaledCanvas CreateScaledPage()
    {
        return new ScaledCanvas();
    }

    public Rectangle Rect()
    {
        //make sure any shape on the canvas is in the hittest
        var result = new Rectangle(0, 0, TrueCanvasWidth, TrueCanvasHeight);
        return result;
    }

    public Rectangle UserWindow()
    {
        return UserWindowRect;
    }

    public Rectangle SetUserWindow(Size size)
    {
        UserWindowRect = new Rectangle(UserWindowRect.Location, size);
        return UserWindowRect;
    }
    public Rectangle SetUserWindow(Point loc)
    {
        UserWindowRect = new Rectangle(-loc.X, -loc.Y, UserWindowRect.Width, UserWindowRect.Height);
        return UserWindowRect;
    }
    // public Point InchesToPixelInset(Length width, Length height)
    // {
    //     var w = (int)ToPixels(width + PageMargin);
    //     var h = (int)ToPixels(height + PageMargin);
    //     return new Point(w, h);
    // }



    public void SetCanvasPixelSize(int width, int height)
    {
        TrueCanvasWidth = width;
        TrueCanvasHeight = height;
    }
    public Size TrueCanvasSize()
    {
        return new Size(TrueCanvasWidth, TrueCanvasHeight);
    }

    public void SetPageSizeInches(double width, double height)
    {
        PageWidth.Assign(width, "in");
        PageHeight.Assign(height, "in");
    }

    public void SetPageLandscape()
    {
        if (PageWidth < PageHeight)
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
    }
    public void SetPagePortrait()
    {
        if (PageWidth > PageHeight)
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
    }

    public void SetPageDefaults(FoPage2D page)
    {
        page.PageMargin = this.PageMargin;
        page.PageWidth = this.PageWidth;
        page.PageHeight = this.PageHeight;
        page.Smash(false);
    }
    public string CanvasWH()
    {
        return $"Canvas W:{TrueCanvasWidth} H:{TrueCanvasHeight}";
    }

    public static int ToPixels(Length inches)
    {
        return (int)inches.AsPixels();
    }


    public async Task ClearCanvas(Canvas2DContext ctx)
    {
        await ctx.ClearRectAsync(0, 0, TrueCanvasWidth, TrueCanvasHeight);
        await ctx.SetFillStyleAsync("#98AFC7");
        await ctx.FillRectAsync(0, 0, TrueCanvasWidth, TrueCanvasHeight);

        await ctx.SetStrokeStyleAsync("Black");
        await ctx.StrokeRectAsync(0, 0, TrueCanvasWidth, TrueCanvasHeight);
    }

    public async Task DrawHorizontalGrid(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = ToPixels(minor);
        var dMajor = ToPixels(major);
        var dMargin = ToPixels(PageMargin);
        var dWidth = ToPixels(PageWidth) + dMargin;
        var dHeight = ToPixels(PageHeight) + dMargin;


        await ctx.SetLineWidthAsync(1);
        await ctx.SetLineDashAsync(new float[] { 5, 1 });


        await ctx.SetStrokeStyleAsync("White");

        var x = dMargin; //left;
        while (x <= dWidth)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin);
            await ctx.LineToAsync(x, dHeight);
            await ctx.StrokeAsync();
            x += dMinor;
        }


        await ctx.SetLineDashAsync(Array.Empty<float>());
        await ctx.SetStrokeStyleAsync("Black");

        x = dMargin; //left;
        while (x <= dWidth)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x, dMargin);
            await ctx.LineToAsync(x, dHeight);
            await ctx.StrokeAsync();
            x += dMajor;
        }

        await ctx.RestoreAsync();
    }


    public async Task DrawVerticalGrid(Canvas2DContext ctx, Length minor, Length major)
    {
        await ctx.SaveAsync();

        var dMinor = ToPixels(minor);
        var dMajor = ToPixels(major);
        var dMargin = ToPixels(PageMargin);
        var dWidth = ToPixels(PageWidth) + dMargin;
        var dHeight = ToPixels(PageHeight) + dMargin;


        await ctx.SetLineWidthAsync(1);
        await ctx.SetLineDashAsync(new float[] { 5, 1 });


        await ctx.SetStrokeStyleAsync("White");

        var x = dMargin; //left;
        while (x <= dHeight)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin, x);
            await ctx.LineToAsync(dWidth, x);
            await ctx.StrokeAsync();
            x += dMinor;
        }


        await ctx.SetLineDashAsync(Array.Empty<float>());
        await ctx.SetStrokeStyleAsync("Black");

        x = dMargin; //left;
        while (x <= dHeight)
        {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(dMargin, x);
            await ctx.LineToAsync(dWidth, x);
            await ctx.StrokeAsync();
            x += dMajor;
        }

        await ctx.RestoreAsync();
    }


}
