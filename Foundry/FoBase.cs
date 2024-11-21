using FoundryRulesAndUnits.Models;

namespace FoundryBlazor;


public class FoBase: ITreeNode
{
    public string Key { get; set; }
    public StatusBitArray StatusBits = new();
    private ControlParameters? metaData { get; set; }

    public FoBase(string name)
    {
        Key = name;
    }
    public ControlParameters MetaData()
    {
        metaData ??= new ControlParameters();
        return metaData;
    }


    public bool HasMetaData()
    {
        return metaData != null;
    }

    public bool HasMetaDataKey(string key)
    {
        if (metaData != null)
        {
            return metaData.Find(key) != null;
        }
        return false;
    }

    public ControlParameters AddMetaData(string key, string value)
    {
        MetaData().Establish(key, value);
        return metaData!;
    }

    public bool GetIsSelected()
    {
        return this.StatusBits.IsSelected;
    }
    public bool SetSelected(bool value)
    {
        this.StatusBits.IsSelected = value;
        return value;
    }

    public bool GetIsExpanded()
    {
        return this.StatusBits.IsExpanded;
    }
    public bool SetExpanded(bool value)
    {
        this.StatusBits.IsExpanded = value;
        return value;
    }

    public virtual string GetTreeNodeTitle()
    {
        return $"{Key} {GetType().Name}";
    }
    public virtual IEnumerable<TreeNodeAction>? GetTreeNodeActions()
    {
        return null;
    }

    public virtual IEnumerable<ITreeNode> GetTreeChildren()
    {
        return new List<ITreeNode>();
    }
}
