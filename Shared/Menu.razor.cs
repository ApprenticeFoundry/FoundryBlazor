using BlazorComponentBus;
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
    [Inject] private ComponentBus? PubSub { get; set; }

    public List<IFoMenu> AllMenus = new();

    protected override async Task OnInitializedAsync()
    {
        var drawing = Workspace?.GetDrawing();
        var arena = Workspace?.GetArena();

        drawing?.CreateMenus(JsRuntime!, Navigation!);

        arena?.CreateMenus(JsRuntime!, Navigation!);

        // drawing?.EstablishMenu<FoMenu2D>("Main2D", new Dictionary<string, Action>()
        // {
        //     { "New", async () => await OpenNewWindow()},
        // }, false);

        // arena?.EstablishMenu<FoMenu3D>("Main3D", new Dictionary<string, Action>()
        // {
        //     { "New", async () => await OpenNewWindow()},
        // }, false);

        arena?.EstablishMenu<FoMenu3D>("BlazorThreeJS", new Dictionary<string, Action>()
        {
            { "Example1", async () => await Navigate("example1")},
            { "Example2", async () => await Navigate("example2")},
            { "Example3", async () => await Navigate("example3")},
            { "Example4", async () => await Navigate("example4")},
            { "Example5", async () => await Navigate("example5")},
            { "Viewer 3D", async () => await Navigate("viewer3D")}
        }, false);

        await base.OnInitializedAsync();
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
        if (FoPage2D.RefreshMenus && Workspace != null)
        {
            AllMenus.Clear();
            AllMenus = Workspace.CollectMenus(AllMenus);

            // AllMenus.ForEach(item =>
            // {
            //     item.Name.WriteLine();
            // });

            FoPage2D.RefreshMenus = false;
        }
        return AllMenus;
    }


}
