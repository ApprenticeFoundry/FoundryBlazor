namespace FoundryBlazor;


[System.Serializable]
public class ControlParameters
{
    private Dictionary<string, object>? _lookup { get; set; }


    public void Establish(string key, object value)
    {
        _lookup ??= new Dictionary<string, object>();
        _lookup[key] = value;
    }
    public object? Find(string key)
    {
        if (_lookup?.TryGetValue(key, out object? value) == true) return value;
        return null;
    }

}
