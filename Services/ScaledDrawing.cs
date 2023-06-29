using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using IoBTMessage.Units;

namespace FoundryBlazor.Shape;

public interface IScaledCanvas
{
    Rectangle Rect();
    Rectangle UserWindow();
    Rectangle SetUserWindow(Size size);
    Rectangle SetUserWindow(Point loc);
    void SetPageDefaults(FoPage2D page);

    Task ClearCanvas(Canvas2DContext ctx);
    string CanvasWH();
    void SetCanvasPixelSize(int width, int height);
    Size TrueCanvasSize();

    void SetPageSizeInches(double width, double height);
    void SetPageLandscape();
    void SetPagePortrait();


    Task DrawHorizontalGrid(Canvas2DContext ctx, Length minor, Length major);
    Task DrawVerticalGrid(Canvas2DContext ctx, Length minor, Length major);

    ScaledCanvas CreateScaledPage();
}

public class ScaledDrawing : IScaledCanvas
{
    public int TrueCanvasWidth = 0;
    public int TrueCanvasHeight = 0;

    private Rectangle userWindow { get; set; } = new Rectangle(0, 0, 1500, 400);


    public Length PageMargin { get; set; } = new Length(.50, "in"); //inches
    public Length PageWidth { get; set; } = new Length(10.0, "in");  //inches
    public Length PageHeight { get; set; } = new Length(6.0, "in");  //inches

    public ScaledDrawing()
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
        return userWindow;
    }

    public Rectangle SetUserWindow(Size size)
    {
        userWindow = new Rectangle(userWindow.Location, size);
        return userWindow;
    }
    public Rectangle SetUserWindow(Point loc)
    {
        userWindow = new Rectangle(-loc.X, -loc.Y, userWindow.Width, userWindow.Height);
        return userWindow;
    }

    public static int ToPixels(Length inches)
    {
        return (int)inches.AsPixels();
    }




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
        if (PageWidth.Value() < PageHeight.Value())
        {
            (PageWidth, PageHeight) = (PageHeight, PageWidth);
        }
    }
    public void SetPagePortrait()
    {
        if (PageWidth.Value() > PageHeight.Value())
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
        return $"Canvas W:{TrueCanvasWidth} H:{TrueCanvasHeight} ";
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

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


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

        var dMinor = minor.AsPixels();
        var dMajor = major.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


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
