using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public interface IScaledArena
{
    Rectangle Rect();

    void SetPageDefaults(FoStage3D page);
    double ConvertToPixels(double inches);
    int ToPixels(double inches);
    double ToInches(int value);
    double ConvertToInches(double pixels);
    double GetPixelsPerInch();

    string CanvasWH();
    void SetCanvasSize(int width, int height);
    Size CanvasSize();


    Point InchesToPixelInset(double width, double height);

}

public class ScaledArena : IScaledArena
{
    public int TrueCanvasWidth = 0;
    public int TrueCanvasHeight = 0;

    public double PixelsPerInch { get; set; } = 50; // 70; pixels per in or SRS machine
    public double PageMargin { get; set; } = .50;  //inches
    public double PageWidth { get; set; } = 10.0;  //inches
    public double PageHeight { get; set; } = 6.0;  //inches
    public double PageDepth { get; set; } = 6.0;  //inches
    public ScaledArena()
    {
    }

    public Rectangle Rect()
    {
        //make sure any shape on the canvas is in the hittest
        var result = new Rectangle(0, 0, TrueCanvasWidth, TrueCanvasHeight);
        return result;
    }



    public Point InchesToPixelInset(double width, double height)
    {
        var w = (int)ConvertToPixels(width + PageMargin);
        var h = (int)ConvertToPixels(height + PageMargin);
        return new Point(w, h);
    }

    public void SetCanvasSize(int width, int height)
    {
        TrueCanvasWidth = width;
        TrueCanvasHeight = height;
    }
    public Size CanvasSize()
    {
        return new Size(TrueCanvasWidth,TrueCanvasHeight);
    }

    public void SetPageSizeInches(double width, double height, double depth)
    {
        PageWidth = width;
        PageHeight = height;
        PageDepth = depth;
    }



    public void SetPageDefaults(FoStage3D page)
    {
        page.PageMargin = this.PageMargin;
        page.PageWidth = this.PageWidth;
        page.PageHeight = this.PageHeight;
        page.Smash(false);
    }
    public string CanvasWH()
    {
        return $"Canvas W:{TrueCanvasWidth} H:{TrueCanvasHeight} DPI:{PixelsPerInch}";
    }   
    public double GetPixelsPerInch()
    {
        return PixelsPerInch;
    }
    public int ToPixels(double inches)
    {
        return (int)(PixelsPerInch * inches);
    }
    public double ToInches(int value)
    {
        return (double)(value / PixelsPerInch);
    }
    public double ConvertToPixels(double inches)
    {
        return PixelsPerInch * inches;
    }

    public double ConvertToInches(double pixels)
    {
        return pixels / PixelsPerInch;
    }

}
