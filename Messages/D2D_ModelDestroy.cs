using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Message;

public class D2D_ModelDestroy : D2D_Base
{
    public string TargetId { get; set; }


    public string PayloadType { get; set; }
    public D2D_ModelDestroy()
    {
        TargetId  = PayloadType = "";
    }
    public D2D_ModelDestroy(string guid, object data)
    {
        TargetId = guid;
        PayloadType = data.GetType().Name;
    }

}
