using FoundryBlazor.Shape;

namespace FoundryBlazor;

public interface IFoComponent
{
    bool HasSlot<T>() where T : FoBase;
    T Establish<T>(string key) where T : FoBase;
    T? Find<T>(string key) where T : FoBase;
    List<T> Members<T>() where T : FoBase;
}

public class SlotGroups<V>: Dictionary<string, FoCollection<V>> where V : FoBase
{
    public FoCollection<U> EstablishSlot<U>() where U: V
    {
        var key = typeof(U).Name;
        if ( ContainsKey(key) == false )
        {
            var result = Activator.CreateInstance<FoCollection<U>>();
            return Add(key, result);
        }
        return (this[key] as FoCollection<U>)!;
    }

    public FoCollection<U>? FindSlot<U>() where U : V
    {
        var key = typeof(U).Name;
        var found = ContainsKey(key) == true ? this[key] : null;

        return found as FoCollection<U>;
    }
    private FoCollection<U> Add<U>(string key, FoCollection<U> found) where U : V
    {
        Add(key, found);
        return found;
    }
}

public class FoComponent : FoBase, IFoComponent
{
    private SlotGroups<FoBase> Slots { get; set; } = new();

    public FoComponent(string name = "") : base(name)
    {
    }

    public virtual bool OpenEdit() { return false; }
    public virtual bool OpenCreate() { return false; }



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
        target?.Remove(value);
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
        if (target == null)
        {
            return null as T;
        }

        if (target!.TryGetValue(key, out T? found) == false)
        {
            return null;
        }
        return (found as T)!;
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

    public virtual List<FoBase> AllMembers()
    {
        var list = new List<FoBase>();
        foreach (var item in Slots.Values)
        {
            if (item is FoCollection<FoBase> col)
                list.AddRange(col.Values());
        }
        return list;
    }


    public virtual T Establish<T>(string key) where T : FoBase
    {
        FoCollection<T> target = Slot<T>();
        if (target.TryGetValue(key, out T? found) == false)
        {
            found = Activator.CreateInstance<T>();
            found.Name = key;
            target.Add(key, found);
        }
        return (found as T)!;
    }
}
