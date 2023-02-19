using IoBTMessage.Models;

namespace FoundryBlazor.Model;

public interface IDT_Manager
{
    bool HasSlot<T>() where T : DT_Base;
    T Establish<T>(string key) where T : DT_Base;
    T? Find<T>(string key) where T : DT_Base;
    List<T> Members<T>() where T : DT_Base;
}


public class DT_Manager: DT_Base, IDT_Manager
{
    private Dictionary<string, object> Slots { get; set; } = new();

    public DT_Manager(string name=""): base(name)
    {
    }

    public virtual bool OpenEdit() { return false; }
    public virtual bool OpenCreate() { return false; }



    public virtual DT_Collection<T> Slot<T>() where T : DT_Base
    {
        var key = typeof(T).Name;
        var found = Slots.ContainsKey(key) ? Slots[key] : null;
        if ( found == null ) 
       {  
            found = Activator.CreateInstance<DT_Collection<T>>();
            Slots.Add(key, found);
        }
        var result = found as DT_Collection<T>;
        return result!;
    }

    public bool HasSlot<T>() where T : DT_Base
    {
        var key = typeof(T).Name;
        return Slots.ContainsKey(key);
    }

    public virtual DT_Collection<T>? GetSlot<T>() where T : DT_Base
    {
        var key = typeof(T).Name;
        return Slots.ContainsKey(key) ? Slots[key] as DT_Collection<T> : null;
    }

    public virtual T Add<T>(T value) where T: DT_Base
    {
        var target = Slot<T>();
        target.Add(value);
        return value;
    }

    public virtual T Remove<T>(T value) where T: DT_Base
    {
        var target = GetSlot<T>();
        target?.Remove(value);
        return value;
    }

    public virtual bool Remove<T>(string key) where T: DT_Base
    {
        if ( Slots.ContainsKey(typeof(T).Name)  )
        {
            var target = Slot<T>();
            return target.Remove(key);
        }
        return false;
    }

    public virtual T? Find<T>(string key) where T : DT_Base
    {
        if ( Slots.ContainsKey(typeof(T).Name) == false )
        {
            return null as T;
        }

        var target = GetSlot<T>() as DT_Collection<T>;
        if (target!.TryGetValue(key, out T? found) == false)
        {
            return null;
        }
        return (found as T)!;
    }

    public virtual List<T>? GetMembers<T>() where T : DT_Base
    {
        DT_Collection<T>? target = GetSlot<T>();
        return target?.Values();
    }
    public virtual List<T> Members<T>() where T : DT_Base
    {
        DT_Collection<T> target = Slot<T>();
        return target.Values();
    }

    public virtual List<DT_Base> AllMembers()
    {
        var list = new List<DT_Base>();
        foreach (var item in Slots.Values)
        {
            if (item is DT_Collection<DT_Base> col)
                list.AddRange(col.Values());
        }
        return list;
    }

  
    public virtual T Establish<T>(string key) where T : DT_Base
    {
        DT_Collection<T> target = Slot<T>();
        if (target.TryGetValue(key, out T? found) == false)
        {
            found = Activator.CreateInstance<T>();
            found.name = key;
            target.Add(key, found);
        }
        return (found as T)!;
    }  
}
