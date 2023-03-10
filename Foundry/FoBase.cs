namespace FoundryBlazor;


public class FoBase
{
    public string Name { get; set; }
    private ControlParameters? metaData { get; set; }

    public FoBase(string name)
    {
        //if ( !string.IsNullOrEmpty(name))
        Name = name;
        //else
        //    Name = Guid.NewGuid().ToString();
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
}
