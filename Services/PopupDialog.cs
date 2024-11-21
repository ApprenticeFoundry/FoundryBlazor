
using FoundryBlazor.Message;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Reflection.Metadata;
namespace FoundryBlazor.Shared;

public interface IPopupDialog
{

    void Open<T>(string title, Dictionary<string, object> parameters = null!, DialogOptions options = null!) where T : ComponentBase;
    Task<dynamic> OpenSideAsync<T>(string title, Dictionary<string, object> parameters = null!, SideDialogOptions options = null!) where T : ComponentBase;
    void Alert(string message, string title, AlertOptions options = null!);
    Task<bool?> Confirm(string message, string title, ConfirmOptions options = null!);
    void Close(dynamic result=null!);
}

public class PopupDialog : IPopupDialog
{
    private DialogService? _dialogService { get; set; }


    public PopupDialog(DialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public void Close(dynamic result=null!)
    {
        _dialogService?.Close(result);
    }

    public void Open<T>(string title, Dictionary<string, object> parameters = null!, DialogOptions options = null!) where T : ComponentBase
    {
        _dialogService?.Open<T>(title, parameters, options);
    }

    public async Task<dynamic> OpenSideAsync<T>(string title, Dictionary<string, object> parameters = null!, SideDialogOptions options = null!) where T : ComponentBase
    {
        if ( _dialogService == null) return false;
        return await _dialogService.OpenSideAsync<T>(title, parameters, options);
    }
    public void Alert(string message, string title, AlertOptions options = null!)
    {
        _dialogService?.Alert(message, title, options);
    }

    public async Task<bool?> Confirm(string message, string title, ConfirmOptions options = null!)
    {
        if ( _dialogService == null) return false;
    
        return await _dialogService.Confirm(message, title, options);

    }
}