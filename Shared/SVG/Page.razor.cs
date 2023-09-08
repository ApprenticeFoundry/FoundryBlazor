using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shared.SVG;

public class PageBase : ComponentBase
{
    [Parameter] public FoPage2D Page { get; set; } = new ("page1", "White");

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected string GetMatrix()
    {
        var mtx = Page.GetMatrix();
        var result = $"matrix({mtx.a}, {mtx.b}, {mtx.c}, {mtx.d}, {mtx.tx}, {mtx.ty})";
        return result;
    }

   public List<FoConnector1D> GetConnectors()
    {
        var shapes = Page.AllShapes1D().Where((shape) => shape is FoConnector1D).Cast<FoConnector1D>().ToList();
        $"GetConnectors {shapes.Count}".WriteInfo();
        return shapes;
    }

    public List<FoShape2D> GetShapes2D()
    {
        var shapes = Page.AllShapes2D();
        $"GetShapes2D {shapes.Count}".WriteInfo();
        return shapes;
    }

    protected int GetWidth()
    {
        var margin = Page.PageMargin.AsPixels();
        var width = Page.PageWidth.AsPixels() + 2.0 * margin;

        return (int)width;
    }
    protected int GetHeight()
    {
        var margin = Page.PageMargin.AsPixels();
        var height = Page.PageHeight.AsPixels() + 2.0 * margin;
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
