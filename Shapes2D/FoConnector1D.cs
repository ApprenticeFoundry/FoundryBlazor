using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoConnector1D : FoShape1D, IGlueOwner, IShape1D
{



    public LineLayoutStyle Layout { get; set; } = LineLayoutStyle.None;





    public Action<Canvas2DContext, FoConnector1D> DrawHorizontalFirst = async (ctx, obj) =>
    {
        //"DrawHorizontalFirst".WriteLine();

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, 0);
        await ctx.LineToAsync(obj.Width, 0);
        await ctx.LineToAsync(obj.Width, obj.Height);
        await ctx.LineToAsync(obj.Width, 0);
        await ctx.LineToAsync(0, 0);
        await ctx.ClosePathAsync();
        await ctx.SetStrokeStyleAsync(obj.Color);
        await ctx.SetLineWidthAsync(obj.Thickness);
        await ctx.StrokeAsync();
    };

    public Action<Canvas2DContext, FoConnector1D> DrawVerticalFirst = async (ctx, obj) =>
    {
        // var angle = (float)obj.ComputeAngle();
        // await ctx.RotateAsync(-angle);

        //"DrawVerticalFirst".WriteLine();

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, 0);
        await ctx.LineToAsync(0, obj.Height);
        await ctx.LineToAsync(obj.Width, obj.Height);
        await ctx.LineToAsync(0, obj.Height);
        await ctx.LineToAsync(0, 0);
        await ctx.ClosePathAsync();
        await ctx.SetStrokeStyleAsync(obj.Color);
        await ctx.SetLineWidthAsync(obj.Thickness);
        await ctx.StrokeAsync();
    };


    public FoConnector1D() : base()
    {
        this.Thickness = 5;
        Smash(false);
    }

    public FoConnector1D(int x1, int y1, int x2, int y2, string color) : base(x1,y1,x2,y2,1, color)
    {
        this.Thickness = 5;
    }


    public FoConnector1D(FoGlyph2D? start, FoGlyph2D? finish, string color) : base(start, finish, 1, color)
    {
        this.Thickness = 5;
    }





  

    
    
 

        //PageManager.Add(new FoConnector1D(300,300, 500, 500, 10, "Red"));
        //PageManager.Add(new FoConnector1D(300,800, 800, 800, 10, "Blue"));

   public override async Task Draw(Canvas2DContext ctx, int tick)
    {
        await ctx.SaveAsync();

        if ( Layout == LineLayoutStyle.HorizontalFirst)
            DrawHorizontalFirst?.Invoke(ctx, this);
        else if ( Layout == LineLayoutStyle.VerticalFirst)
            DrawVerticalFirst.Invoke(ctx, this);
        else
            await DrawStraight(ctx, Color, tick);

        // await ctx.SetStrokeStyleAsync("Yellow");
        // await ctx.SetLineWidthAsync(1);
        // await ctx.StrokeRectAsync(0, 0, Width, Height);
        
        //await DrawTruePin(ctx);
        await ctx.RestoreAsync();
    }


 
}
