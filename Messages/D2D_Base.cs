using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Message;

public class D2D_Base
{
    public string Topic() 
    {
        return D2D_Base.AsTopic(GetType().Name);
    }

    public static string AsTopic(string name)
    {
        return name.Replace("D2D_", "");
    }
    
    public string MsgId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string TimeStamp { get; set; } = "";
    public string UserID { get; set; } = "";

    private ControlParameters? _metaData;


    public D2D_Base()
    {
        this.Initialize();
    }



    public string ResetTimeStamp()
    {
        this.TimeStamp = DateTime.UtcNow.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        return this.TimeStamp;
    }



    public static string AsTopic<T>() where T : D2D_Base
    {
        return D2D_Base.AsTopic(typeof(T).Name);
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

    public D2D_Base Initialize()
    {
        if (string.IsNullOrEmpty(Type))
        {
            Type = D2D_Base.AsTopic(this.GetType().Name);
        }
        if (string.IsNullOrEmpty(TimeStamp))
        {
            ResetTimeStamp();
        }
        if (string.IsNullOrEmpty(MsgId))
        {
            MsgId = System.Guid.NewGuid().ToString();
        }
        return this;
    }
}
