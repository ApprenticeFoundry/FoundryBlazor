using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_ModelCreate : D2D_Base
{
    public string TargetId { get; set; }
    public string Payload { get; set; }
    public string PayloadType { get; set; }
    public D2D_ModelCreate()
    {
        TargetId = Payload = PayloadType = "";
    }
    public D2D_ModelCreate(string guid, object data)
    {
        TargetId = guid;
        Payload = StorageHelpers.Dehydrate(data, false);
        PayloadType = data.GetType().Name;
    }

}