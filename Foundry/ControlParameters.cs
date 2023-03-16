namespace FoundryBlazor;


[System.Serializable]
public class ControlParameters
{
    private Dictionary<string, object>? lookup { get; set; }


    public void Establish(string key, object value)
    {
        lookup ??= new Dictionary<string, object>();
        lookup[key] = value;
    }
    public object? Find(string key)
    {
        if (lookup?.TryGetValue(key, out object? value) == true) return value;
        return null;
    }

}
