using System.Drawing;

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
    private int x1 = 0;
    public int StartX { get { return this.x1; } set { this.x1 = AssignInt(value,x1); } }
    private int y1 = 0;
    public int StartY { get { return this.y1; } set { this.y1 = AssignInt(value,y1); } }
    private int x2 = 0;
    public int FinishX { get { return this.x2; } set { this.x2 = AssignInt(value,x2); } }
    private int y2 = 0;
    public int FinishY { get { return this.y2; } set { this.y2 = AssignInt(value,y2); } }

    public override void Smash() 
    {
        Width = (int)Distance();
        PinX = Cx();
        PinY = Cy();
        base.Smash();
    }

    public FoShape1D() : base()
    {
         ShapeDraw = DrawRect;
        this.Height = 10;
        Smash();
    }

    public FoShape1D(int x1, int y1, int x2, int y2, int height, string color) : base("", color)
    {
        ShapeDraw = DrawRect;

        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        this.Height = height;
        Smash();
    }


    public FoShape1D(FoGlyph2D? start, FoGlyph2D? finish, int height, string color) : base("", color)
    {
        ShapeDraw = DrawRect;
        this.x1 = start?.PinX ?? 0;
        this.y1 = start?.PinY ?? 0;
        this.x2 = finish?.PinX ?? 0;
        this.y2 = finish?.PinY ?? 0;
        this.Height = height;

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
            _matrix.AppendTransform(this.PinX, this.PinY, 1.0, 1.0, angle + RotationZ(this), 0.0, 0.0, this.CenterX(), this.CenterY());
            FoGlyph2D.ResetHitTesting = true;
        }
        return _matrix;
    }


    public FoGlue2D? GlueStartTo(FoGlyph2D? target) 
    {
        if ( target == null) return null;

        var glue = new FoGlue2D($"Start_{target.Name}_{Guid.NewGuid().ToString()}");
        glue.GlueTo(this, target);
        Smash();
        return glue;
    }
    public FoGlue2D? GlueFinishTo(FoGlyph2D? target) 
    {
        if ( target == null) return null;

        var glue = new FoGlue2D($"Finish_{target.Name}_{Guid.NewGuid().ToString()}");
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
}
