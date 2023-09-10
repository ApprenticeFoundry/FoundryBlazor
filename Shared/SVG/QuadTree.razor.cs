using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shape;

using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.Solutions;
using BlazorComponentBus;
using FoundryBlazor.Canvas;
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

    public QuadTree<FoGlyph2D>? TopLeftChild { get { return TreeNode?.TopLeftChild; } }
    public QuadTree<FoGlyph2D>? TopRightChild { get { return TreeNode?.TopRightChild; } }
    public QuadTree<FoGlyph2D>? BottomLeftChild { get { return TreeNode?.BottomLeftChild; } }
    public QuadTree<FoGlyph2D>? BottomRightChild { get { return TreeNode?.BottomRightChild; } }

}
