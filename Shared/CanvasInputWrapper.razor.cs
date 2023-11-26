using Microsoft.AspNetCore.Components;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components.Web;
using BlazorComponentBus;

using FoundryBlazor.Solutions;
using FoundryBlazor.Shared;
using System.Text;

namespace FoundryBlazor.Shared;

public class CanvasInputWrapperBase : ComponentBase, IAsyncDisposable, IDisposable
{

    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string CanvasStyle { get; set; } = "width:max-content; border:1px solid black; cursor: default";

    [Parameter] public int CanvasWidth { get; set; } = 1800;
    [Parameter] public int CanvasHeight { get; set; } = 1200;



    public string GetCanvasStyle()
    {
        //var cursor = Workspace!.GetDrawing().Tools().GetCursor();
        var style = new StringBuilder(CanvasStyle).Append("; ")
                    //.Append("cursor:").Append(cursor).Append("; ")
                    .Append("width:")
                    .Append(CanvasWidth).Append("px; ")
                    .Append("height:")
                    .Append(CanvasHeight).Append("px; ").ToString();
        return style;
    }
    public ValueTask DisposeAsync()
    {
        $"InputWrapperBase DisposeAsync".WriteInfo();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        $"InputWrapperBase Dispose".WriteInfo();
        GC.SuppressFinalize(this);
    }

    protected void OnMouseDown(MouseEventArgs args)
    {
        //$"On Mouse Down {args.ClientX}, {args.ClientY}".WriteInfo();

        var canvasArgs = ToCanvasMouseArgs(args, "ON_MOUSE_DOWN");
        PubSub?.Publish<CanvasMouseArgs>(canvasArgs);
    }


    protected void OnMouseUp(MouseEventArgs args)
    {
        var canvasArgs = ToCanvasMouseArgs(args, "ON_MOUSE_UP");
        PubSub?.Publish<CanvasMouseArgs>(canvasArgs);
    }

    protected void OnWheelMove(WheelEventArgs args)
    {
        var canvasArgs = ToCanvasWheelChangeArgs(args, "ON_WHEEL_CHANGE");
        PubSub?.Publish<CanvasWheelChangeArgs>(canvasArgs);
    }


    protected void OnMouseMove(MouseEventArgs args)
    {
        var canvasArgs = ToCanvasMouseArgs(args, "ON_MOUSE_MOVE");
        PubSub?.Publish<CanvasMouseArgs>(canvasArgs);
    }

    protected void OnKeyDown(KeyboardEventArgs args)
    {
        var canvasArgs = ToCanvasKeyboardArgs(args, "ON_KEY_DOWN");
        PubSub?.Publish<CanvasKeyboardEventArgs>(canvasArgs);
    }
    protected void OnKeyUp(KeyboardEventArgs args)
    {
        var canvasArgs = ToCanvasKeyboardArgs(args, "ON_KEY_UP");
        PubSub?.Publish<CanvasKeyboardEventArgs>(canvasArgs);

    }
    protected void OnKeyPress(KeyboardEventArgs args)
    {
        var canvasArgs = ToCanvasKeyboardArgs(args, "ON_KEY_PRESS");
        PubSub?.Publish<CanvasKeyboardEventArgs>(canvasArgs);
    }

    private static CanvasMouseArgs ToCanvasMouseArgs(MouseEventArgs args, string topic)
    {
        var canvasArgs = new CanvasMouseArgs
        {
            ScreenX = (int)args.ScreenX,
            ScreenY = (int)args.ScreenY,
            ClientX = (int)args.ClientX,
            ClientY = (int)args.ClientY,
            MovementX = (int)args.MovementX,
            MovementY = (int)args.MovementY,
            OffsetX = (int)args.OffsetX,
            OffsetY = (int)args.OffsetY,
            AltKey = args.AltKey,
            CtrlKey = args.CtrlKey,
            MetaKey = args.MetaKey,
            ShiftKey = args.ShiftKey,
            Button = (int)args.Button,
            Buttons = (int)args.Buttons,
            Topic = topic
        };
        return canvasArgs;
    }

    private static CanvasWheelChangeArgs ToCanvasWheelChangeArgs(WheelEventArgs args, string topic)
    {
        var canvasArgs = new CanvasWheelChangeArgs
        {
            DeltaX = args.DeltaX,
            DeltaY = args.DeltaY,
            DeltaZ = args.DeltaZ,
            DeltaMode = (int)args.DeltaMode,
            Topic = topic
        };
        return canvasArgs;
    }

    private static CanvasKeyboardEventArgs ToCanvasKeyboardArgs(KeyboardEventArgs args, string topic)
    {
        var canvasArgs = new CanvasKeyboardEventArgs
        {
            AltKey = args.AltKey,
            CtrlKey = args.CtrlKey,
            MetaKey = args.MetaKey,
            ShiftKey = args.ShiftKey,
            Code = args.Code,
            Key = args.Key,
            Repeat = args.Repeat,
            Location = (int)args.Location,
            Topic = topic
        };
        return canvasArgs;
    }




}
