using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryBlazor.Model;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace FoundryBlazor.Shared;

public class CommandManager : ComponentBase
{
    [Inject] public NavigationManager? Navigation { get; set; }
    [Inject] protected IJSRuntime? JsRuntime { get; set; }
    [Inject] public IWorkspace? Workspace { get; init; }
    [Inject] public IDTARSolution? DTARSolution { get; set; }

    public List<FoCommand2D> AllCommands = new();


    // protected override async Task OnInitializedAsync()
    // {
    //     var url = DTARSolution?.GetServerUrl() ?? "";
    //     Workspace?.CreateCommands(JsRuntime!, Navigation!, url);
    //     await base.OnInitializedAsync();
    // }

    public List<FoCommand2D> GetAllCommands()
    {
        if (FoWorkspace.RefreshCommands)
        {
            AllCommands.Clear();
            var commands = Workspace?.GetAllCommands();

            Workspace?.GetAllCommands().ForEach(item => AllCommands.Add(item) );


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
