using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;

using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.Solutions;
using BlazorComponentBus;
 
using System.Drawing;


namespace FoundryBlazor.Shared.SVG;

public class MouseTrackerBase : ComponentBase
{
    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] public IHitTestService? HitTest { get; set; }
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

    public QuadTree<QuadHitTarget> GetTreeNode()
    {
        var tree = HitTest!.GetTree();
        return tree;
    }


    public List<Rectangle> GetSearches()
    {
        var list = new List<Rectangle>();
        if ( HitTest != null)
            list.AddRange(HitTest.GetSearches());
        return list;
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

    public bool IsFenceSelecting()
    {
        return PanZoom?.IsFenceSelecting() ?? false;
    }

    public Rectangle GetFence()
    {
        var fence = BaseInteraction.GetDragArea();
        var rect = PanZoom?.TransformRect(fence) ?? fence;
        return rect;
    }



}
