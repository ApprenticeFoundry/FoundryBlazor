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
    private string transform { get; set; } = "";

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
            transform = "";
            InvokeAsync(StateHasChanged);
        }
    }



    protected string GetTransform()
    {
        if ( !string.IsNullOrEmpty(transform) )
            return transform;
            
        var mtx = PanZoom?.GetMatrix() ?? new Matrix2D();
        transform = mtx.SVGMatrix();
        return transform;
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
