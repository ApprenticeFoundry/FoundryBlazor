using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace FoundryBlazor.Shared.SVG;

public class VideoBase : SVGBase<FoVideo2D>
{
    [Inject] protected ISelectionService? SelectionService { get; set; }

    [Parameter] public FoVideo2D Shape { get; set; } = new();
    protected int PaddingX { get; set; } = 12;
    protected int PaddingY { get; set; } = 12;

    protected string GetURL()
    {
        return Shape.ImageUrl;
    }

    protected string GetStyle()
    {
        var style = new StringBuilder("width:").Append(GetWidth() - 2 * PaddingX).Append("px;").Append("height:").Append(GetHeight() - 2 * PaddingY).Append("px").ToString();
        return style;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Shape);
    }

}
