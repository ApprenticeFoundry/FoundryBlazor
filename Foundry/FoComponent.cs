using FoundryRulesAndUnits.Models;
using System.Text.Json.Serialization;

namespace FoundryBlazor;

public interface IFoComponent
{
    bool HasSlot<T>() where T : FoBase;
    bool RemoveSlot<T>() where T : FoBase;
    T Establish<T>(string key) where T : FoBase;
    T? Find<T>(string key) where T : FoBase;
    List<T> Members<T>() where T : FoBase;
}

public class SlotGroups: Dictionary<string, object>
{
    public IFoCollection EstablishSlotFor(Type TypeSpec)
    {
        var key = TypeSpec.Name;
        if (ContainsKey(key) == false)
        {
            var type = typeof(FoCollection<>).MakeGenericType(TypeSpec);
            var result = Activator.CreateInstance(type);
            Add(key, result!);
        }
        return (this[key] as IFoCollection)!;
    }

    public FoCollection<U> EstablishSlot<U>() where U: FoBase
    {
        var key = typeof(U).Name;
        if ( ContainsKey(key) == false )
        {
            var result = Activator.CreateInstance<FoCollection<U>>();
            Add(key, result);
            return result;
        }
        return (this[key] as FoCollection<U>)!;
    }

    public FoCollection<U>? FindSlot<U>() where U : FoBase
    {
        var key = typeof(U).Name;
        var found = ContainsKey(key) == true ? this[key] : null;

        return found as FoCollection<U>;
    }

}

public class FoComponent : FoBase, IFoComponent
{
    public string? Name { get; set; }

    [JsonPropertyName("@type")]
    public string Type { get; init; }
    private SlotGroups Slots { get; set; } = new();

    public Func<FoComponent?> GetParent = () => null;

    public FoComponent(string key = "") : base(key)
    {
        Type = GetType().Name;
    }

    public Func<bool> OpenCreater { get; set; } = null!;
    public Func<bool> OpenEditor { get; set; } = null!;
    public Func<bool> OpenViewer { get; set; } = null!;


    public virtual IFoCollection DynamicSlot(Type type)
    {
        var found = Slots.EstablishSlotFor(type);
        return found;
    }

    public T? GetParentOfType<T>() where T : FoComponent
    {
        if ( this is T)
            return this as T;


        var parent = GetParent();
        if ( parent == null)
            return null;
            
        if ( parent is T)
            return parent as T;

        return parent.GetParentOfType<T>();
    }

    public virtual FoCollection<T> Slot<T>() where T : FoBase
    {
        var found = Slots.EstablishSlot<T>();
        return found;
    }

    public bool HasSlot<T>() where T : FoBase
    {
        var key = typeof(T).Name;
        return Slots.ContainsKey(key);
    }
    public bool RemoveSlot<T>() where T : FoBase
    {
        var key = typeof(T).Name;
        if (Slots.ContainsKey(key) )
        {
            Slots.Remove(key);
            return true;
        }
        return false;
    }

    public virtual FoCollection<T>? GetSlot<T>() where T : FoBase
    {
        return Slots.FindSlot<T>();
    }

    public virtual T Add<T>(T value) where T : FoBase
    {
        var target = Slot<T>();
        target.Add(value);
        return value;
    }

    public virtual T Add<T>(string key, T value) where T : FoBase
    {
        var target = Slot<T>();
        target.Add(key, value);
        return value;
    }

    public virtual T Remove<T>(T value) where T : FoBase
    {
        var target = GetSlot<T>();
        if (target == null)
            return value;

        target.Remove(value);
        return value;
    }

    public virtual bool Remove<T>(string key) where T : FoBase
    {
        if (Slots.ContainsKey(typeof(T).Name))
        {
            var target = Slot<T>();
            return target.Remove(key);
        }
        return false;
    }

    public virtual T? Find<T>(string key) where T : FoBase
    {
        var target = Slots.FindSlot<T>();
        if (target == null) return null;


        if (target.TryGetValue(key, out T? found) == false)
        {
            return null;
        }
        return (found as T)!;
    }

    public virtual List<T>? ExtractWhere<T>(Func<T, bool> whereClause) where T : FoBase
    {
        var target = GetSlot<T>();
        return target?.ExtractWhere(whereClause);
    }

    public virtual List<T>? FindWhere<T>(Func<T, bool> whereClause) where T : FoBase
    {
        var target = GetSlot<T>();
        return target?.FindWhere(whereClause);
    }
    
    public virtual List<T>? GetMembers<T>() where T : FoBase
    {
        FoCollection<T>? target = GetSlot<T>();
        return target?.Values();
    }

    public virtual List<T> Members<T>() where T : FoBase
    {
        FoCollection<T> target = Slot<T>();
        return target.Values();
    }


    public virtual T Establish<T>(string key) where T : FoBase
    {
        FoCollection<T> target = Slot<T>();
        if (target.TryGetValue(key, out T? found) == false)
        {
            found = (Activator.CreateInstance(typeof(T),key) as T)!;
            target.Add(key, found);
        }
        return (found as T)!;
    }



    protected ITreeNode FolderOf<T>() where T : FoBase
    {
        var count = Members<T>().Count;
        var name = typeof(T).Name.Replace("Fo", "");
        
        if ( name.EndsWith("y"))
            name = name[..^1] + "ies";
        else if ( !name.EndsWith("es"))
            name += "s";

        var folder = new FoFolder(name);

        Members<T>().ForEach(item =>
        {
            folder.AddChild(item);
        });
        return folder;
    }

    public void AddFolderIfNotEmpty<T>(List<ITreeNode> list, bool skip=true) where T : FoBase
    {
        var count = GetMembers<T>()?.Count ?? 0;
        if ( count == 0)
            return;

        var folder = FolderOf<T>();
        if ( skip )
            list.AddRange(folder.GetTreeChildren());
        else
            list.Add(folder);  
    }

}
