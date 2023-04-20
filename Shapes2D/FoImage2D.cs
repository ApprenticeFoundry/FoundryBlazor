using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

//  https://learn.microsoft.com/en-us/aspnet/core/blazor/images?view=aspnetcore-7.0
//  https://www.mikesdotnetting.com/article/361/resize-images-before-uploading-in-blazor-web-assembly
namespace FoundryBlazor.Shape;

public class FoImage2D : FoGlyph2D, IImage2D
{
    public static bool RefreshImages { get; set; } = true;

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

    public double ImageWidth { get; set; }
    public double ImageHeight { get; set; }

    private string imageUrl = "";
    public string ImageUrl 
    { 
        get { return this.imageUrl; } 
        set { 
            this.imageUrl = value; 
            waitcount = 0; 
            FoImage2D.RefreshImages = true; 
        } 
    }

    private ElementReference imageRef;
    public ElementReference ImageRef 
    { 
        get { return this.imageRef; } 
        set { this.imageRef = value; } 
    }

    private int waitcount = 0;

    public FoImage2D() : base()
    {
    }
    public FoImage2D(int width, int height, string color) : base("", width, height, color)
    {
    }
    public FoImage2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
    }
    
    public override FoImage2D ZoomBy(double factor) 
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

    public override List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true)
    {
        list.Add(this);
        return list;
    }

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {

        await ctx.SaveAsync();
        if (!String.IsNullOrEmpty(ImageRef.Id))
        {
            await ctx.ScaleAsync(ScaleX, ScaleY);
            await ctx.DrawImageAsync(ImageRef, 0, 0);
        }
        else if (waitcount > 10)
        {

            await ctx.SetFillStyleAsync("Black");
            await ctx.FillRectAsync(0, 0, Width, Height);

            await ctx.SetTextAlignAsync(TextAlign.Center);
            await ctx.SetTextBaselineAsync(TextBaseline.Middle);

            var TextColor = "#98AFC7";
            var FontSpec = $"normal bold 45px sans-serif";

            await ctx.SetFontAsync(FontSpec);
            await ctx.SetFillStyleAsync(TextColor);
            await ctx.FillTextAsync(ImageUrl, Width / 2, Height / 2);
        }
        else
        {
            waitcount++;
        }
        await ctx.RestoreAsync();

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
