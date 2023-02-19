namespace FoundryBlazor.Shape;

public class FoDragTarget2D : FoShape2D
{

    public FoShape1D? Connector;
    public FoDragTarget2D() : base()
    {
        ShapeDraw = DrawCircle;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public FoDragTarget2D(int width, int height, string color) : base("", width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawCircle;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }

    public FoDragTarget2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawCircle;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }



}
