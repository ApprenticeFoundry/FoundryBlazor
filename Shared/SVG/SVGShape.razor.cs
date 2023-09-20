using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace FoundryBlazor.Shared.SVG;

public class SVGShapeBase : SVGBase<FoSVGShape2D>
{
    [Inject] protected ISelectionService? SelectionService { get; set; }

    [Parameter] public FoSVGShape2D Shape { get; set; } = new();
    protected int PaddingX { get; set; } = 16;
    protected int PaddingY { get; set; } = 16;

    private FoSVGShape2D GetTextShape()
    {
        return Shape as FoSVGShape2D;
    }

    protected string GetText()
    {
        return GetTextShape()?.Text ?? "";
    }

    protected string GetTextColor()
    {
        return GetTextShape()?.TextColor ?? "white";
    }

    protected List<string> GetDetails()
    {
        return GetTextShape()?.Details ?? new List<string>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Shape);
    }




    protected string GetTextareaStyle()
    {
        var style = new StringBuilder();
        style.Append("resize: none").Append(";")
            .Append("background-color:").Append(GetColor()).Append(";")
            .Append("color:").Append(GetTextColor()).Append(";")
            .Append("width:").Append(GetWidth() - 2 * PaddingX).Append("px").Append(";")
            .Append("height:").Append(GetHeight() - 2 * PaddingY).Append("px").Append(";");
        return style.ToString();
    }

    protected void OnClickText()
    {
        $"OnClickText".WriteInfo();
        SelectionService?.ClearAll();
        SelectionService?.AddItem(Shape);
    }

    protected void OnClickButton()
    {
        $"OnClickButton".WriteInfo();
    }

}
