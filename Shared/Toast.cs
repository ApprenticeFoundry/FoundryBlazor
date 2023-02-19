using FoundryBlazor.Message;
using Radzen;
namespace FoundryBlazor.Shared;

public interface IToast
{
    void Info(string message);
    void Success(string message);
    void Warning(string message);
    void Error(string message);
    void SetNotificationService(NotificationService notificationService);
    void ClearNotificationService();
    void RenderToast(D2D_UserToast toast);
}

public class Toast : IToast
{
    private NotificationService? _notificationService { get; set; }
    private static NotificationMessage NotificationDefault(NotificationSeverity severity, string message)
    {
        var n = new NotificationMessage
        {
            Severity = severity,
            Summary = null,
            Detail = message,
            Duration = 4000
        };
        return n;
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
    public void ClearNotificationService()
    {
        _notificationService = null;
    }
    public void SetNotificationService(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void Info(string message)
    {
        var n = NotificationDefault(NotificationSeverity.Info, message);
        _notificationService?.Notify(n);
    }
    public void Success(string message)
    {
        var n = NotificationDefault(NotificationSeverity.Success, message);
        _notificationService?.Notify(n);
    }

    public void Warning(string message)
    {
        var n = NotificationDefault(NotificationSeverity.Warning, message);
        _notificationService?.Notify(n);
    }

    public void Error(string message)
    {
        var n = NotificationDefault(NotificationSeverity.Error, message);
        _notificationService?.Notify(n);

    }

}