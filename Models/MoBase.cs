using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Model;

public interface IMoBase
{
    void Start();
    void Stop();
    void OnNext(int tick);
    void Forward(object message);
    }

public class MoBase : FoBase, IMoBase
{
    
    public string ModelId { get; set;  } = Guid.NewGuid().ToString();
    public MoBase(string name): base(name)
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
}

public class MoPointer : MoBase
{
    public int Counter { get; set; } = 0;
    public MoComponent Target;
    public MoComponent Source;
    public MoPointer(string name, MoComponent source, MoComponent target): base(name)
    {
        Target = target;
        Source = source;
    }

    public override void Start() 
    {
        Target?.Start();
    }
    public override void Stop() 
    {
        Target?.Stop();
    }
    public override void OnNext(int tick) 
    {
        Counter++;
        Target?.OnNext(tick);
    }

    public void SetShapePostDraw(FoShape1D shape) 
    {
        var model = this;
        shape.PostDraw = async (ctx, obj) =>
        {
            await ctx.SetTextAlignAsync(TextAlign.Center);
            await ctx.SetTextBaselineAsync(TextBaseline.Middle);

            //"normal bold 80px sans-serif"
            var TextColor = "#98AFC7";
            var FontSpec = $"normal bold 65px sans-serif";

            await ctx.SetFontAsync(FontSpec);
            await ctx.SetFillStyleAsync(TextColor);
            await ctx.FillTextAsync($"{Counter}", obj.LocPinX(obj), obj.LocPinY(obj));
        };
    }

    public virtual void ApplyExternalMethods(FoShape1D shape)
    {
        var model = this;
        shape.GlyphId = model.ModelId;

        shape.ContextLink = (obj, tick) =>
        {
            SetShapePostDraw(shape);
        };
    }
 
}

public class MoInward : MoPointer
{
    public MoInward(string name, MoComponent source, MoComponent target): base(name,source,target)
    {
    }
}

public class MoOutward : MoPointer
{
    public MoOutward(string name, MoComponent source, MoComponent target): base(name,source,target)
    {
    }
}