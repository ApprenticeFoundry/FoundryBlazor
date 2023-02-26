
using System.Collections.Generic;
using System.Linq;


namespace FoundryBlazor;

public interface IFoCollection 
{
   int GetLayer();
   string GetKey();
   void Clear();
}

[System.Serializable]
public class FoCollection<T>: IFoCollection where T : FoBase
{
    public string Key { get; set; }
    protected int Layer { get; set; } = 0;

    private readonly Dictionary<string, T> members = new();

    public FoCollection()
    {
        Key = typeof(T).Name;
    }

    public int GetLayer()
    {
        return Layer;
    }
    public void SetLayer(int layer)
    {
        Layer = layer;
    }
    public string GetKey()
    {
        return Key;
    }

    public List<string> Keys()
    {
        return this.members.Keys.ToList();
    }

    public List<T> Values()
    {
        return this.members.Values.ToList<T>();
    }
    public T? GetValue(string key)
    {
        if (TryGetValue(key, out T? value))
            return value;
        return null;
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
        if ( string.IsNullOrEmpty(value.Name)) {
            value.Name = $"{typeof(T).Name}-{this.members.Count}";
        }
        return this.Add(value.Name, value);
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
        if ( string.IsNullOrEmpty(value.Name)) {
            value.Name = $"{typeof(T).Name}-{this.members.Count}";
        }
        return this.Remove(value.Name, value);
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
        extraction.ForEach(item => this.members.Remove(item.Name));
        return extraction;
    }

    public List<T> FindWhere(Func<T,bool> whereClause)
    {
        var extraction = Values().Where(item => whereClause(item)).ToList();
        return extraction;
    }

    public void Clear()
    {
        members.Clear();
    }

    public FoCollection<T> Flush()
    {
        Clear();
        return this;
    }

}
