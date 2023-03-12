namespace FoundryBlazor.Message;

public class D2D_Create : D2D_Base
{
    public string TargetId { get; set; }
    public string Payload { get; set; }
    public string PayloadType { get; set; }
    public D2D_Create()
    {
        TargetId = Payload = PayloadType = "";
    }
}
