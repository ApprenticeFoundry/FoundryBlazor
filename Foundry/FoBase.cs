namespace FoundryBlazor;


public class FoBase
{
    public string Name { get; set; }
    private ControlParameters? _metaData { get; set; }

    public FoBase(string name)
    {
        //if ( !string.IsNullOrEmpty(name))
        Name = name;
        //else
        //    Name = Guid.NewGuid().ToString();
    }
    public ControlParameters MetaData()
    {
        _metaData ??= new ControlParameters();
        return _metaData;
    }


    public bool HasMetaData()
    {
        return _metaData != null;
    }

    public bool HasMetaDataKey(string key)
    {
        if (_metaData != null)
        {
            return _metaData.Find(key) != null;
        }
        return false;
    }

    public ControlParameters AddMetaData(string key, string value)
    {
        MetaData().Establish(key, value);
        return _metaData!;
    }
}
