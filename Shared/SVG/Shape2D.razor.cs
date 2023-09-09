using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components.Web;
using FoundryBlazor.PubSub;
using BlazorComponentBus;

namespace FoundryBlazor.Shared.SVG;

public class Shape2DBase : ComponentBase
{
    [Inject] private ComponentBus? PubSub { get; set; }

    [Parameter] public FoShape2D Shape { get; set; } = new();
    private string matrix { get; set; } = "";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Shape.AfterMatrixSmash((obj) =>
        {
            $"Shape2DBase.AfterMatrixSmash {Shape.GetGlyphId()}".WriteInfo(2);
            matrix = "";
            StateHasChanged();
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            PubSub!.SubscribeTo<ShapeHoverUIEvent>(OnRefreshUIEvent);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnRefreshUIEvent(ShapeHoverUIEvent e)
    {
        if (e.Shape == Shape)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    protected string GetMatrix()
    {
        if ( !string.IsNullOrEmpty(matrix) )
        {
            //$"Shape2DBase.GetMatrix {Shape.GetGlyphId()} cached={matrix}  ".WriteSuccess(2);
            return matrix;
        }

        var mtx = Shape.GetMatrix();
        matrix = mtx.SVGMatrix();
        //$"Shape2DBase.GetMatrix {Shape.GetGlyphId()} result={matrix}  ".WriteInfo(2);
        return matrix;
    }

    protected bool IsHovering()
    {
        var result = Shape.HoverDraw != null;
        return result;
    }

    protected bool IsSelected()
    {
        var result = Shape.IsSelected;
        return result;
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
