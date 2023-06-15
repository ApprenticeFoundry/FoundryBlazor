
using UnitsNet;
namespace FoundryBlazor.Shape;

public class FoScaledShape2D : FoShape2D, IShape2D
{
    public Length Units { get; set; }

    public FoScaledShape2D() : base()
    {
        ShapeDraw = DrawRect;
    }
    public FoScaledShape2D(string name, string color) : base(name, color)
    {
        ShapeDraw = DrawRect;
    }

    public virtual void SetUnits(Length data)
    {
        Units = data;
    }
}

