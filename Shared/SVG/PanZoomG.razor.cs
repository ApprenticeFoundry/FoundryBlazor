using Microsoft.AspNetCore.Components;
using FoundryBlazor.PubSub;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shared.SVG;

public class PanZoomGBase : ComponentBase
{
    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] public IPanZoomService? PanZoom { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        PanZoom?.AfterMatrixSmash((obj) =>
        {
            // $"PanZoomGBase.AfterMatrixSmash ".WriteInfo(2);
            InvokeAsync(StateHasChanged);
        });
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
        return mtx.SVGMatrix();
    }


}
