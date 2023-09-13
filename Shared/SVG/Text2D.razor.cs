using FoundryBlazor.Shape;
using FoundryBlazor.PubSub;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;

namespace FoundryBlazor.Shared.SVG;

public class Text2DBase : Shape2DBase
{
    [Inject] protected ISelectionService? SelectionService { get; set; }
    protected int PaddingX { get; set; } = 16;
    protected int PaddingY { get; set; } = 16;
    private FoText2D? GetTextShape()
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
        Shape.AfterMatrixSmash((obj) =>
        {
            //$"Shape2DBase.AfterMatrixSmash {Shape.GetGlyphId()}".WriteInfo(2);
            InvokeAsync(StateHasChanged);
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            PubSub!.SubscribeTo<ShapeHoverUIEvent>(OnShapeHover);
            PubSub!.SubscribeTo<ShapeSelectedUIEvent>(OnShapeSelected);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnShapeHover(ShapeHoverUIEvent e)
    {
        if (e.Shape == Shape)
        {
            InvokeAsync(StateHasChanged);
        }
    }
    private void OnShapeSelected(ShapeSelectedUIEvent e)
    {
        if (e.Shape == Shape)
        {
            InvokeAsync(StateHasChanged);
        }
    }

    protected string GetTextareaStyle()
    {
        // return $"background-color: {GetColor()};color: {GetTextColor()};width: {GetWidth() - 2 * PaddingX}px;height: {GetHeight() - 2 * PaddingY}px";
        return $"background-color: orange;color: {GetTextColor()};width: {GetWidth() - 2 * PaddingX}px;height: {GetHeight() - 2 * PaddingY}px";
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
