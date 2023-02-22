using Microsoft.AspNetCore.Components.Forms;

namespace FoundryBlazor.Canvas
{
    /// <summary>
    /// Mouse information passed from JavaScript
    /// </summary>
    /// 
    //https://developer.mozilla.org/en-US/docs/Web/API/DataTransfer
    //https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Recommended_drag_types
    //event.dataTransfer.setData("text/plain", "This is text to drag");
    
    //public class FileTransfer
    //{
    //    public string name { get; set; }
    //    public long size { get; set; }
    //}
    
    // public class DataTransfer
    // {
    //     public string dropEffect { get; set; } = "";
    //     public string effectAllowed { get; set; } = "";
    //     //public List<FileTransfer>? files { get; set; }
    //     //public List<dynamic>? items { get; set; }
    //     public List<string>? types { get; set; }
    // }

    public class CanvasDragArgs : CanvasMouseArgs
    {
        //public DataTransfer? DataTransfer { get; set; }

    }
}
