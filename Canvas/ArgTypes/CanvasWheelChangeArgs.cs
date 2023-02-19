namespace FoundryBlazor.Canvas
{
    /// <summary>
    /// Mouse wheel information passed from JavaScript
    /// </summary>
    public class CanvasWheelChangeArgs : CanvasArgsBase
    {
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public double DeltaZ { get; set; }
        public int DeltaMode { get; set; }
    }
}
