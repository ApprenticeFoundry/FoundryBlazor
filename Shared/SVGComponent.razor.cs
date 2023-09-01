using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FoundryBlazor.Shared;

public partial class SVGComponentBase : ComponentBase, IDisposable
{
    // [Parameter, NotNull]
    // public required RenderFragment<Circle> Entity { get; set; }

    public List<RenderFragment> Nodes { get; set; } = new();
    protected override void OnInitialized()
    {
    }

    public void Dispose()
    {
    }

    public void DrawCircle()
    {
        $"DrawCircle Called".WriteInfo();
        var attributes = new List<KeyValuePair<string, object>>() { new("r", 50), new("cx", 100), new("cy", 140), new("fill", "red") };

        void node(RenderTreeBuilder builder)
        {
            var i = 0;
            builder.OpenElement(i++, "circle");
            builder.AddMultipleAttributes(i++, attributes);
            builder.CloseElement();
        }

        Nodes.Add(node);

        StateHasChanged();

    }

    public void DrawRect()
    {
        $"DrawCircle Called".WriteInfo();
        var attributes = new List<KeyValuePair<string, object>>() { new("x", 120), new("y", 10), new("width", 300), new("height", 100), new("rx", 4), new("ry", 4), new("style", "fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)") };


        void node(RenderTreeBuilder builder)
        {
            var i = 0;
            builder.OpenElement(i++, "g");
            builder.AddAttribute(i++, "transform", "translate(20,2.5) rotate(10)");

            builder.OpenElement(i++, "rect");
            builder.AddMultipleAttributes(i++, attributes);
            builder.CloseElement();

            // close g
            builder.CloseElement();
        }

        Nodes.Add(node);

        StateHasChanged();

    }

    public void Refresh()
    {
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    protected void ClickRect()
    {
        "ClickRect".WriteInfo();
    }

    protected void ClickCircle()
    {
        "ClickCircle".WriteInfo();
    }

    protected void MouseEnterCircle()
    {
        "MouseEnterCircle".WriteInfo();
    }

    protected void MouseLeaveCircle()
    {
        "MouseLeaveCircle".WriteInfo();
    }

    protected void MouseDownCircle()
    {
        "MouseDownCircle".WriteInfo();
    }

    protected void MouseUpCircle()
    {
        "MouseUpCircle".WriteInfo();
    }

}
