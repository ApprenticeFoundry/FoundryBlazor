using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components.Web;
using FoundryBlazor.PubSub;
using BlazorComponentBus;

namespace FoundryBlazor.Shared.SVG;

public class SVGBase<T> : ComponentBase where T : FoGlyph2D
{
    [Inject] protected ComponentBus? PubSub { get; set; }
    [Parameter] public T? Source { get; set; }


    protected T InitSource(T glyph)
    {
        Source = glyph;
        Source.AfterMatrixSmash((obj) =>
        {
            InvokeAsync(StateHasChanged);
        });
        return Source;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            PubSub!.SubscribeTo<ShapeHoverUIEvent>(OnShapeHover);
            PubSub!.SubscribeTo<ShapeSelectedUIEvent>(OnShapeSelected);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnShapeHover(ShapeHoverUIEvent e)
    {
        if (e.Shape == Source)
        {
            InvokeAsync(StateHasChanged);
        }
    }
    private void OnShapeSelected(ShapeSelectedUIEvent e)
    {
        if (e.Shape == Source)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    protected string GetSVGMatrix()
    {
        var mtx = Source?.GetMatrix() ?? new Matrix2D();
        if (mtx.IsSVGRefreshed())
        {
            //$"Shape2DBase.GetMatrix {Shape.GetGlyphId()} cached={matrix}  ".WriteSuccess(2);
            return mtx.SVGMatrix();
        }


        //$"Shape2DBase.GetMatrix {Shape.GetGlyphId()} result={matrix}  ".WriteInfo(2);
        return mtx.SVGMatrix();
    }

    protected bool IsHovering()
    {
        var result = Source?.HoverDraw != null;
        return result;
    }

    protected bool IsSelected()
    {
        var result = Source?.IsSelected ?? false;
        return result;
    }

    protected virtual int GetWidth()
    {
        var width = Source?.Width ?? 0;
        return (int)width;
    }

    protected virtual int GetHeight()
    {
        var height = Source?.Height ?? 0;
        return (int)height;
    }

    protected string GetColor()
    {
        return Source?.Color ?? "Green";
    }
    protected string GetDashArray()
    {
        if (IsSelected())
            return "5 5";
        else
            return "";
    }
    protected string GetStrokeColor()
    {
        if (IsSelected())
            return "white";
        else if (IsHovering())
            return "blue";
        else
            return "black";
    }

}
