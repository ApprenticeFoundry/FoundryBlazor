using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Model;


public class MoComponent : FoComponent, IMoBase
{
    
    public int Tick { get; set; } = 0;
    public string ModelId { get; set;  } = Guid.NewGuid().ToString();
    public MoComponent(string name):base(name)
    {
    }

    public virtual void Start() 
    {
    }
    public virtual void Stop() 
    {
    }
    public virtual void OnNext(int tick) 
    {
    }

    public virtual void Forward(object message) 
    {
    }

    public void SetDrawModelText(FoCompound2D shape) 
    {
        shape.DrawModelText = async (ctx, obj) =>
        {
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);

            //"normal bold 80px sans-serif"
            var TextColor = "#98AFC7";
            var FontSpec = $"normal bold 65px sans-serif";

            await ctx.SetFontAsync(FontSpec);
            await ctx.SetFillStyleAsync(TextColor);
            await ctx.FillTextAsync(Name, obj.LeftX() + 1, obj.TopY() + 1);
        };
    }
}
