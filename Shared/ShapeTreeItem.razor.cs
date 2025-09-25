using BlazorComponentBus;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shared;


public partial class ShapeTreeItem : ComponentBase
{

    [Inject] private ComponentBus? PubSub { get; set; }
    [Inject] private IJSRuntime? JSRuntime { get; set; }

    [Parameter]
    public ITreeNode? Parent { get; set; } = null;
    [Parameter]
    public IEnumerable<ITreeNode>? Items { get; set; }
    [Parameter]
    public int Level { get; set; } = 1;
    [Parameter]
    public EventCallback<ITreeNode> OnSelect { get; set; }

    // Context menu state
    protected bool contextMenuVisible = false;
    protected ITreeNode? contextMenuItem = null;
    protected double contextMenuX = 0;
    protected double contextMenuY = 0;

    protected IEnumerable<ITreeNode> GetItems()
    {
        var list = new List<ITreeNode>();
        if ( Items != null)
            list.AddRange(Items.Where(x => x != null).ToList());
        return list;
    }
    
    protected async Task ItemSelected(ITreeNode selectedItem)
    {
        // Close any open context menu when selecting items
        contextMenuVisible = false;
        contextMenuItem = null;
        
        selectedItem.SetSelected(!selectedItem.GetIsSelected());
        await OnSelect.InvokeAsync(selectedItem);
        
        // Auto-scroll to make the selected item visible
        await ScrollIntoView($"tree-item-{selectedItem.GetHashCode()}");
    }

    protected async Task ToggleExpanded(ITreeNode item)
    {
        item.ToggleExpanded();
        
        // Wait for the DOM to update, then scroll to show newly expanded content
        await Task.Delay(10);
        await ScrollIntoView($"tree-item-{item.GetHashCode()}");
    }

    protected void ToggleContextMenu(ITreeNode item, MouseEventArgs e)
    {
        if (contextMenuVisible && contextMenuItem == item)
        {
            // Close if clicking same item
            contextMenuVisible = false;
            contextMenuItem = null;
        }
        else
        {
            // Open context menu at mouse position
            contextMenuVisible = true;
            contextMenuItem = item;
            contextMenuX = e.ClientX;
            contextMenuY = e.ClientY;
        }
        StateHasChanged();
    }

    protected async Task ExecuteContextAction(ITreeNode item, TreeNodeAction action)
    {
        // Close context menu
        contextMenuVisible = false;
        contextMenuItem = null;
        
        // Execute the action
        await EvalAction(item, action, Parent);
        StateHasChanged();
    }

    private async Task ScrollIntoView(string elementId)
    {
        if (JSRuntime != null)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("scrollIntoViewIfNeeded", elementId);
            }
            catch
            {
                // Ignore JS errors - scrolling is a nice-to-have feature
            }
        }
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