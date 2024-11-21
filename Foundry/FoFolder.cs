using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor;
public class FoFolder : FoBase
{
    public List<ITreeNode> Children = new();
    public FoFolder(string key = ""): base(key)
    {
        SetExpanded(true);
    }
    public override string GetTreeNodeTitle()
    {
        var count = Children.Count();
        return $"{Key} ({count})";
    }

    public ITreeNode AddChild(FoBase child)
    {
        Children.Add(child);
        return child;
    }
    public ITreeNode AddTreeNode(ITreeNode child)
    {
        Children.Add(child);
        return child;
    }
    public ITreeNode AddRange(IEnumerable<ITreeNode> children)
    {
        Children.AddRange(children);
        return this;
    }

    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var result = new List<ITreeNode>();
        result.AddRange(Children);
        return result;
    }

    public bool AddReference(FoBase? item)
    {
        if ( item == null) 
            return false;

        var list = Children.OfType<FoReference>().ToList();

        var found = list.FirstOrDefault(x => x.IsReferenceTo(item));
        if ( found == null)
        {
            var reference = new FoReference(item);
            AddChild(reference);
        }
        return true;
    }
}


