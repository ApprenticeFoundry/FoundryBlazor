using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;

namespace FoundryBlazor.Shared.SVG;

public class Shape1DBase : SVGBase<FoShape1D>
{
    [Parameter] public FoShape1D Shape { get; set; } = new();


    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Shape);
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


        //$"Shape1DBase.GetPoints result={result}".WriteInfo(2);
        return result;
    }
}
