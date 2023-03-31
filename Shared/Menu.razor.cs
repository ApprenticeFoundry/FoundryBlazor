using BlazorComponentBus;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace FoundryBlazor.Shared;

public class MenuManager : ComponentBase
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }
    //[Inject] private ComponentBus? PubSub { get; set; }

    public List<IFoMenu> AllMenus = new();


 
    private async Task OpenNewWindow()
    {
        var target = Navigation!.ToAbsoluteUri("/");
        try
        {
            if (JsRuntime != null)
                await JsRuntime.InvokeAsync<object>("open", target); //, "_blank", "height=600,width=1200");
        }
        catch { }
    }

    private async Task Navigate(string page)
    {
        var target = Navigation!.ToAbsoluteUri($"/{page}");
        try
        {
            if (JsRuntime != null)
                await JsRuntime.InvokeAsync<object>("open", target); //, "_blank", "height=600,width=1200");
        }
        catch { }
    }

    public List<IFoMenu> GetAllMenus()
    {
        if (FoPage2D.RefreshMenus && Workspace != null)
        {
            AllMenus.Clear();
            AllMenus = Workspace.CollectMenus(AllMenus);
            FoPage2D.RefreshMenus = false;

            // AllMenus.ForEach(item =>
            // {
            //     item.DisplayText().WriteInfo();
            //     item.Buttons().ForEach(but =>
            //     {
            //         but.DisplayText().WriteInfo(1);
            //     });
            // });
        }
        return AllMenus;
    }


}
