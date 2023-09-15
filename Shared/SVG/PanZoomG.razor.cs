using Microsoft.AspNetCore.Components;
using FoundryBlazor.PubSub;
using BlazorComponentBus;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Shared.SVG;

public class PanZoomGBase : ComponentBase, IDisposable
{
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] public IPanZoomService? PanZoom { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        }
        await base.OnAfterRenderAsync(firstRender);
    }




    protected string GetTransform()
    {
        var mtx = PanZoom?.GetMatrix() ?? new Matrix2D();
        if (mtx.IsSVGRefreshed())
        {
            //$"Shape2DBase.GetMatrix {Shape.GetGlyphId()} cached={matrix}  ".WriteSuccess(2);
            return mtx.SVGMatrix();
        }

        //$"Shape2DBase.GetMatrix {Shape.GetGlyphId()} result={matrix}  ".WriteInfo(2);
    }


}
