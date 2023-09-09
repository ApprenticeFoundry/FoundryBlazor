using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;

namespace FoundryBlazor.Shared.SVG;

public class Shape1DBase : ComponentBase
{
    [Parameter] public FoShape1D Shape { get; set; } = new();

    private string matrix { get; set; } = "";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Shape.AfterMatrixSmash((obj) =>
        {
            $"Shape1DBase.AfterMatrixSmash {Shape.GetGlyphId()}".WriteInfo(2);
            matrix = "";
            StateHasChanged();
        });
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
        //$"Shape1DBase.GetMatrix result={matrix}".WriteInfo(2);
        return matrix;
    }
    protected string GetPoints()
    {
        //Shape.GetMatrix();
        var width = Shape.FinishX - Shape.StartX;
        var height = Shape.FinishY - Shape.StartY;

        var result = $"0,0 {width},{height}";

        if (Shape.Layout == LineLayoutStyle.HorizontalFirst)
            result = $"{0},{0} {width},{0} {width},{height}";
        else if (Shape.Layout == LineLayoutStyle.VerticalFirst)
            result = $"{0},{0} {0},{height} {width},{height}";
        else if (Shape.Layout == LineLayoutStyle.Straight)
            result = $"0,0 {width},{height}";


        $"Shape1DBase.GetPoints result={result}".WriteInfo(2);
        return result;
    }
}
