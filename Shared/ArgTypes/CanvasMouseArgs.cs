namespace FoundryBlazor.Shared
{
    /// <summary>
    /// Mouse information passed from JavaScript
    /// </summary>
    public class CanvasMouseArgs : CanvasArgsBase
    {
        public int ScreenX { get; set; }
        public int ScreenY { get; set; }
        public int ClientX { get; set; }
        public int ClientY { get; set; }
        public int MovementX { get; set; }
        public int MovementY { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public bool AltKey { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool MetaKey { get; set; }
        public bool Bubbles { get; set; }
        public int Buttons { get; set; }
        public int Button { get; set; }
    }
}
