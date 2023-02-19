namespace FoundryBlazor.Message;

public class D2D_UserToast : D2D_Base
{
    public string Message { get; set; }
    public D2D_UserToast()
    {
        Message = "";
    }
    public D2D_UserToast Info(string message)
    {
        Message = message;
        Name = "Info";
        return this;
    }

    public D2D_UserToast Warning(string message)
    {
        Message = message;
        Name = "Warning";
        return this;
    }

    public D2D_UserToast Error(string message)
    {
        Message = message;
        Name = "Error";
        return this;
    }

    public D2D_UserToast Success(string message)
    {
        Message = message;
        Name = "Success";
        return this;
    }
}
