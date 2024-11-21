using BlazorComponentBus;
using Microsoft.AspNetCore.Components;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shared;




public partial class ShapeTreeItemBase : ComponentBase
{

    [Inject] private ComponentBus? PubSub { get; set; }

    [Parameter]
    public ITreeNode? Parent { get; set; } = null;
    [Parameter]
    public IEnumerable<ITreeNode>? Items { get; set; }
    [Parameter]
    public int Level { get; set; } = 1;
    [Parameter]
    public EventCallback<ITreeNode> OnSelect { get; set; }

    protected IEnumerable<ITreeNode> GetItems()
    {
        var list = new List<ITreeNode>();
        if ( Items != null)
            list.AddRange(Items.Where(x => x != null).ToList());
        return list;
    }
    protected async Task ItemSelected(ITreeNode selectedItem)
    {
        selectedItem.SetSelected(!selectedItem.GetIsSelected());
        await OnSelect.InvokeAsync(selectedItem);
    }

    // protected async Task EvalKnAction(ITreeNode item, KnTreeNodeAction action, ITreeNode? parent)
    // {
    //     await Task.CompletedTask;

    //     $"EvalKnAction {action.Name} {action.Style}".WriteSuccess();
    //     // action.Action.Invoke(MentorServices!); 
    //     // PubSub?.Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());   
    // }

    protected async Task EvalAction(ITreeNode item, TreeNodeAction action, ITreeNode? parent)
    {
        //$"EvalAction {action.Name} {action.Style}".WriteSuccess();
        action.Action.Invoke(); 
        await Task.CompletedTask;
        //PubSub?.Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());   
    }


    
}