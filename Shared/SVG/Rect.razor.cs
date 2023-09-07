using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared.SVG;

public class RectBase : ComponentBase
{
    [Parameter] public FoShape2D Shape { get; set; } = new();
    private string Matrix { get; set; } = "0,0";

    protected override void OnInitialized()
    {
        GetMatrix();
        base.OnInitialized();
    }

    protected string GetMatrix()
    {
        var mtx = Shape.GetMatrix();
        Matrix = $"matrix({mtx.a}, {mtx.b}, {mtx.c}, {mtx.d}, {mtx.tx}, {mtx.ty})";
        return Matrix;
    }

    protected void MouseDown(MouseEventArgs evt)
    {
        Console.WriteLine($"In MouseDown Rect {evt.ScreenX}  {evt.ScreenY}");
    }


}
