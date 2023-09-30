using FoundryBlazor.Shared;
using Microsoft.AspNetCore.Components.Forms;
 


namespace FoundryBlazor.PubSub;
public class UploadFileEvent
{
    public IBrowserFile? File { get; set; }
    public CanvasMouseArgs? MouseArgs { get; set; }
    //public UDTO_File? FileInfo { get; set; }
}
