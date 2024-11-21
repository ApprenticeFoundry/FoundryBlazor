using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components.Web;
using FoundryBlazor.PubSub;
using BlazorComponentBus;

namespace FoundryBlazor.Shared.SVG;

public class Shape2DBase : SVGBase<FoShape2D>
{
    [Parameter] public FoShape2D Shape { get; set; } = new();
    protected string StrokeColor { get; set; } = "black";


    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Shape);
    }





}
