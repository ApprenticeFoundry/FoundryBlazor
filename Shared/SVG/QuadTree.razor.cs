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

    [Parameter] public QuadTree<FoGlyph2D>? TreeNode { get; set; }

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

    public QuadTree<FoGlyph2D>? TopLeftChild { get => TreeNode?.TopLeftChild;}
    public QuadTree<FoGlyph2D>? TopRightChild { get => TreeNode?.TopRightChild;}
    public QuadTree<FoGlyph2D>? BottomLeftChild { get => TreeNode?.BottomLeftChild;}
    public QuadTree<FoGlyph2D>? BottomRightChild { get => TreeNode?.BottomRightChild;}

}
