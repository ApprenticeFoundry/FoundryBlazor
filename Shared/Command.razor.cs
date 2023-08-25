using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
namespace FoundryBlazor.Shared;

public class CommandManager : ComponentBase, IDisposable
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }

    public List<IFoCommand> AllCommands = new();

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

    public List<IFoCommand> GetAllCommands()
    {
        if (FoWorkspace.RefreshCommands && Workspace != null)
        {
            AllCommands.Clear();
            AllCommands = Workspace.CollectCommands(AllCommands);
            FoWorkspace.RefreshCommands = false;

            // AllCommands.ForEach(obj =>
            // {
            //     $"AllCommands commands {obj.Name}".WriteLine();
            //     obj.Buttons().ForEach(but =>
            //     {
            //         $"AllCommands the but {but.DisplayText()}".WriteLine();
            //     });
            // });
        }

        return AllCommands;
    }


}
