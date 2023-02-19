using FoundryBlazor.Canvas;

using FoundryBlazor.Shape;
using FoundryBlazor.Extensions;
using IoBTMessage.Models;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Model;


public class MoSimulation : MoComponent
{
    private IPageManagement PageManager { get; set; }
    private IDrawing Drawing { get; set; }
    private DialogService DialogService { get; set; }
    public MoSimulation(string name, IPageManagement manager, IDrawing drawing, DialogService dialogService):base(name)
    {
        (PageManager, Drawing, DialogService) = (manager, drawing, dialogService);
    }

    public Dictionary<string, Action> MenuItems(IJSRuntime js)
    {

        var menu = new Dictionary<string, Action>()
        {
            { "TVA", () => SetDoCreateTVA(js,"Squire1", "https://iobtsquire1.azurewebsites.net/")},
            //{ "TVA Local", () => SetDoCreateTVA(js,"Local", "https://localhost:5501/")},
            { "Clock", () => SetDoCreateClock()},
            { "Biometric", () => SetDoCreateBiometric()},
            { "Position", () => SetDoCreatePosition()},
            { "Chat", () => SetDoCreateChat()},
            { "System", () => SetDoCreateSystem()},
            { "Image", () => SetDoCreateImage()},
        };
        return menu;
    }

    public MoComponent? FindModel(string ModelId)
    {
        var model = Slot<MoComponent>().FindWhere(child => child.ModelId == ModelId).FirstOrDefault();
        return model;
    }

    public void SetDoCreateTVA(IJSRuntime js, string name, string url)
    {
        var options = new DialogOptions() { Resizable = true, Draggable = true, CloseDialogOnOverlayClick = true };

        async void OpenTVAUrl(string target)
        {
            try { await js.InvokeAsync<object>("open", target); }
            catch { };
        }

    //     Drawing.SetDoCreate((CanvasMouseArgs args) =>
    //     {             
    //         var model = new MoTVANode(name, url, "I AM CANVAS", OpenTVAUrl);
    //         Add<MoComponent>(model);

    //         model.CreateTVALink();
    //         var shape = new FoCompound2D(260, 70, "Green")
    //         {
    //             GlyphId = model.ModelId
    //         };
    //         shape.MoveTo(args.OffsetX, args.OffsetY);
    //         model.ApplyExternalMethods(shape);
            
    //         PageManager.Add<FoCompound2D>(shape);

    //         var parmas = new Dictionary<string, object>() {
    //             { "Shape", shape },
    //             { "Model", model },
    //             { "OnOk", () => {

    //             } },
    //             { "OnCancel", () => {
 
    //             } }
    //         };

    //         // Task.Run(async () =>
    //         // {
    //         //     await DialogService!.OpenAsync<TVAConfigure>("Create TVA", parmas, options);
    //         // });

    //     });
     }


    private void SetDoCreateClock()
    {
        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoClock("Clock");
            Add<MoComponent>(model);

            var shape = new FoCompound2D(120, 120, "Cyan")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            PageManager.Add<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateBiometric()
    {
        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoBiometricDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Red")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            PageManager.Add<FoCompound2D>(shape);
        });
    }



    private void SetDoCreatePosition()
    {
        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoPositionDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Green")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            PageManager.Add<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateChat()
    {
        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoChatMessageDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(150, 70, "Blue")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);
            PageManager.Add<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateSystem()
    {
        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoSystemDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Cyan")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);    
            PageManager.Add<FoCompound2D>(shape);
        });
    }

    private void SetDoCreateImage()
    {
        Drawing.SetDoCreate((CanvasMouseArgs args) =>
        {
            var model = new MoImageDataNode();
            Add<MoComponent>(model);

            var shape = new FoCompound2D(130, 70, "Cyan")
            {
                GlyphId = model.ModelId
            };
            shape.MoveTo(args.OffsetX, args.OffsetY);
            model.ApplyExternalMethods(shape);    
            PageManager.Add<FoCompound2D>(shape);
        });
    }

    public MoOutward? ModelConnect(string StartID, string FinishID)
    {
        var Start = FindModel(StartID);
        if ( Start != null)
            $"Start {Start.Name} {Start.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);

        var Finish = FindModel(FinishID);
        if ( Finish != null)
            $"Finish {Finish.Name} {Finish.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);  

        if ( Start != null && Finish != null) {
            var result = Start.Add<MoOutward>(new MoOutward(FinishID, Start, Finish));
            Finish.Add<MoInward>(new MoInward(StartID, Finish, Start));
            Start.Start();
            return result;
        }
        return null;
    }

    public void ModelDisconnect(string StartID, string FinishID)
    {
        var Start = FindModel(StartID);
        if ( Start != null)
            $"Start {Start.Name} {Start.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);

        var Finish = FindModel(FinishID);
        if ( Finish != null)
            $"Finish {Finish.Name} {Finish.GetType().Name}".WriteLine(ConsoleColor.DarkCyan);  

        if ( Start != null && Finish != null) {
            Start.Stop();
            Start.Remove<MoOutward>(FinishID);
            Finish.Remove<MoInward>(StartID);
        }
    }
}
