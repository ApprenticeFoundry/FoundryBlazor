using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared.SVG;

public class Shape2DBase : ComponentBase
{
    [Parameter] public FoShape2D Shape { get; set; } = new();
    private string Matrix { get; set; } = "0,0";

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected string GetMatrix()
    {
        var mtx = Shape.GetMatrix();
        Matrix = $"matrix({mtx.a}, {mtx.b}, {mtx.c}, {mtx.d}, {mtx.tx}, {mtx.ty})";
        return Matrix;
    }

    protected int GetWidth()
    {
        var width = Shape.Width;
        return (int)width;
    }

    protected int GetHeight()
    {
        var height = Shape.Height;
        return (int)height;
    }

    protected string GetColor()
    {
        return Shape.Color;
    }

}
