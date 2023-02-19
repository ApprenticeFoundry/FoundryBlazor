using Microsoft.AspNetCore.Components.Forms;
using FoundryBlazor.Canvas;
using IoBTMessage.Models;

namespace FoundryBlazor.PubSub;
public class UploadFileEvent
{
    public IBrowserFile? File { get; set; }
    public CanvasMouseArgs? MouseArgs { get; set; }
    public UDTO_File? FileInfo { get; set; }
}
