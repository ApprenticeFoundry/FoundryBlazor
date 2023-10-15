using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace FoundryBlazor.Shared.SVG;

public class ImageBase : SVGBase<FoImage2D>
{
    public static (int width, int height, string url) RandomSpec()
    {
        var gen = new MockDataGenerator();
        var width = 50 * gen.GenerateInt(1, 6);
        var height = 50 * gen.GenerateInt(1, 6);

        return (
            width,
            height,
            $"https://picsum.photos/{width}/{height}"
        );
    }

    [Inject] protected ISelectionService? SelectionService { get; set; }

    [Parameter] public FoImage2D Shape { get; set; } = new();
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
