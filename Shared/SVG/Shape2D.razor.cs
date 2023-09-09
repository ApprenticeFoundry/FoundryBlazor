using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components.Web;

namespace FoundryBlazor.Shared.SVG;

public class Shape2DBase : ComponentBase
{
    [Parameter] public FoShape2D Shape { get; set; } = new();
    private string matrix { get; set; } = "";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Shape.OnMatrixSmash = (obj) =>
        {
            $"Shape2DBase.OnMatrixSmash {Shape.GetGlyphId()}".WriteInfo(2);
            matrix = "";
            StateHasChanged();
        };
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
