using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared.SVG;
using Microsoft.AspNetCore.Components;

//  https://learn.microsoft.com/en-us/aspnet/core/blazor/images?view=aspnetcore-7.0
//  https://www.mikesdotnetting.com/article/361/resize-images-before-uploading-in-blazor-web-assembly
namespace FoundryBlazor.Shape;

public class FoSymbol2D : FoShape2D, IShape2D
{

    private double scaleX = 1.0;
    public double ScaleX 
    { 
        get { return this.scaleX; } 
        set { this.scaleX = AssignDouble(value, scaleX); } 
    }


    private double scaleY = 1.0;
    public double ScaleY 
    { 
        get { return this.scaleY; } 
        set { this.scaleY = AssignDouble(value, scaleY); } 
    }

    public double SymbolWidth { get; set; }
    public double SymbolHeight { get; set; }

    private string imageUrl = "";
    public string ImageUrl 
    { 
        get { return this.imageUrl; } 
        set { 
            this.imageUrl = value; 
            waitcount = 0; 
        } 
    }

    public string GetMarkup()
    {
        string SVG = @"<path d='M52,94l-26-19l9-8l-16-10l-17-38l9-17l15,30l26,20l26-20l15-30l9,17l-17,38l-16,10l9,8l-26,19z' fill='#ddd' stroke='#aaa' /> <path d='M52,91l10-7l-10-7l-10,7zM40,83l4-11l-6-5l-9,8zM46,63l-32-23l7,16l12,7zM48,61l3-7l-46-34l7,15zM21,28l-10-22l-6,11zM58,63l32-23l-7,16l-12,7zM56,61l-3-7l46-34l-7,15z M83,28l10-22l6,11zM64,83l-4-11l6-5l9,8z' stroke-linejoin='round' stroke='#222' fill='#138' /> <circle r='5' cx='52' cy='69' stroke='#EEE' fill='#138' />";
        return SVG;
    }


    protected int waitcount = 0;

    public FoSymbol2D() : base()
    {
    }
    public FoSymbol2D(int width, int height, string color) : base("", width, height, color)
    {
    }
    public FoSymbol2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
    }
    
    public override FoDynamicRender GetDynamicRender()
    {
        foDynamicRender ??= new FoDynamicRender(typeof(SVGShape), this);
        return foDynamicRender;
    }

    public override FoSymbol2D ZoomBy(double factor) 
    { 
        ShrinkBy(factor); return this; 
    }
    
    public void ShrinkBy(double factor)
    {
        ScaleX *= factor;
        ScaleY *= factor;
    }

    public override FoGlyph2D ResizeToBox(Rectangle rect)
    {
        var dx = (double)rect.Width / (double)Width;
        var dy = (double)rect.Height / (double)Height;
        base.ResizeToBox(rect);
        ScaleX *= dx;
        ScaleY *= dy;

        //Console.WriteLine($"ScaleX {ScaleX} ScaleY {ScaleY}  {Width} width={rect.Width}, {Height} height={rect.Height}");

        return this;
    }




    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        HoverDraw?.Invoke(ctx, this);
        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);


        await ctx.RestoreAsync();
        return true;
    }


}
