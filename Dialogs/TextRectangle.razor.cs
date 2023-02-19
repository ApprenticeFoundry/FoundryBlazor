using FoundryBlazor.Shape;

using Microsoft.AspNetCore.Components;


namespace FoundryBlazor.Dialogs;

public class TextRectangleDialog : DialogBase
{
    [Parameter] 
    public FoText2D Shape { get; set; }

    [Parameter] 
    public Action? OnOK { get; set; }

    [Parameter] 
    public Action? OnCancel { get; set; }
    
    public TextRectangleDialog()
    {
        Shape = new FoText2D();
    }

     public void OnTextAreaChange(string value)
    {
        if ( Shape != null)
            Shape.Text = value;
    }

    public void OnSpeechCaptured(string speechValue, bool updateTextArea)
    {
        if (updateTextArea && Shape != null)
        {
            Shape.Text = speechValue;
        }
    }

    public void OnInput(object? value)
    {
        Console.WriteLine($"OnInput changed to {value}");
        if ( Shape != null)
            Shape.Text = value?.ToString() ?? "";
    }
    


    public override void Cancel()
    {
        OnCancel?.Invoke();
        base.Cancel();
    }

    public override void Save()
    {
        OnOK?.Invoke();
        base.Save();
    }
}
