using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;

using FoundryRulesAndUnits.Extensions;
using System.Linq;
using FoundryBlazor.Solutions;
using BlazorComponentBus;
 
using System.Drawing;


namespace FoundryBlazor.Shared.SVG;

public class QuadTreeBase : ComponentBase
{

    [Parameter] public QuadTree<QuadHitTarget>? TreeNode { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

    }

    public Rectangle Rect()
    {
        return TreeNode?.QuadRect ?? new Rectangle(0, 0, 0, 0);
    }

    protected List<Rectangle> HitRectangles()
    {
        return TreeNode?.HitRectangles() ?? new List<Rectangle>();
    }

    public QuadTree<QuadHitTarget>? TopLeftChild { get => TreeNode?.TopLeftChild;}
    public QuadTree<QuadHitTarget>? TopRightChild { get => TreeNode?.TopRightChild;}
    public QuadTree<QuadHitTarget>? BottomLeftChild { get => TreeNode?.BottomLeftChild;}
    public QuadTree<QuadHitTarget>? BottomRightChild { get => TreeNode?.BottomRightChild;}

}
