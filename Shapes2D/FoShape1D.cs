using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared.SVG;
using FoundryRulesAndUnits.Extensions;
using System.Drawing;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace FoundryBlazor.Shape;


public interface IGlueOwner: IGlyph2D
{
    void AddGlue(FoGlue2D glue);
    void RemoveGlue(FoGlue2D glue);
    void RemoveGlue(string name);
    string GetGlyphId();
    bool Smash(bool force);
}

public class FoShape1D : FoGlyph2D, IGlueOwner, IShape1D
{
    private static int gluecount = 0;



    protected int x1 = 0;
    public int StartX { get { return this.x1; } set { this.x1 = AssignInt(value, x1); } }
    protected int y1 = 0;
    public int StartY { get { return this.y1; } set { this.y1 = AssignInt(value, y1); } }
    protected int x2 = 0;
    public int FinishX { get { return this.x2; } set { this.x2 = AssignInt(value, x2); } }
    protected int y2 = 0;
    public int FinishY { get { return this.y2; } set { this.y2 = AssignInt(value, y2); } }

    private double rotation = 0;
    public float AntiRotation { get { return (float)(-1.0 * this.rotation * Matrix2D.DEG_TO_RAD); } }
    public LineLayoutStyle Layout { get; set; } = LineLayoutStyle.None;
    
    protected Point? startPT;
    protected Point? finishPT;
    public Point Start()
    {
        if (startPT == null)
        {
            var matrix = GetInvMatrix();
            if (matrix == null)
            {
                "Point Start() IMPOSSABLE".WriteError();
                return new Point();
            }

            startPT = matrix.TransformToPoint(StartX, StartY);
        }
        return (Point)startPT;
    }
    public Point Finish()
    {
        if (finishPT == null)
        {
            var matrix = GetInvMatrix();
            if (matrix == null)
            {
                "Point Finish() IMPOSSABLE".WriteError();
                return new Point();
            }
            finishPT = matrix.TransformToPoint(FinishX, FinishY);
        }
        return (Point)finishPT;
    }

    public FoShape1D() : base()
    {
        ShapeDraw = DrawSimpleLine;
        ShapeDrawSelected = DrawDashedLine;
        this.height = 10;
    }
    public FoShape1D(string name, string color) : base(name, color)
    {
        ShapeDraw = DrawSimpleLine;
        ShapeDrawSelected = DrawDashedLine;
        Height = 10;
        Thickness = Height;
    }
    public FoShape1D(int x1, int y1, int x2, int y2, int height, string color) : base("", color)
    {
        ShapeDraw = DrawSimpleLine;
        ShapeDrawSelected = DrawDashedLine;
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        Height = height;
        Thickness = Height;
    }


    public FoShape1D(FoGlyph2D? start, FoGlyph2D? finish, int height, string color) : base("", color)
    {
        ShapeDraw = DrawSimpleLine;
        ShapeDrawSelected = DrawDashedLine;
        this.x1 = start?.PinX ?? 0;
        this.y1 = start?.PinY ?? 0;
        this.x2 = finish?.PinX ?? 0;
        this.y2 = finish?.PinY ?? 0;
        Height = height;
        Thickness = Height;

        GlueStartTo(start);

        GlueFinishTo(finish);
    }

    public override void MoveBy(int dx, int dy) 
    {
        if (HasNoGlue(this))
        {
            (StartX, StartY) = (StartX + dx, StartY + dy);
            (FinishX, FinishY) = (FinishX + dx, FinishY + dy);
        }
    }
    public override FoDynamicRender GetDynamicRender()
    {
        foDynamicRender ??= new FoDynamicRender(typeof(FoShape1D), this);
        return foDynamicRender;
    }
    public (int, int) ComputeLocation(double percent)
    {
        if ( percent <= 0)
            return (StartX, StartY);

        if ( percent >= 1)
            return (FinishX, FinishY);

        var x = (int)(StartX + (FinishX - StartX) * percent);
        var y = (int)(StartY + (FinishY - StartY) * percent);
        return (x, y);
    }
    
    public Action<Canvas2DContext, FoGlyph2D> DrawSimpleLine = async (ctx, obj) =>
    {
        var shape = obj as FoShape1D;
        var start = shape!.Start();
        var finish = shape!.Finish();

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(start.X, start.Y);
        await ctx.LineToAsync(finish.X, finish.Y);
        await ctx.ClosePathAsync();

        await ctx.SetStrokeStyleAsync(obj.Color);
        await ctx.SetLineWidthAsync(obj.Thickness);
        await ctx.StrokeAsync();
    };

    public Action<Canvas2DContext, FoGlyph2D> DrawDashedLine = async (ctx, obj) =>
    {
        var shape = obj as FoShape1D;
        var start = shape!.Start();
        var finish = shape!.Finish();

        await ctx.SetFillStyleAsync("White");
        await ctx.SetLineWidthAsync(10);
        await ctx.SetLineDashAsync(new float[] { 10, 10 });

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(start.X, start.Y);
        await ctx.LineToAsync(finish.X, finish.Y);
        await ctx.ClosePathAsync();
        await ctx.StrokeAsync();
    };

    public override Point[] HitTestSegment()
    {
        //var dx = Math.Abs(x2 - x1);
        //var dy = Math.Abs(y2 - y1);

        //var mat = GetMatrix();
        //var p1 = mat.TransformToPoint(0, 0);
        //var p2 = mat.TransformToPoint(dx, dy);
        //var p1 = mat.TransformToPoint(StartX, StartY);
        //var p2 = mat.TransformToPoint(FinishX, FinishY);
        //var p1 = Start();
        //var p2 = Finish();
        var p1 = new Point(StartX, StartY);
        var p2 = new Point(FinishX, FinishY);
        return [p1, p2];
    }
    
    public override Rectangle HitTestRect()
    {
        var dx = Math.Abs(x2 - x1);
        var dy = Math.Abs(y2 - y1);
        //var x = (x2 + x1) / 2;  //compute PinX in center
        //var y = (y2 + y1) / 2; //compute PinY in center

        var mat = GetMatrix();
        mat.TransformRectangle(0, 0, dx, dy, ref rectangle);
        return rectangle;
    }

    public void RemoveGlue(string name)
    {
        //$"remove glue from {Name} glue {glue.Name}".WriteLine(ConsoleColor.DarkBlue);;
        this.Remove<FoGlue2D>(name);
        this.SmashGlue();
    }

    public override bool Smash(bool force)
    {
        if (!base.Smash(force)) return false;
        if (startPT != null)
        {
            startPT = null;
            //$"Smashing startPT  {Name} {GetType().Name}".WriteInfo(3);
        }
        if (finishPT != null)
        {
            finishPT = null;
            //$"Smashing finishPT  {Name} {GetType().Name}".WriteInfo(3);
        }

        return true;
    }



    public override Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            RecomputeGlue();
            var dx = (double)(x2 - x1);
            var dy = (double)(y2 - y1);
            x = (x2 + x1) / 2;  //compute PinX in center
            y = (y2 + y1) / 2; //compute PinY in center
            width = (int)Math.Sqrt(dx * dx + dy * dy); //compute the length
            rotation = (Math.Atan2(dy, dx) * Matrix2D.RAD_TO_DEG) + RotationZ(this);

            //$"Shape1D GetMatrix {PinX} {PinY} {angle}".WriteError();
            _matrix = Matrix2D.NewMatrix();
            if (_matrix != null)
            {
                _matrix.AppendTransform(this.PinX, this.PinY, 1.0, 1.0, rotation, LocPinX(this), LocPinY(this));
            }
            else
                "GetMatrix Shape1D here is IMPOSSABLE".WriteError();

            OnMatrixRefresh?.Invoke(this);
        }
        return _matrix!;
    }



    public void RecomputeGlue()
    {
        GetMembers<FoGlue2D>()?.ForEach(glue =>
        {
            var (source, target, body) = glue;
            if (source == this && target != null)
            {
                var found = glue.Key[..3] switch
                {
                    "STA" => ComputeStartFor(target),
                    "FIN" => ComputeFinishFor(target),
                    _ => false
                };
            }
        });
    }


    public async Task DrawStart(Canvas2DContext ctx, string color)
    {
        var start = Start();

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.ArcAsync(start.X, start.Y, 26.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);
        var FontSpec = $"normal bold 28px sans-serif";
        await ctx.SetFontAsync(FontSpec);
        await ctx.SetFillStyleAsync("Black");

        await ctx.RotateAsync(AntiRotation);
        await ctx.FillTextAsync("Start", start.X, start.Y);

        await ctx.RestoreAsync();
    }

    public async Task DrawFinish(Canvas2DContext ctx, string color)
    {

        var finish = Finish();

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.ArcAsync(finish.X, finish.Y, 26.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.SetTextAlignAsync(TextAlign.Center);
        await ctx.SetTextBaselineAsync(TextBaseline.Middle);
        await ctx.SetFillStyleAsync("Black");
        var FontSpec = $"normal bold 28px sans-serif";
        await ctx.SetFontAsync(FontSpec);
        await ctx.FillTextAsync("Finish", finish.X, finish.Y);

        await ctx.RestoreAsync();
    }

    public static bool HasNoGlue(FoGlyph2D? target, string child = "")
    {
        if (target == null) return true;
        var part = string.IsNullOrEmpty(child) ? target : target.FindConnectionPoint(child, true) ?? target;

        var total = part.Members<FoGlue2D>().Count;
        return total == 0;
    }

    public FoGlue2D? GlueStartTo(FoGlyph2D? target, string child="")
    {
        if (target == null) return null;

        var part = string.IsNullOrEmpty(child) ? target : target.FindConnectionPoint(child, true) ?? target;
        var name = part != target ? $"START_{part.Key}_{gluecount++}" : $"START_CENTER_{gluecount++}";

        var glue = new FoGlue2D(name);
        glue.GlueTo(this, part, target);
        Smash(false);
        return glue;
    }

    public FoGlue2D? GlueFinishTo(FoGlyph2D? target, string child="")
    {
        if (target == null) return null;
        
        var part = string.IsNullOrEmpty(child) ? target : target.FindConnectionPoint(child, true) ?? target;
        var name = part != target ? $"FINISH_{part.Key}_{gluecount++}" : $"FINISH_CENTER_{gluecount++}";

        var glue = new FoGlue2D(name);
        glue.GlueTo(this, part, target);
        Smash(false);
        return glue;
    }

    public bool ComputeStartFor(FoGlyph2D? target)
    {
        if (target == null) return false;
        var pt = target.AttachTo();
        StartX = pt.X;
        StartY = pt.Y;

        IsSelected = false;
        return true;

    }

        //  $"{Name} ComputeStartFor {target.Name}: {StartX}  {StartY}:  pin {PinX} {PinY}".WriteInfo();


    public bool ComputeFinishFor(FoGlyph2D? target)
    {
        if (target == null) return false;
        var pt = target.AttachTo();
        FinishX = pt.X;
        FinishY = pt.Y;

        IsSelected = false;
        return true;

        // $"{Name} ComputeFinishFor {target.Name}: {FinishX}  {FinishY}:   pin {PinX} {PinY}".WriteInfo();
    }

    public async override Task DrawWhenSelected(Canvas2DContext ctx, int tick, bool deep)
    {
        //"Shape1D DrawWhenSelected".WriteNote();
        await ctx.SaveAsync();
        ShapeDrawSelected?.Invoke(ctx, this);

        //await DrawStart(ctx, "Blue");
        //await DrawFinish(ctx, "Cyan");
        //await DrawPin(ctx);
        await ctx.RestoreAsync();
    }

    public virtual async Task<bool> DrawStraight(Canvas2DContext ctx, string color, int tick)
    {

        var start = Start();
        var finish = Finish();

        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(start.X, start.Y);
        await ctx.LineToAsync(finish.X, finish.Y);
        await ctx.ClosePathAsync();

        await ctx.SetStrokeStyleAsync(color ?? Color);
        await ctx.SetLineWidthAsync(Thickness);
        await ctx.StrokeAsync();
        return true;
    }


    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (CannotRender()) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        if (!IsSelected)
        {
            await Draw(ctx, tick);
            HoverDraw?.Invoke(ctx, this);
        }

        await DrawTag(ctx);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
            RenderDeepDetailed(ctx, tick);


        // if (GetMembers<FoGlue2D>()?.Count > 0)
        //     await DrawTriangle(ctx, "Black");

        // await DrawStraight(ctx, "Red", tick);
        // await DrawStart(ctx, "Blue");
        // await DrawFinish(ctx, "Cyan");

        await ctx.RestoreAsync();
        return true;
    }


}
