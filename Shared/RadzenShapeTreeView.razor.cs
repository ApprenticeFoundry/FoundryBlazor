using Microsoft.AspNetCore.Components;
using Radzen;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shared;

public partial class RadzenShapeTreeViewBase : ShapeTreeViewBase
{
    protected IEnumerable<TreeNodeWrapper> TreeData => GetTreeData();

    protected void OnNodeSelect(TreeEventArgs args)
    {
        if (args.Value is TreeNodeWrapper wrapper)
        {
            wrapper.OriginalNode.SetSelected(!wrapper.OriginalNode.GetIsSelected());
            Console.WriteLine(wrapper.OriginalNode.GetTreeNodeTitle());
        }
    }

    protected void OnNodeExpand(TreeExpandEventArgs args)
    {
        if (args.Value is TreeNodeWrapper wrapper)
        {
            // Toggle the original node's expanded state
            wrapper.OriginalNode.ToggleExpanded();
            
            // Refresh the tree data to show/hide children
            StateHasChanged();
        }
    }

    private IEnumerable<TreeNodeWrapper> GetTreeData()
    {
        return GetAllNodes()?.Select(node => new TreeNodeWrapper(node)) ?? Enumerable.Empty<TreeNodeWrapper>();
    }

    public class TreeNodeWrapper
    {
        public ITreeNode OriginalNode { get; }
        public string DisplayText { get; }
        public List<TreeNodeAction> Actions { get; }
        public IEnumerable<TreeNodeWrapper>? Children => GetChildren();
        public bool HasChildren { get; }

        public TreeNodeWrapper(ITreeNode node)
        {
            OriginalNode = node;
            DisplayText = node.GetTreeNodeTitle();
            Actions = node.GetTreeNodeActions()?.ToList() ?? new List<TreeNodeAction>();
            HasChildren = node.HasChildren();
        }

        private IEnumerable<TreeNodeWrapper>? GetChildren()
        {
            if (HasChildren && !OriginalNode.IsCollapsed())
            {
                return OriginalNode.GetTreeChildren()?.Select(child => new TreeNodeWrapper(child));
            }
            return null;
        }
    }
}
