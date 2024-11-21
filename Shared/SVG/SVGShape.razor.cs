using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace FoundryBlazor.Shared.SVG;

public class SVGShapeBase : SVGBase<FoSymbol2D>
{
    [Inject] protected ISelectionService? SelectionService { get; set; }

    [Parameter] public FoSymbol2D Shape { get; set; } = new();
 

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Shape);
    }




 

}
