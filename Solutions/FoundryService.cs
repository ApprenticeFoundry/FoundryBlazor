using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public interface IFoundryService
{
    ICommand Command();
    DialogService Dialog();
    IJSRuntime JS();
    ComponentBus PubSub();
    IToast Toast();
    IDrawing Drawing();
    IArena Arena();
    IPanZoomService PanZoom();
    ISelectionService Selection();
}

public class FoundryService : IFoundryService
{
    protected IToast toast { get; set; }
    protected ICommand cmd { get; set; }
    protected IDrawing drawing { get; set; }
    protected IArena arena { get; set; }
    protected IPanZoomService panzoom { get; set; }
    protected ISelectionService selection { get; set; }
    protected DialogService dialog { get; set; }
    protected IJSRuntime js { get; set; }
    protected ComponentBus pubsub { get; set; }

    public FoundryService(
        IToast toast,
        ICommand command,
        ISelectionService selection,
        IPanZoomService panzoom,
        IDrawing drawing,
        IArena arena,
        DialogService dialog,
        IJSRuntime js,
        ComponentBus pubsub)
    {
        this.cmd = command;
        this.dialog = dialog;
        this.js = js;
        this.pubsub = pubsub;
        this.toast = toast;
        this.drawing = drawing;
        this.arena = arena;
        this.panzoom = panzoom;
        this.selection = selection;
    }


    public ICommand Command()
    {
        return cmd;
    }

    public DialogService Dialog()
    {
        return dialog;
    }

    public IJSRuntime JS()
    {
        return js;
    }

    public ComponentBus PubSub()
    {
        return pubsub;
    }

    public IToast Toast()
    {
        return toast;
    }

    public IDrawing Drawing()
    {
        return drawing;
    }

    public IArena Arena()
    {
        return arena;
    }

    public IPanZoomService PanZoom()
    {
        return panzoom;
    }

    public ISelectionService Selection()
    {
        return selection;
    }
}
