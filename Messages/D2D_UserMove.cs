namespace FoundryBlazor.Message;

public class D2D_UserMove : D2D_Base
{
    public bool Active { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public D2D_UserMove()
    {
        Active = false;
        X = Y = 0;
    }
}
