using Microsoft.AspNetCore.Components;

namespace FoundryBlazor.Shared.SVG;

public class GBase : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Transform { get; set; } = "matrix(1, 0, 0, 1, 1, 1)";

    public string GetTransform()
    {
        return Transform;
    }

    public string SetTransform()
    {
        Transform = "matrix(1, 0, 0, 1, 100, 50)";
        return Transform;
    }

}
