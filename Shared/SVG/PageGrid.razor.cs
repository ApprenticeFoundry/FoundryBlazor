using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;

namespace FoundryBlazor.Shared.SVG;

public class PageGridBase : ComponentBase
{
    [Parameter] public FoPage2D Page { get; set; } = new("page1", "Gray");
    //[Parameter] public RenderFragment? ChildContent { get; set; }

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

    public List<SVGLine> GetMajortHorizontalGridSVG()
    {
        return GetHorizontalGridSVG(Page.GridMajorH);
    }
    
    public List<SVGLine> GetMinortHorizontalGridSVG()
    {
        return GetHorizontalGridSVG(Page.GridMinorH);
    }

    private List<SVGLine> GetHorizontalGridSVG(Length step)
    {
        var dStep = step.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dWidth = Page.PageWidth.AsPixels() + dMargin;
        var dHeight = Page.PageHeight.AsPixels() + dMargin;

        var list = new List<SVGLine>();

        var x = dMargin; //left;
        while (x <= dWidth)
        {
            var rect = new SVGLine(x, dMargin, x, dHeight);
            list.Add(rect);
            x += dStep;
        }
        return list;
    }


    public List<SVGLine> GetMajorVerticalGridSVG()
    {
        return GetVerticalGridSVG(Page.GridMajorV);
    }

    public List<SVGLine> GetMinorVerticalGridSVG()
    {
        return GetVerticalGridSVG(Page.GridMinorV);
    }

    private List<SVGLine> GetVerticalGridSVG(Length step)
    {
        var dStep = step.AsPixels();
        var dMargin = Page.PageMargin.AsPixels();
        var dWidth = Page.PageWidth.AsPixels() + dMargin;
        var dHeight = Page.PageHeight.AsPixels() + dMargin;

        var list = new List<SVGLine>();

        var x = dHeight; //left;
        while (x >= dMargin)
        {
            var rect = new SVGLine(dMargin, x, dWidth, x);
            list.Add(rect);
            x -= dStep;
        }
        return list;
    }


}
