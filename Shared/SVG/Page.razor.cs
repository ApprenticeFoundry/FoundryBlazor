using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shared.SVG;

public class PageBase : ComponentBase
{
    [Parameter] public FoPage2D Page { get; set; } = new("page1", "White");
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected string GetMatrix()
    {
        var mtx = Page.GetMatrix();
        var result = mtx.SVGMatrix();
        return result;
    }

    public List<FoShape1D> GetShapes1D()
    {
        var shapes = Page.AllShapes1D();
        $"GetShapes1D {shapes.Count}".WriteInfo();
        return shapes;
    }

    public List<FoShape2D> GetShapes2D()
    {
        var shapes = Page.AllShapes2D();
        $"GetShapes2D {shapes.Count}".WriteInfo();
        return shapes;
    }

    protected int GetPageWidth()
    {
        var margin = Page.PageMargin.AsPixels();
        var width = Page.PageWidth.AsPixels() + 2.0 * margin;

        return (int)width;
    }
    protected int GetWidth()
    {
        var width = Page.PageWidth.AsPixels();
        return (int)width;
    }
    protected int GetPageHeight()
    {
        var margin = Page.PageMargin.AsPixels();
        var height = Page.PageHeight.AsPixels() + 2.0 * margin;
        return (int)height;
    }
    protected int GetHeight()
    {
        var height = Page.PageHeight.AsPixels();
        return (int)height;
    }
    protected int GetMargin()
    {
        var margin = Page.PageMargin.AsPixels();
        return margin;
    }

    protected string GetColor()
    {
        return Page.Color;
    }

}
