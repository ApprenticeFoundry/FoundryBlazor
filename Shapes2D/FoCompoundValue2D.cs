using System.Reflection;
using Blazor.Extensions.Canvas.Canvas2D;

namespace FoundryBlazor.Shape;

public class FoCompoundValue2D : FoGlyph2D, IShape2D
{
    private readonly PropertyInfo propInfo;
    public string Text { get; set; } = "";
    public string Value { get; set; } = "";
    public string Units { get; set; } = "";
    public string TextColor { get; set; } = "White";
    public int Margin { get; set; } = 2;
    public string FontSize { get; set; } = "20";
    public string Font { get; set; } = "Segoe UI";
    public List<string>? Details { get; set; }

    private bool recompute = true;

    public string NameValue()
    {
        return $"{Text}: {Value} {Units}"; 
    }
    public string FontSizeAndName()
    {
        return $"{FontSize}px {Font}";
    }

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {
        recompute = false;

        await ctx.FillRectAsync(0, 0, Width, Height);
        await ctx.SetFillStyleAsync(TextColor);

        await ctx.SetTextAlignAsync(TextAlign.Left);
        await ctx.SetTextBaselineAsync(TextBaseline.Top);
   
        await ctx.SetFontAsync(FontSizeAndName());

        await ctx.FillTextAsync(NameValue() ,LeftX()+1,TopY()+1);

        //await DrawPin(ctx);
    }

    public async Task ComputeSize(Canvas2DContext ctx)
    {

        var result = await ComputeMeasuredText( ctx, NameValue(), FontSize, Font);
        Height = result.Height + Margin;
        Width = result.Width + Margin;
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
         if ( CannotRender() ) return false;

        await ctx.SaveAsync();
        await ctx.SetFontAsync(FontSizeAndName());

        if ( recompute )
            await ComputeSize(ctx);

        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        if ( !IsSelected )
            HoverDraw?.Invoke(ctx, this);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
        {
            DrawSelected?.Invoke(ctx, this);
            GetHandles()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));    
            //await DrawPin(ctx);
        }


        await ctx.RestoreAsync();
        return true;
    }


   public void UpdateValue(object source)
    {
        if ( propInfo == null) {
            Value = "Null Prop";
            return;
        }
        var result = propInfo.GetValue(source, null);

        if ( result == null) {
            Value = "Null Result";
            return;
        }

        Value = result.ToString() ?? "NAN";
    }

    public FoCompoundValue2D(PropertyInfo prop) : base(prop.Name, "Orange")
    {
        propInfo = prop;
        ResetLocalPin((obj) => 0, (obj) => 0);
    }



}
