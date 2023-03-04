using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public interface IGlueOwner
{
    void AddGlue(FoGlue2D glue);
    void RemoveGlue(FoGlue2D glue);
    public void SetFinishTo(FoGlyph2D? target);
    public void SetStartTo(FoGlyph2D? target);
}

public class FoShape1D : FoGlyph2D, IGlueOwner, IShape1D
{
    protected int x1 = 0;
    public int StartX { get { return this.x1; } set { this.x1 = AssignInt(value,x1); } }
    protected int y1 = 0;
    public int StartY { get { return this.y1; } set { this.y1 = AssignInt(value,y1); } }
    protected int x2 = 0;
    public int FinishX { get { return this.x2; } set { this.x2 = AssignInt(value,x2); } }
    protected int y2 = 0;
    public int FinishY { get { return this.y2; } set { this.y2 = AssignInt(value,y2); } }

    //prevent smashing matrix when height and width are calculated
    // public new int Height { get { return this.height; } set { this.height = value; } }
    // public new int Width { get { return this.width; } set { this.width = value; } }

    public override bool Smash() 
    {
        if ( !base.Smash() ) return false;

        //SRS  where and when does this get calculated...
        width = (int)Distance();
        x = Cx();
        y = Cy();
        return true;
    }

    public FoShape1D() : base()
    {
         ShapeDraw = DrawRect;
        this.height = 10;
    }

    public FoShape1D(int x1, int y1, int x2, int y2, int height, string color) : base("", color)
    {
        ShapeDraw = DrawRect;

        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        this.height = height;
    }


    public FoShape1D(FoGlyph2D? start, FoGlyph2D? finish, int height, string color) : base("", color)
    {
        ShapeDraw = DrawRect;
        this.x1 = start?.PinX ?? 0;
        this.y1 = start?.PinY ?? 0;
        this.x2 = finish?.PinX ?? 0;
        this.y2 = finish?.PinY ?? 0;
        this.height = height;

        if ( start != null)
            GlueStartTo(start);

        if ( finish != null)    
            GlueFinishTo(finish);

    }

    public override Rectangle Rect() 
    {
        var d = Height / 2;
        var loc = PinLocation();
        var pt = GetMatrix().TransformPoint(loc.X-d, loc.Y-d);
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
            var angle = ComputeAngle();
            if ( _matrix != null)
                _matrix.AppendTransform(this.PinX, this.PinY, 1.0, 1.0, angle + RotationZ(this), 0.0, 0.0, this.CenterX(), this.CenterY());
            else
                "Smahing here is IMPOSSABLE".WriteError();

            //FoGlyph2D.ResetHitTesting = true;
            //$"GetMatrix  {Name}".WriteLine(ConsoleColor.DarkBlue);
        }
        return _matrix!;
    }

    public async Task DrawStart(Canvas2DContext ctx, string color)
    {
        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.ArcAsync(StartX, StartY, 26.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }
      
    public async Task DrawFinish(Canvas2DContext ctx, string color)
    {
        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.ArcAsync(FinishX, FinishY, 26.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
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
        var pt = target.AttachTo();
        StartX = pt.X;
        StartY = pt.Y;

        IsSelected = false;
        Smash();

        //$"{Name} SetStartTo {target.Name}".WriteLine(ConsoleColor.DarkBlue);
    }

    
    
    public void SetFinishTo(FoGlyph2D? target) 
    {
        if ( target == null) return;
        var pt = target.AttachTo();
        FinishX = pt.X;
        FinishY = pt.Y;
        
        IsSelected = false;
        Smash();
          
        //$"{Name} SetFinishTo {target.Name}".WriteLine(ConsoleColor.DarkBlue);
    }

    public async Task<bool> DrawStraight(Canvas2DContext ctx, int tick)
    {
        //"DrawStraight".WriteLine();
        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(StartX, StartY);
        await ctx.LineToAsync(FinishX, FinishY);
        await ctx.ClosePathAsync();

        await ctx.SetStrokeStyleAsync(Color);
        await ctx.SetLineWidthAsync(Thickness);
        await ctx.StrokeAsync();
        return true;
    }

    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (CannotRender()) return false;

        width = (int)Distance();
        x = Cx();
        y = Cy();

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        //await Draw(ctx, tick);
        await DrawStraight(ctx, tick);
        
        if (!IsSelected)
            HoverDraw?.Invoke(ctx, this);

        await DrawTag(ctx);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await RenderAsSelected(ctx, tick, deep);
        
        if (deep)
        {
            GetMembers<FoShape1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        }

        if (GetMembers<FoGlue2D>()?.Count > 0)
            await DrawTriangle(ctx, "Black");
        

        await ctx.RestoreAsync();
        return true;
    }
}
