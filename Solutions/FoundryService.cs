using BlazorComponentBus;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;
using Microsoft.JSInterop;
using Radzen;

namespace FoundryBlazor.Solutions;

public class AnimationEvent
{
    public double fps = 0;
}

public interface IFoundryService
{
    ICommand Command();
    IUnitSystem UnitSystem();
    IPopupDialog PopupDialog();
    IJSRuntime JS();
    ComponentBus PubSub();
    IToast Toast();
    IDrawing Drawing();
    IArena Arena();
    IPanZoomService PanZoom();
    ISelectionService Selection();
    IHitTestService HitTest();
    IQRCodeService QRCode();
    IToolManagement Tools();
    IWorldManager WorldManager();
    ComponentBus AnimationBus();
}

public class FoundryService : IFoundryService
{
    protected IToast toast { get; set; }
    protected ICommand cmd { get; set; }
    protected IUnitSystem units { get; set; }
    protected IDrawing drawing { get; set; }
    protected IArena arena { get; set; }
    protected IWorldManager manager { get; set; }
    protected IPanZoomService panzoom { get; set; }
    protected ISelectionService selection { get; set; }
    protected IHitTestService hittest { get; set; }
    protected IQRCodeService qrcode { get; set; }
    protected IPopupDialog dialog { get; set; }
    protected IJSRuntime js { get; set; }
    protected ComponentBus pubsub { get; set; }

    private static ComponentBus Publish { get; set; } = null!;
    private static DateTime _lastRender;

    public FoundryService(
        IToast toast,
        ICommand command,
        IUnitSystem units,
        ISelectionService selection,
        IHitTestService hittest,
        IQRCodeService qrcode,
        IPanZoomService panzoom,
        IDrawing drawing,
        IArena arena,
        IWorldManager manager,
        IPopupDialog dialog,
        IJSRuntime js,
        ComponentBus pubsub)
    {
        this.cmd = command;
        this.units = units;
        this.dialog = dialog;
        this.js = js;
        this.pubsub = pubsub;
        this.toast = toast;
        this.drawing = drawing;
        this.arena = arena;
        this.manager = manager;
        this.panzoom = panzoom;
        this.selection = selection;
        this.hittest = hittest;
        this.qrcode = qrcode;

        Publish = new ComponentBus();
        _lastRender = DateTime.Now;
    }

    [JSInvokable]
    public static async void TriggerAnimationFrame()
    {
        if (Publish != null)
        {
            var framerate = 1.0 / (DateTime.Now - _lastRender).TotalSeconds;
            _lastRender = DateTime.Now; // update for the next time 
            //$"TriggerAnimationFrame  {framerate}".WriteSuccess();
            await Publish.Publish<AnimationEvent>(new AnimationEvent() { fps = framerate });
        }
    }

    public ComponentBus AnimationBus()
    {
        return Publish;
    }

    public ICommand Command()
    {
        return cmd;
    }

    public IPopupDialog PopupDialog()
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
    public IHitTestService HitTest()
    {
        return hittest;
    }
    public ISelectionService Selection()
    {
        return selection;
    }

    public IQRCodeService QRCode()
    {
        return qrcode;
    }

    public IToolManagement Tools()
    {
        return Drawing().Tools();
    }

    public IWorldManager WorldManager()
    {
        return manager;
    }
}
