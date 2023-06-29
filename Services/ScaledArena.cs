using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public interface IScaledArena
{
    Rectangle Rect();

    void SetStageDefaults(FoStage3D page);
    double ConvertToUnits(double meters);
    double ToUnits(double meters);
    double ToMeters(int value);
    double ConvertToMeters(double pixels);
    double GetUnitsPerMeter();

    string CanvasWH();
    void SetCanvasPixelSize(int width, int height);
    Size CanvasPixelSize();


    Point MetersToPixelInset(double width, double height);

}

public class ScaledArena : IScaledArena
{
    public int TrueCanvasWidth = 0;
    public int TrueCanvasHeight = 0;

    public double UnitsPerMeter { get; set; } = 1; // 70; pixels per in or SRS machine
    public double StageMargin { get; set; } = .50;  //Meters
    public double StageWidth { get; set; } = 30.0;  //Meters
    public double StageHeight { get; set; } = 30.0;  //Meters
    public double PageDepth { get; set; } = 30.0;  //Meters
    public ScaledArena()
    {
    }

    public Rectangle Rect()
    {
        //make sure any shape on the canvas is in the hittest
        var result = new Rectangle(0, 0, TrueCanvasWidth, TrueCanvasHeight);
        return result;
    }



    public Point MetersToPixelInset(double width, double height)
    {
        var w = (int)ConvertToUnits(width + StageMargin);
        var h = (int)ConvertToUnits(height + StageMargin);
        return new Point(w, h);
    }

    public void SetCanvasPixelSize(int width, int height)
    {
        TrueCanvasWidth = width;
        TrueCanvasHeight = height;
    }
    public Size CanvasPixelSize()
    {
        return new Size(TrueCanvasWidth, TrueCanvasHeight);
    }

    public void SetPageSizeMeters(double width, double height, double depth)
    {
        StageWidth = width;
        StageHeight = height;
        PageDepth = depth;
    }



    public void SetStageDefaults(FoStage3D stage)
    {
        stage.StageMargin = this.StageMargin;
        stage.StageWidth = this.StageWidth;
        stage.StageHeight = this.StageHeight;
        stage.Smash(false);
    }
    public string CanvasWH()
    {
        return $"Canvas W:{TrueCanvasWidth} H:{TrueCanvasHeight} DPI:{UnitsPerMeter}";
    }
    public double GetUnitsPerMeter()
    {
        return UnitsPerMeter;
    }
    public double ToUnits(double meters)
    {
        return UnitsPerMeter * meters;
    }
    public double ToMeters(int value)
    {
        return value / UnitsPerMeter;
    }
    public double ConvertToUnits(double Meters)
    {
        return UnitsPerMeter * Meters;
    }

    public double ConvertToMeters(double units)
    {
        return units / UnitsPerMeter;
    }

}
