using FoundryBlazor.Canvas;
using FoundryBlazor.Dialogs;
using FoundryBlazor.Services;
using FoundryBlazor.Shape;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;


public class Stencil
{
    private IWorkspace Workspace { get; set; }
    private DialogService Dialog { get; set; }
    private IJSRuntime jsRuntime { get; set; }
    public Stencil(IWorkspace space, DialogService dialog, IJSRuntime js)
    {
        Workspace = space;
        Dialog = dialog;
        jsRuntime = js;
    }  

    public void CreateMenus(IJSRuntime js, NavigationManager nav)
    {

        Workspace.GetDrawing()?.EstablishMenu<FoMenu2D>("Create", new Dictionary<string, Action>()
        {
            { "Blue Shape", () => SetDoCreateBlue()},
            { "Text Shape", () => SetDoCreateText()},
            { "Image Shape", () => SetDoCreateImage()},
            { "Image URL", () => SetDoAddImage()},
            { "Video URL", () => SetDoAddVideo()},
        }, true)
        .ToggleLayout().MoveTo(0, 80);

    }

    public void SetDoCreateBlue()
    {

        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoShape2D(150, 100, "Blue");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing()?.AddShape<FoShape2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        //SendShapeCreated(target);
                        //SendToast(UserToast.Success("Created"));
                    }},
                    { "OnCancel", () => {
                        Workspace.GetDrawing()?.ExtractShapes(target.GlyphId);
                    }}
                };
                var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };
                await Dialog.OpenAsync<BlueRectangle>("Create Square", parmas, options);
            };

            shape.OpenCreate();

        });
    }


    public void SetDoCreateText()
    {
        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoText2D(200, 100, "Red");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing()?.AddShape<FoText2D>(shape);

            shape.DoOnOpenCreate = shape.DoOnOpenEdit = async (target) =>
            {
                var parmas = new Dictionary<string, object>() {
                    { "Shape", target },
                    { "OnOk", () => {
                        //SendShapeCreated(target);
                        //SendToast(UserToast.Success("Created"));
                    }},
                    { "OnCancel", () => {
                        Workspace.GetDrawing()?.ExtractShapes(target.GlyphId);
                    }}
                };
                var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };
                await Dialog.OpenAsync<TextRectangle>("Create Text", parmas, options);
            };

            shape.OpenCreate();

        });
    }

    private void SetDoCreateImage()
    {
        var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };

        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var shape = new FoImage2D(200, 100, "Red");
            shape.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing()?.AddShape<FoImage2D>(shape);

            var parmas = new Dictionary<string, object>() {
                { "Shape", shape },
                { "OnOk", () => {
                    //SendShapeCreated<FoImage2D>(shape);
                    //SendToast(UserToast.Success("Created"));
                    //MenuEventService.Notify("ADD_MEDIA");

                } },
                { "OnCancel", () => {
                    Workspace.GetDrawing()?.ExtractShapes(shape.GlyphId);
                }}
            };

            Task.Run(async () =>
            {
                await Dialog.OpenAsync<ImageUploader>("Upload Image", parmas, options);
            });

        });

    }

    private void SetDoAddImage()
    {
        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var r1 = SPEC_Image.RandomSpec();
            var shape1 = new FoImage2D(r1.width, r1.height, "Yellow")
            {
                ImageUrl = r1.url,
            };
            shape1.MoveTo(args.OffsetX, args.OffsetY);
            Workspace.GetDrawing()?.AddShape<FoImage2D>(shape1);

            // var r2 = SPEC_Image.RandomSpec();
            // var shape2 = new FoImage2D(r2.width, r2.height, "Blue")
            // {
            //     ImageUrl = r2.url
            // };
            // shape2.MoveTo(args.OffsetX, args.OffsetY);
            // PageManager.Add<FoImage2D>(shape2);

            //MenuEventService.Notify("ADD_MEDIA");
        });
    }


    private void SetDoAddVideo()
    {
        Workspace.GetDrawing()?.SetDoCreate((CanvasMouseArgs args) =>
         {
             var r1 = new UDTO_Image()
             {
                 width = 400,
                 height = 300,
                 url = "https://upload.wikimedia.org/wikipedia/commons/7/79/Big_Buck_Bunny_small.ogv"
             };
             var shape = new FoVideo2D(r1.width, r1.height, "Yellow")
             {
                 ImageUrl = r1.url,
                 ScaleX = 2.0,
                 ScaleY = 2.0,
                 JsRuntime = jsRuntime
             };
             shape.MoveTo(args.OffsetX, args.OffsetY);
             Workspace.GetDrawing()?.AddShape<FoVideo2D>(shape);
         });
    }



}
