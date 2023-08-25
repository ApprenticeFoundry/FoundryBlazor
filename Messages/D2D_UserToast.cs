namespace FoundryBlazor.Message;

public enum ToastType
{
    Info,
    Warning,
    Error,
    Success,
    Note
}
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
        Name = $"{ToastType.Info}";
        return this;
    }

    public D2D_UserToast Warning(string message)
    {
        Message = message;
        Name = $"{ToastType.Warning}";
        return this;
    }

    public D2D_UserToast Error(string message)
    {
        Message = message;
        Name = $"{ToastType.Error}";
        return this;
    }

    public D2D_UserToast Success(string message)
    {
        Message = message;
        Name = $"{ToastType.Success}";
        return this;
    }
    public D2D_UserToast Note(string message)
    {
        Message = message;
        Name = $"{ToastType.Note}";
        return this;
    }
}
