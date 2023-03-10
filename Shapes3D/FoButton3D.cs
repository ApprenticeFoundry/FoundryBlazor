namespace FoundryBlazor.Shape;

public class FoButton3D : FoText3D, IFoButton
{
    public Action? OnClick;


    public Action ClickAction()
    {
        return OnClick ?? (() => { });
    }

    public string DisplayText()
    {
        return Text;
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
