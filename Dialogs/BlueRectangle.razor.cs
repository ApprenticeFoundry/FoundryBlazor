using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;


namespace FoundryBlazor.Dialogs;

public partial class BlueRectangleDialog : DialogBase
{

    [Parameter] 
    public FoShape2D? Shape { get; set; }

    [Parameter] 
    public Action? OnOK { get; set; }

    [Parameter] 
    public Action? OnCancel { get; set; }



    public string color = "rgb(200, 0, 0)";
    public bool showHSV = true;
    public bool showRGBA = true;
    public bool showColors = true;
    public bool showButton = false;

    protected override void OnInitialized()
    {
        if (Shape != null)
            color = Shape.Color;
    }
    public void OnChange(string value)
    {
        if (Shape != null)
            Shape.Color = value;
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
