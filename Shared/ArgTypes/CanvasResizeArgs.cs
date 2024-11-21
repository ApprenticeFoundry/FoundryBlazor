using System.Drawing;

namespace FoundryBlazor.Shared
{
    /// <summary>
    /// Keyboard information passed from JavaScript
    /// </summary>
    public class CanvasResizeArgs : CanvasArgsBase
    {
        public Size size { get; set; }
    }
}
