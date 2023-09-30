using Microsoft.AspNetCore.Components;
using System.Collections.Specialized;
using Radzen;
using Radzen.Blazor;

namespace FoundryBlazor.Shared;

public class ToastNotificationComponent : ComponentBase, IDisposable
{
    [Inject] public NotificationService? Service { get; set; }
    [Inject] private IToast? Toast { get; set; }

    [Parameter]
    public string? Style { get; set; }

    public RenderFragment DrawMessage(int index, NotificationMessage m)
    {
        return new RenderFragment(builder =>
        {
            builder.OpenComponent(0, typeof(RadzenNotificationMessage));
            builder.AddAttribute(1, "Message", m);
            builder.AddAttribute(2, "Style", m.Style);
            builder.CloseComponent();
        });
    }

    void Update(object sender, NotifyCollectionChangedEventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (Service != null)
        {
            Toast?.ClearNotificationService();
            Service.Messages.CollectionChanged -= Update!;
        }
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        if (Service != null)
        {
            Toast?.SetNotificationService(Service);
            Service.Messages.CollectionChanged += Update!;
        }
    }
}