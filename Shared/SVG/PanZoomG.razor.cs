using Microsoft.AspNetCore.Components;
using FoundryBlazor.PubSub;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared.SVG;

public class PanZoomGBase : ComponentBase, IDisposable
{
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] public IPanZoomService? PanZoom { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    private string matrix { get; set; } = "";

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
            matrix = "";
            InvokeAsync(StateHasChanged);
        }
    }



    protected string GetTransform()
    {
        if ( !string.IsNullOrEmpty(matrix) )
            return matrix;
            
        var mtx = PanZoom?.GetMatrix() ?? new Matrix2D();
        matrix = mtx.SVGMatrix();
        //$"PanZoomGBase.GetTransform {matrix}".WriteInfo(2);
        return matrix;
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


}
