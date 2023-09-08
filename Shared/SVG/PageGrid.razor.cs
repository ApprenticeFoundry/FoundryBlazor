using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shared.SVG;

public class PageGridBase : ComponentBase
{
    [Parameter] public FoPage2D Page { get; set; } = new("page1", "Gray");
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

    public void DrawHorizontalGridSVG(CanvasSVGComponentBase ctx, Length step, bool major)
    {
        var dStep = step.AsPixels();
        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        List<KeyValuePair<string, object>> attributes;
        if (!major)
        {
            attributes = new List<KeyValuePair<string, object>>()
            {
                new("stroke", "White"),
                new("stroke-width", 1),
                new("stroke-dasharray", "5 1"),
            };
        }
        else
        {
            attributes = new List<KeyValuePair<string, object>>()
            {
                new("stroke", "Black"),
                new("stroke-width", 3),
            };
        }


        var x = dMargin; //left;
        while (x <= dWidth)
        {
            var lineAttributes = new List<KeyValuePair<string, object>>() {
                new("x1", x),
                new("y1", dMargin),
                new("x2", x),
                new("y2", dHeight)
            };
            lineAttributes.AddRange(attributes);

            //$"DrawHorizontalGridSVG {x} {dMargin} {dHeight}".WriteLine(ConsoleColor.Blue);
            void node(RenderTreeBuilder builder)
            {
                builder.OpenElement(30, "line");
                builder.AddMultipleAttributes(31, lineAttributes);
                builder.CloseElement();
            }
            ctx.Nodes.Add(node);
            x += dStep;
        }
            // g
            //builder.CloseElement();
        

    }
    public void DrawVerticalGridSVG(CanvasSVGComponentBase ctx, Length step, bool major)
    {

        var dStep = step.AsPixels();

        var dMargin = PageMargin.AsPixels();
        var dWidth = PageWidth.AsPixels() + dMargin;
        var dHeight = PageHeight.AsPixels() + dMargin;


        List<KeyValuePair<string, object>> attributes;
        if (!major)
        {
            attributes = new List<KeyValuePair<string, object>>()
            {
                new("stroke", "White"),
                new("stroke-width", 1),
                new("stroke-dasharray", "5 1"),
            };
        }
        else
        {
            attributes = new List<KeyValuePair<string, object>>()
            {
                new("stroke", "Black"),
                new("stroke-width", 3),
            };
        }

        var x = dHeight; //left;
        while (x >= dMargin)
        {
            var lineAttributes = new List<KeyValuePair<string, object>>() {
                new("x1", dMargin),
                new("y1", x),
                new("x2", dWidth),
                new("y2", x)
            };
            lineAttributes.AddRange(attributes);

            //$"DrawHorizontalGridSVG {x} {dMargin} {dHeight}".WriteLine(ConsoleColor.Blue);
            void node(RenderTreeBuilder builder)
            {
                builder.OpenElement(30, "line");
                builder.AddMultipleAttributes(31, lineAttributes);
                builder.CloseElement();
            }
            ctx.Nodes.Add(node);
            x -= dStep;
        }
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
