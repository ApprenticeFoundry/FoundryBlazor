using FoundryBlazor.Shape;

namespace FoundryBlazor.PubSub;


public class ShapeUIEvent
{
    public FoGlyph2D Shape { get; set; }

    public ShapeUIEvent(FoGlyph2D shape)
    {
        Shape = shape;
    }
}

public class ShapeHoverUIEvent : ShapeUIEvent
{
    public ShapeHoverUIEvent(FoGlyph2D shape):base(shape)
    {
    }
}

public class ShapeSelectedUIEvent : ShapeUIEvent
{
    public ShapeSelectedUIEvent(FoGlyph2D shape):base(shape)
    {
    }
}


