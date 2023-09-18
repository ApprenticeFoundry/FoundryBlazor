using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;

namespace FoundryBlazor.Shared.SVG;

public class PageGridBase : SVGBase<FoPage2D>
{
    [Parameter] public FoPage2D Page { get; set; } = new("page1", "Gray");
    //[Parameter] public RenderFragment? ChildContent { get; set; }
    protected string StrokeColor { get; set; } = "#d8d8d8"; // light gray
    protected string StrokeDashArray { get; set; } = "15 0"; // dash-length, pixels between dash
    protected int MajorAxisStrokeWidth { get; set; } = 3;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Page);
    }


    public List<SVGLine> GetMajorHorizontalGridSVG()
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

        var x = dMargin + dStep; //left;
        while (x <= dWidth - dStep)
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

        var x = dHeight + dStep; //left;
        while (x >= dMargin - dStep)
        {
            var rect = new SVGLine(dMargin, x, dWidth, x);
            list.Add(rect);
            x -= dStep;
        }
        return list;
    }


}
