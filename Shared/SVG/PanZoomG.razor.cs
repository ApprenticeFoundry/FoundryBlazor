using Microsoft.AspNetCore.Components;
using FoundryBlazor.PubSub;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared.SVG;

public class PanZoomGBase : ComponentBase, IDisposable
{
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] public IPanZoomService? PanZoom { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Transform { get; set; } = "matrix(1, 0, 0, 1, 30, 30)";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        if (e.note == "PanZoom")
        {
            SetTransform();
            InvokeAsync(StateHasChanged);
        }
    }

    private string SetTransform()
    {
        var mtx = PanZoom?.GetMatrix() ?? new Matrix2D();
        Transform = $"matrix({mtx.a}, {mtx.b}, {mtx.c}, {mtx.d}, {mtx.tx}, {mtx.ty})";
        return Transform;
    }

    public void Dispose()
    {
        PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);

        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue 
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    protected void MouseDown(MouseEventArgs evt)
    {
        Console.WriteLine($"In MouseDown PanZoomG {evt.ScreenX}  {evt.ScreenY}");
    }


}
