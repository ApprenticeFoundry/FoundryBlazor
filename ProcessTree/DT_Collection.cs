
using System.Collections.Generic;
using System.Linq;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;

[System.Serializable]
public class DT_Collection<T> where T : DT_Base
{
    public string Key { get; set; }
    private readonly Dictionary<string, T> members = new();

    public DT_Collection()
    {
        Key = typeof(T).Name;
    }

    public List<string> Keys()
    {
        return this.members.Keys.ToList();
    }

    public List<T> Values()
    {
        return this.members.Values.ToList<T>();
    }

    public bool TryGetValue(string key, out T? found)
    {
        found = null;
        if (this.members.ContainsKey(key))
            return this.members.TryGetValue(key, out found);
        return false;
    }
    public List<T> AddRange(List<T> list)
    {
        list.ForEach(item => Add(item));
        return list;
    }
    
    public T Add(T value)
    {
        if ( string.IsNullOrEmpty(value.name)) {
            value.name = $"{typeof(T).Name}-{this.members.Count}";
        }
        return this.Add(value.name, value);
    }

    public T Add(string key, T value)
    {
        if (!TryGetValue(key, out T? found) || found == null)
        {
            this.members.Add(key, value);
        }
        return value;
    }

    public T Remove(T value)
    {
        if ( string.IsNullOrEmpty(value.name)) {
            value.name = $"{typeof(T).Name}-{this.members.Count}";
        }
        return this.Remove(value.name, value);
    }
    public bool Remove(string key)
    {
        if (TryGetValue(key, out T? found) && found != null)
        {
            this.members.Remove(key);
            return true;
        }
        return false;
    }

    public T Remove(string key, T value)
    {
        if (TryGetValue(key, out T? found) && found != null)
        {
            this.members.Remove(key);
        }
        return value;
    }

    public List<T> ExtractWhere(Func<T,bool> whereClause)
    {
        var extraction = FindWhere(whereClause);
        extraction.ForEach(item => this.members.Remove(item.name));
        return extraction;
    }

    public List<T> FindWhere(Func<T,bool> whereClause)
    {
        var extraction = Values().Where(item => whereClause(item)).ToList();
        return extraction;
    }

    public void Flush()
    {
        members.Clear();
    }
}
