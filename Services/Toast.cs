
using FoundryBlazor.Message;
using Radzen;
namespace FoundryBlazor.Shared;

public interface IToast
{
    void Info(string message, string? summary = null);
    void Success(string message, string? summary = null);
    void Warning(string message, string? summary = null);
    void Error(string message, string? summary = null);
    void RenderToast(D2D_UserToast toast);
}

public class Toast : IToast
{
    private NotificationService? _notificationService { get; set; }
    private static NotificationMessage NotificationDefault(NotificationSeverity severity, string message, string? summary = null)
    {
        var n = new NotificationMessage
        {
            Severity = severity,
            Summary = summary,
            Detail = message,
            Duration = 4000
        };
        return n;
    }

    public Toast(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }


    public void RenderToast(D2D_UserToast toast)
    {
        switch (toast.Name)
        {
            case "Info":
                Info(toast.Message);
                break;
            case "Warning":
                Warning(toast.Message);
                break;
            case "Error":
                Error(toast.Message);
                break;
            case "Success":
                Success(toast.Message);
                break;
        }
    }
    // public void ClearNotificationService()
    // {
    //     _notificationService = null;
    // }

    public void Info(string message, string? summary = null)
    {
        var n = NotificationDefault(NotificationSeverity.Info, message, summary);
        _notificationService?.Notify(n);
    }


    public void Success(string message, string? summary = null)
    {
        var n = NotificationDefault(NotificationSeverity.Success, message, summary);
        _notificationService?.Notify(n);
    }


    public void Warning(string message, string? summary = null)
    {
        var n = NotificationDefault(NotificationSeverity.Warning, message, summary);
        _notificationService?.Notify(n);
    }


    public void Error(string message, string? summary = null)
    {
        var n = NotificationDefault(NotificationSeverity.Error, message, summary);
        _notificationService?.Notify(n);

    }

}