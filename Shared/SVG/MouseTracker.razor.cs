using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shared.SVG;

public class MouseTrackerBase : ComponentBase
{
    [Parameter] public FoPage2D Page { get; set; } = new("page1", "White");
 
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }


    public int X1()
    {
        return 20;
    }
    public int Y1()
    {
        return 30;
    }





}
