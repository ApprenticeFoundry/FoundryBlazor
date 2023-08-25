namespace FoundryBlazor.Shape;

public class FoButton3D : FoGlyph3D, IFoButton
{
    public Action? OnClick;

    private int countdown = 0;
    public bool ComputeResize { get; set; } = false;

    private string text = "";
    public string Text { get { return this.text; } set { this.text = AssignText(value, text); } }


    public Action ClickAction()
    {
        return OnClick ?? (() => { });
    }

    public string DisplayText()
    {
        return Text;
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
