using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;


using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
namespace FoundryBlazor.Shared;

public class WorkspaceMenuBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }


    public List<IFoMenu> AllMenus = new();

    protected override void OnInitialized()
    {
        if ( Navigation != null)
            Navigation.LocationChanged += LocationChanged;
    }

    void IDisposable.Dispose()
    {
        if ( Navigation != null)
            Navigation.LocationChanged -= LocationChanged;
    }

    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        StateHasChanged();
    }
 
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
        "GetAllMenus".WriteWarning(3);
        if (FoWorkspace.RefreshMenus && Workspace != null)
        {
            
            AllMenus.Clear();
           //$"GetAllMenus() Clearing menus {AllMenus.Count}".WriteInfo();
            AllMenus = Workspace.CollectMenus(AllMenus);
            FoWorkspace.RefreshMenus = false;

            AllMenus.ForEach(item =>
            {
                item.DisplayText().WriteInfo();
                // item.Buttons().ForEach(but =>
                // {
                //     but.DisplayText().WriteInfo(1);
                // });
            });
        }
        return AllMenus;
    }


}
