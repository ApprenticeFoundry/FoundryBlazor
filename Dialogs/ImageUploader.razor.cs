
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace FoundryBlazor.Dialogs;

public class ImageUploaderDialog : DialogBase
{
    [Parameter]
    public FoImage2D Shape { get; set; }

    [Parameter]
    public Action? OnOK { get; set; }

    [Parameter]
    public Action? OnCancel { get; set; }


    public ImageUploaderDialog()
    {
        Shape = new FoImage2D();
    }
    public override void Cancel()
    {
        OnCancel?.Invoke();
        base.Cancel();
    }

    public override void Save()
    {
        OnOK?.Invoke();
        base.Save();
    }

    public void OnProgress(UploadProgressArgs args, string name)
    {
        Console.WriteLine($"{args.Progress}% '{name}' / {args.Loaded} of {args.Total} bytes.");

        if (args.Progress == 100)
        {
            foreach (var file in args.Files)
            {
                Console.WriteLine($"Uploaded: {file.Name} / {file.Size} bytes");
            }
        }
    }

    public void OnComplete(UploadCompleteEventArgs args)
    {
        var data = args.RawResponse;
        var response = StorageHelpers.Hydrate<ContextWrapper<UDTO_Image>>(data, true);
        var image = response.payload.First();

        if (image != null)
        {
            if (Shape != null)
            {
                Shape.ImageUrl = image.url;
                Shape.ImageWidth = image.width;
                Shape.ImageHeight = image.height;
                // Shape.Width = image.width;
                // Shape.Height = image.height;

                var scalex = Shape.ImageWidth / (double)Shape.Width;
                var scaley = Shape.ImageHeight / (double)Shape.Height;
                
                //var scale = scalex > scaley ? scalex : scaley;
                Shape.ScaleX = 1.0 / scalex;
                Shape.ScaleY = 1.0 / scaley;

                Console.WriteLine($"ScaleX {Shape.ScaleX} ScaleY {Shape.ScaleY}  {Shape.Width} width={image.width}, {Shape.Height} height={image.height}");

            }
            else
            {
                Console.WriteLine($"image is not null, but Shape is NULL");
            }
        }

    }


}
