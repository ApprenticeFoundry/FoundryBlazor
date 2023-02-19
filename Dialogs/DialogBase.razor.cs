
using Microsoft.AspNetCore.Components;
using Radzen;

namespace FoundryBlazor.Dialogs;

public class DialogBase : ComponentBase
{
    [Inject] public DialogService? DialogService { get; set; }


    public void Close(bool result)
    {
        DialogService?.Close(result);
    }
    public virtual void Cancel()
    {
        //Console.WriteLine("Cancel");
        Close(false);
    }

    public virtual void Save()
    {
        // Console.WriteLine("Base Save");
        Close(true);
    }
}
