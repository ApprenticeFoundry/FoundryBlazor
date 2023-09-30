using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using BlazorComponentBus;
 
using FoundryRulesAndUnits.Extensions;
using System.Drawing;

namespace FoundryBlazor.Shared.SVG;

public class PageBase : SVGBase<FoPage2D>
{

    [Inject] public IPanZoomService? PanZoom { get; set; }

    [Parameter] public FoPage2D Page { get; set; } = new("page1", "Grey");
    [Parameter] public RenderFragment? ChildContent { get; set; }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Page);
    }


    public List<FoShape1D> GetShapes1D()
    {
        var shapes = Page.AllShapes1D();
        //$"GetShapes1D {shapes.Count}".WriteInfo();
        return shapes;
    }

    public List<FoShape2D> GetShapes2D()
    {
        var shapes = Page.AllShapes2D();
        //$"GetShapes2D {shapes.Count}".WriteInfo();
        return shapes;
    }

    public List<FoText2D> GetText2D()
    {
        var shapes = Page.AllShapes2D().Where(obj => obj is FoText2D).Cast<FoText2D>().ToList();
        //$"GetShapes2D {shapes.Count}".WriteInfo();
        return shapes;
    }

    protected int GetPageWidth()
    {
        var margin = Page.PageMargin.AsPixels();
        var width = Page.PageWidth.AsPixels() + 2.0 * margin;

        return (int)width;
    }

    protected int GetPageHeight()
    {
        var margin = Page.PageMargin.AsPixels();
        var height = Page.PageHeight.AsPixels() + 2.0 * margin;
        return (int)height;
    }

    protected override int GetWidth()
    {
        var width = Page.PageWidth.AsPixels();
        return width;
    }

    protected override int GetHeight()
    {
        var height = Page.PageHeight.AsPixels();
        return height;
    }

    protected int GetMargin()
    {
        var margin = Page.PageMargin.AsPixels();
        return margin;
    }

    public bool IsFenceSelecting()
    {
        return PanZoom?.IsFenceSelecting() ?? false;
    }

    public Rectangle GetFence()
    {
        var fence = BaseInteraction.GetDragArea();
        var rect = PanZoom?.TransformRect(fence) ?? fence;
        return rect;
    }
}
