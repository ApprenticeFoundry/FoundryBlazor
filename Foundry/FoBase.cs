using FoundryRulesAndUnits.Models;

namespace FoundryBlazor;


public class FoBase
{
    public string Key { get; set; }
    public StatusBitArray StatusBits = new();
    private ControlParameters? metaData { get; set; }

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
}
