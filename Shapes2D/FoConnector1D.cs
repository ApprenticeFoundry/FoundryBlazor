using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoConnector1D : FoGlyph2D, IGlueOwner
{

    private int x1 = 0;
    public int StartX { get { return this.x1; } set { this.x1 = AssignInt(value,x1); } }
    private int y1 = 0;
    public int StartY { get { return this.y1; } set { this.y1 = AssignInt(value,y1); } }
    private int x2 = 0;
    public int FinishX { get { return this.x2; } set { this.x2 = AssignInt(value,x2); } }
    private int y2 = 0;
    public int FinishY { get { return this.y2; } set { this.y2 = AssignInt(value,y2); } }

    public LineLayoutStyle Layout { get; set; } = LineLayoutStyle.None;


    public new Func<FoConnector1D, int> LocPinX = (obj) => 0;
    public new Func<FoConnector1D, int> LocPinY = (obj) => 0;



    public Action<Canvas2DContext, FoConnector1D> DrawStraight = async (ctx, obj) =>
    {
        //"DrawStraight".WriteLine();
        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(0, 0);
        await ctx.LineToAsync(obj.Width, obj.Height);
        await ctx.LineToAsync(0, 0);
        await ctx.ClosePathAsync();
        await ctx.SetStrokeStyleAsync(obj.Color);
        await ctx.SetLineWidthAsync(obj.Thickness);
        await ctx.StrokeAsync();
    };

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
    public override void Smash() 
    {
        width  = Dx();
        height = Dy();
        PinX = StartX;
        PinY = StartY;
        base.Smash();
    }

    public FoConnector1D() : base()
    {
        this.Thickness = 5;
        Smash();
    }

    public FoConnector1D(int x1, int y1, int x2, int y2, string color) : base("", color)
    {
        this.Thickness = 5;
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        Smash();
    }


    public FoConnector1D(FoGlyph2D? start, FoGlyph2D? finish, string color) : base("", color)
    {
        this.Thickness = 5;
        this.x1 = start?.PinX ?? 0;
        this.y1 = start?.PinY ?? 0;
        this.x2 = finish?.PinX ?? 0;
        this.y2 = finish?.PinY ?? 0;

        if ( start != null)
            GlueStartTo(start);

        if ( finish != null)    
            GlueFinishTo(finish);

    }

    public override Rectangle Rect() 
    {
        var loc = PinLocation();
        var pt = GetMatrix().TransformPoint(loc.X, loc.Y);
        var sz = new Size(Height, Height);
        var result = new Rectangle(pt, sz);
        return result;
    }
    public int Dx() => x2 - x1;
    public int Dy() => y2 - y1;

    public int Cx() => (x2 + x1)/2;
    public int Cy() => (y2 + y1)/2;

    public double Distance() { 
        var dx = (double)Dx();
        var dy = (double)Dy();
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double ComputeAngle() { 
        var dx = (double)Dx();
        var dy = (double)Dy();
        return Math.Atan2(dy , dx) * 180 / Math.PI;
    }

    public override List<FoHandle2D> GetHandles() 
    {
        if ( !this.HasSlot<FoHandle2D>()) 
        {
            Add<FoHandle2D>(new FoHandle2D("Begin", LeftX(), CenterY(), "Green"));
            Add<FoHandle2D>(new FoHandle2D("Finish", RightX(), CenterY(), "Green"));
            Add<FoHandle2D>(new FoHandle2D("Top", CenterX(), TopY(), "Green"));
            Add<FoHandle2D>(new FoHandle2D("Bottom", CenterX(), BottomY(), "Green"));
        }
        var result = this.Members<FoHandle2D>();
        return result;
    }

    public override Matrix2D GetMatrix() 
    {
        if (_matrix == null) {
            _matrix = new Matrix2D();
            _matrix.AppendTransform(PinX, PinY, 1.0, 1.0, RotationZ(this), 0.0, 0.0, 0, 0);
            FoGlyph2D.ResetHitTesting = true;
        }
        return _matrix;
    }


    public FoGlue2D? GlueStartTo(FoGlyph2D? target) 
    {
        if ( target == null) return null;

        var glue = new FoGlue2D($"Start_{target.Name}_{Guid.NewGuid()}");
        glue.GlueTo(this, target);
        Smash();
        return glue;
    }
    public FoGlue2D? GlueFinishTo(FoGlyph2D? target) 
    {
        if ( target == null) return null;

        var glue = new FoGlue2D($"Finish_{target.Name}_{Guid.NewGuid()}");
        glue.GlueTo(this, target);
        Smash();
        return glue;
    }

    public void SetStartTo(FoGlyph2D? target) 
    {
        if ( target == null) return;
        StartX = target.PinX;
        StartY = target.PinY;

        IsSelected = false;
        Smash();

        //$"{Name} SetStartTo {target.Name}".WriteLine(ConsoleColor.DarkBlue);
    }

    
    
    public void SetFinishTo(FoGlyph2D? target) 
    {
        if ( target == null) return;
        FinishX = target.PinX;
        FinishY = target.PinY;
        
        IsSelected = false;
        Smash();
          
        //$"{Name} SetFinishTo {target.Name}".WriteLine(ConsoleColor.DarkBlue);
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
            DrawStraight?.Invoke(ctx, this);

        // await ctx.SetStrokeStyleAsync("Yellow");
        // await ctx.SetLineWidthAsync(1);
        // await ctx.StrokeRectAsync(0, 0, Width, Height);
        
        //await DrawTruePin(ctx);
        await ctx.RestoreAsync();
    }

    public async Task DrawTruePin(Canvas2DContext ctx)
    {
        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync("#98AFC7");
        await ctx.ArcAsync(0, 0, 16.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }
 
}
