using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared.SVG;

public class RectBase : ComponentBase
{
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] public IPanZoomService? PanZoom { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Transform { get; set; } = "matrix(1, 0, 0, 1, 1, 1)";

    protected void MouseDown(MouseEventArgs evt)
    {
        Console.WriteLine($"In MouseDown Rect {evt.ScreenX}  {evt.ScreenY}");
    }


}
