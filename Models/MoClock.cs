using FoundryBlazor.Shape;

namespace FoundryBlazor.Model;

public class MoClock : MoComponent
{
    public MoClock(string name) : base(name)
    {
    }

    public override void Start() 
    {
        GetMembers<MoOutward>()?.ForEach(item => item.Start());
    }
    public override void Stop() 
    {
        GetMembers<MoOutward>()?.ForEach(item => item.Stop());
    }
    public override void OnNext(int tick) 
    {
        GetMembers<MoOutward>()?.ForEach(item => item.OnNext(tick));
    }
    public void ApplyExternalMethods(FoCompound2D shape)
    {
        var model = this;
        shape.GlyphId = model.ModelId;
        shape.DrawModelText = async (ctx,  obj) =>  
        {
            var angle = 0.0;
            var pulse = true;

            angle = model.Tick % 360 * Math.PI / 180;
            pulse = model.Tick % 30 == 0;
            

            if ( pulse ) 
            {
                await ctx.SetFillStyleAsync("#000000");
                model.OnNext(model.Tick);
            }
            
            var cx = obj.Width / 2;
            var cy = obj.Height / 2;
            var radius = obj.Width / 3;

            await ctx.BeginPathAsync();
            await ctx.ArcAsync(cx, cy, radius, 0*Math.PI,2*Math.PI);
            await ctx.FillAsync();
            await ctx.StrokeAsync();


            var x1 = radius * Math.Cos(angle) + cx;
            var y1 = radius * Math.Sin(angle) + cy;

            //await ctx.BeginPathAsync();
            await ctx.SetLineWidthAsync(5);
            await ctx.SetStrokeStyleAsync("#000000");
            await ctx.MoveToAsync(cx, cy);
            await ctx.LineToAsync(x1, y1);
            //await ctx.ClosePathAsync();
            await ctx.StrokeAsync();
        };

        shape.ContextLink = (obj,tick) =>
        {
            model.Tick = tick;

            //SetDrawModelText(shape);
        };
    }


}
