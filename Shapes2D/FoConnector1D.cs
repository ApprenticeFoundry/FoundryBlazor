using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;

public class FoConnector1D : FoShape1D, IShape1D
{

    public Action<Canvas2DContext, FoConnector1D> DrawHorizontalFirst = async (ctx, obj) =>
    {
        //"DrawHorizontalFirst".WriteLine();

        var width = obj.FinishX - obj.StartX;
        var height = obj.FinishY - obj.StartY;

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, 0);
        await ctx.LineToAsync(width, 0);
        await ctx.LineToAsync(width, height);

        await ctx.SetStrokeStyleAsync(obj.Color);
        await ctx.SetLineWidthAsync(obj.Thickness);
        await ctx.StrokeAsync();
    };

    public Action<Canvas2DContext, FoConnector1D> DrawVerticalFirst = async (ctx, obj) =>
    {
        // var angle = (float)obj.ComputeAngle();
        // await ctx.RotateAsync(-angle);

        //"DrawVerticalFirst".WriteInfo();

        var width = obj.FinishX - obj.StartX;
        var height = obj.FinishY - obj.StartY;

        //$"DrawVerticalFirst {width} {height}".WriteInfo();

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, 0);
        await ctx.LineToAsync(0, height);
        await ctx.LineToAsync(width, height);

        await ctx.SetStrokeStyleAsync(obj.Color);
        await ctx.SetLineWidthAsync(obj.Thickness);
        await ctx.StrokeAsync();
    };


    private void SetupDrawSelected()
    {
        ShapeDrawSelected = async (ctx, obj) =>
        {
            var color = this.Color;
            var thick = this.Thickness;

           // this.Color = "Yellow";
            this.Thickness = 10;
            await ctx.SetLineDashAsync(new float[] { 10, 10 });
            await Draw(ctx, 0);

            this.Color = color;
            this.Thickness = thick;
        };
    }

    public FoConnector1D() : base()
    {
        this.Thickness = 5;
        Smash(false);
        SetupDrawSelected();
    }

    public FoConnector1D(int x1, int y1, int x2, int y2, string color) : base(x1, y1, x2, y2, 1, color)
    {
        this.Thickness = 5;
         SetupDrawSelected();
    }


    public FoConnector1D(FoGlyph2D? start, FoGlyph2D? finish, string color) : base(start, finish, 1, color)
    {
        this.Thickness = 5;
         SetupDrawSelected();
    }

    public override Point[] HitTestSegment()
    {
        var dx = Math.Abs(x2 - x1);
        var dy = Math.Abs(y2 - y1);
        
        var mat = GetMatrix();
        var p1 = mat.TransformToPoint(0, 0);
        var p4 = mat.TransformToPoint(dx, dy);

        switch (Layout)
        {
            case LineLayoutStyle.HorizontalFirst:
                {
                    var p2 = mat.TransformToPoint(dx, 0);
                    return new Point[] { p1, p2, p4 };
                }

            case LineLayoutStyle.VerticalFirst:
                {
                    var p3 = mat.TransformToPoint(0, dy);
                    return new Point[] { p1, p3, p4 };
                }
        }
        return new Point[] { p1, p4 };
    }

    public override async Task Draw(Canvas2DContext ctx, int tick)
    {
        await ctx.SaveAsync();

        switch (Layout)
        {
            case LineLayoutStyle.HorizontalFirst:
                DrawHorizontalFirst?.Invoke(ctx, this);
                break;
            case LineLayoutStyle.VerticalFirst:
                DrawVerticalFirst.Invoke(ctx, this);
                break;
            case LineLayoutStyle.Straight:
            case LineLayoutStyle.None:
                await DrawStraight(ctx, Color, tick);
                break;
        }

        //await DrawTruePin(ctx);
        await ctx.RestoreAsync();
    }

    public override Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            RecomputeGlue();

            x = (x2 + x1) / 2;  //compute PinX in center
            y = (y2 + y1) / 2; //compute PinY in center

            //$"FoConnector1D GetMatrix {PinX} {PinY} {angle}".WriteError();
            _matrix = Matrix2D.NewMatrix();
            if (_matrix != null)
            {
                _matrix.AppendTransform(x1, y1, 1.0, 1.0, 0, LocPinX(this), LocPinY(this));
            }
            else
                "GetMatrix FoConnector1D here is IMPOSSABLE".WriteError();

            //FoGlyph2D.ResetHitTesting = true;
            //$"GetMatrix  {Name}".WriteLine(ConsoleColor.DarkBlue);
            OnMatrixRefresh?.Invoke(this);
        }
        return _matrix!;
    }


}
