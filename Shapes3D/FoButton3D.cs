using Blazor.Extensions.Canvas.Canvas2D;


namespace FoundryBlazor.Shape;

public class FoButton3D : FoText3D, IFoButton
{
    public Action? OnClick;

    private string text = "";
    public string Text { get { return this.text; } set { this.text = CreateDetails(AssignText(value, text)); } }
    public List<string>? Details { get; set; }

    public Action ClickAction()
    {
        return OnClick ?? (() => { });
    }

    public string DisplayText()
    {
        return Text;
    }

    protected string CreateDetails(string text)
    {
        Details = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        return text;
    }

    protected string AssignText(string newValue, string oldValue)
    {
        if (newValue != oldValue)
        {
            //ComputeResize = true;
        }

        return newValue;
    }

    public FoButton3D(string command, Action action) : base(command)
    {
        // FontSize = "20";
        // Font = "Segoe UI";
        Text = command;
        OnClick = action;
        //$"Adding FoButton3D  {Text}".WriteLine();
        // ResizeTo(75, 50);

        // ResetLocalPin((obj) => 0, (obj) => 0);
    }



 

}
