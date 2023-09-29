namespace FoundryBlazor.PubSub;


public class RefreshUIEvent
{
    public string? note { get; set; }

    public RefreshUIEvent(string? note)
    {
        this.note = note;
    }
}

public class TriggerRedrawEvent
{
    public string? note { get; set; }

    public TriggerRedrawEvent(string? note)
    {
        this.note = note;
    }
}


