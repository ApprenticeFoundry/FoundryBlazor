namespace FoundryBlazor.PubSub;


public class RefreshUIEvent
{
    public string? note { get; set; }

    public RefreshUIEvent(string? note)
    {
        this.note = note;
    }
}


