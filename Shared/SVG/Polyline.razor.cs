using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;

namespace FoundryBlazor.Shared.SVG;

public class PolylineBase : ComponentBase
{
    [Parameter] public FoConnector1D Shape { get; set; } = new();
    protected string Points { get; set; } = "0,0";
    private string Matrix { get; set; } = "0,0";

    protected override void OnInitialized()
    {
        GetPoints();
        GetMatrix();
        base.OnInitialized();
    }

    protected string GetMatrix()
    {
        var mtx = Shape.GetMatrix();
        Matrix = $"matrix({mtx.a}, {mtx.b}, {mtx.c}, {mtx.d}, {mtx.tx}, {mtx.ty})";
        return Matrix;
    }
    private string GetPoints()
    {
        Shape.GetMatrix();
        var width = Shape.FinishX - Shape.StartX;
        var height = Shape.FinishY - Shape.StartY;

        Points = $"0,0 {width},{height}";

        if (Shape.Layout == LineLayoutStyle.HorizontalFirst)
            Points = $"{0},{0} {width},{0} {width},{height}";
        else if (Shape.Layout == LineLayoutStyle.VerticalFirst)
            Points = $"{0},{0} {0},{height} {width},{height}";
        else if (Shape.Layout == LineLayoutStyle.Straight)
            Points = $"0,0 {width},{height}";
        return Points;
    }
}
