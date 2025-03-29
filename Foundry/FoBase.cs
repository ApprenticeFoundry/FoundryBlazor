using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using System.Text.Json.Serialization;

namespace FoundryBlazor;


public class FoBase: ITreeNode
{
    public string Key { get; set; }
    protected StatusBitArray StatusBits = new();
    private ControlParameters? metaData { get; set; }

    [JsonIgnore] 
    public bool IsActive { get; set; } = false;

    [JsonIgnore] 
    public bool IsVisible
    {
        get { return this.StatusBits.IsVisible; }
        set { this.StatusBits.IsVisible = value; }
    }
    [JsonIgnore] 
    public bool ShouldRender { 
        get { return this.StatusBits.ShouldRender; } 
        set { this.StatusBits.ShouldRender = value; } 
    }


    
    [JsonIgnore] 
    public bool IsDirty
    {
        get { return this.StatusBits.IsDirty; }
        set { 
            this.StatusBits.IsDirty = value; 
            //if ( value )
            //{
            //    $"Key {this.Key} is dirty".WriteNote();
            //}
        }
    }

    public virtual void SetDirty(bool value, bool deep=true)
    {
        if ( IsDirty == value )
            return;

        IsDirty = value;
        // if ( deep)
        // {
        //     foreach (var child in children)
        //     {
        //         child.SetDirty(value, deep);
        //     }
        // }
    }

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

    [JsonIgnore] 
    public bool Selectable 
    { 
        get { return this.StatusBits.IsSelectable; } 
        set { this.StatusBits.IsSelectable = value; } 
    }
    
    [JsonIgnore] 
    public bool IsSelected
    {
        get { return this.StatusBits.IsSelected; }
        set { this.StatusBits.IsSelected = value; }
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
