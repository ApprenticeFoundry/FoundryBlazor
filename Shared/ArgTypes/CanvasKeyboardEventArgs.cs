namespace FoundryBlazor.Shared
{
    /// <summary>
    /// Keyboard information passed from JavaScript
    /// </summary>
    public class CanvasKeyboardEventArgs : CanvasArgsBase
    {
        public bool AltKey { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool MetaKey { get; set; }
        public bool Bubbles { get; set; }
        public string Code { get; set; } = "";
        public bool IsComposing { get; set; }
        public string Key { get; set; } = "";
        public bool Repeat { get; set; }
        public int Location { get; set; }

    }
}
