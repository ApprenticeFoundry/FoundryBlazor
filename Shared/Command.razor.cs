using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace FoundryBlazor.Shared;

public class CommandManager : ComponentBase
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }

    public List<IFoCommand> AllCommands = new();



    public List<IFoCommand> GetAllCommands()
    {
        if (FoWorkspace.RefreshCommands && Workspace != null)
        {
            AllCommands.Clear();
            AllCommands = Workspace.CollectCommands(AllCommands);


            // AllCommands.ForEach(obj =>
            // {
            //     $"AllCommands commands {obj.Name}".WriteLine();
            //     obj.Buttons().ForEach(but =>
            //     {
            //         $"AllCommands the but {but.DisplayText()}".WriteLine();
            //     });
            // });

            FoWorkspace.RefreshCommands = false;
        }

        return AllCommands;
    }


}
