using FoundryBlazor.Shape;
using FoundryBlazor.PubSub;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace FoundryBlazor.Shared.SVG;

public class Text2DBase : SVGBase<FoText2D>
{
    [Inject] protected ISelectionService? SelectionService { get; set; }

    [Parameter] public FoText2D Shape { get; set; } = new();
    protected int PaddingX { get; set; } = 16;
    protected int PaddingY { get; set; } = 16;
    private FoText2D GetTextShape()
    {
        return Shape as FoText2D;
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
