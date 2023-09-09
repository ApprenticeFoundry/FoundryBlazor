using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;

using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.Solutions;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
using System.Drawing;


namespace FoundryBlazor.Shared.SVG;

public class MouseTrackerBase : ComponentBase
{
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] public IPanZoomService? PanZoom { get; set; } 
    [Inject] private ComponentBus? PubSub { get; set; }
    

    Rectangle dragArea = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        PubSub!.SubscribeTo<CanvasMouseArgs>(args =>
        {
            try
            {
                dragArea = PanZoom!.HitRectStart(args);
                InvokeAsync(StateHasChanged);

            }
            catch (Exception ex)
            {
                $"HELP {args.Topic} {ex.Message}".WriteNote();
            }
        });
    }

    public PanZoomState GetPanZoomState()
    {
        var page = Workspace!.GetDrawing().CurrentPage();
        var state = PanZoom!.ReadFromPage(page);
        return state;
    }

    public int X1()
    {
        //var state = GetPanZoomState();
        return dragArea.X;  //state.LastLocation.X;
    }
    public int Y1()
    {
        //var state = GetPanZoomState();
        return dragArea.Y;  //state.LastLocation.Y;
    }





}
