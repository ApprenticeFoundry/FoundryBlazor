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

                // var page = Workspace!.GetDrawing().CurrentPage();
                // var state = PanZoom!.ReadFromPage(page);
                // var x = args.X;
                // var y = args.Y;
                // var mtx = state.Matrix;
                // var mtxInv = mtx.Inverse();
                // var pt = new Point(x, y);
                // var ptInv = mtxInv.TransformPoint(pt);
                // var xInv = ptInv.X;
                // var yInv = ptInv.Y;
                // $" {args.Topic} {x} {y} {xInv} {yInv}".WriteNote();
                // StateHasChanged();

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
