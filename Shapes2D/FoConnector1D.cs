using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoConnector1D : FoShape1D, IShape1D
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

        // "DrawVerticalFirst".WriteInfo();

        var width = obj.FinishX - obj.StartX;
        var height = obj.FinishY - obj.StartY;

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, 0);
        await ctx.LineToAsync(0, height);
        await ctx.LineToAsync(width, height);
        await ctx.LineToAsync(0, height);
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

    public FoConnector1D(int x1, int y1, int x2, int y2, string color) : base(x1, y1, x2, y2, 1, color)
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

        if (Layout == LineLayoutStyle.HorizontalFirst)
            DrawHorizontalFirst?.Invoke(ctx, this);
        else if (Layout == LineLayoutStyle.VerticalFirst)
            DrawVerticalFirst.Invoke(ctx, this);
        else
            await DrawStraight(ctx, Color, tick);

        // await ctx.SetStrokeStyleAsync("Yellow");
        // await ctx.SetLineWidthAsync(1);
        // await ctx.StrokeRectAsync(0, 0, Width, Height);

        //await DrawTruePin(ctx);
        await ctx.RestoreAsync();
    }

    public override Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            RecomputeGlue();
            // var dx = (double)(x2 - x1);
            // var dy = (double)(y2 - y1);
            x = (x2 + x1) / 2;  //compute PinX in center
            y = (y2 + y1) / 2; //compute PinY in center
            // width = (int)Math.Sqrt(dx * dx + dy * dy); //compute the length
            // rotation = 0;

            //$"Shape1D GetMatrix {PinX} {PinY} {angle}".WriteError();
            _matrix = Matrix2D.NewMatrix();
            if (_matrix != null)
            {
                _matrix.AppendTransform(x1, y1, 1.0, 1.0, 0, 0.0, 0.0, LocPinX(this), LocPinY(this));
            }
            else
                "GetMatrix Shape1D here is IMPOSSABLE".WriteError();

            //FoGlyph2D.ResetHitTesting = true;
            //$"GetMatrix  {Name}".WriteLine(ConsoleColor.DarkBlue);
        }
        return _matrix!;
    }


}
