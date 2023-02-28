using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;

using Unglide;

namespace FoundryBlazor.Shape;

public enum BoxLayoutStyle
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    HorizontalStacked = 3,
    VerticalStacked = 4
}

public enum LineLayoutStyle
{
    None = 0,
    Straight = 1,
    VerticalFirst = 2,
    HorizontalFirst = 3,
}

public interface IHasRectangle
{
    Rectangle Rect();
}

public interface IRender
{
    public Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region);
    public Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true);
}

public interface IShape1D
{

}

public interface IShape2D
{
    
}



public class MeasuredText
{
    public string Text="";
    public int Width=0;
    public int Height=0;
    public bool Failure=false;

}



public class FoGlyph2D : FoComponent, IHasRectangle, IRender
{
    public static Tweener Animations { get; set; } = new Tweener();
    public static bool ResetHitTesting { get; set; } = false;
    public float Thickness { get; set; } = 1;
    public bool IsSelected { get; set; } = false;
    public bool IsVisible { get; set; } = true;
    public bool ShouldRender { get; set; } = true;
    public string Tag { get; set; } = "";
    public string GlyphId { get; set; } = Guid.NewGuid().ToString();
    private int x = 0;
    public int PinX { get { return this.x; } set { this.x = AssignInt(value, x); } }
    private int y = 0;
    public int PinY { get { return this.y; } set { this.y = AssignInt(value, y); } }
    protected int width = 0;
    public int Width { get { return this.width; } set { this.width = AssignInt(value, width); } }
    protected int height = 0;
    public int Height { get { return this.height; } set { this.height = AssignInt(value, height); } }

    private double angle = 0;
    public double Angle { get { return this.angle; } set { this.angle = AssignDouble(value, angle); } }
    public float Opacity { get; set; } = 1.0F;

    public string Color { get; set; }

    public Func<FoGlyph2D, int> LocPinX = (obj) => obj.Width / 2;
    public Func<FoGlyph2D, int> LocPinY = (obj) => obj.Height / 2;
    public Func<FoGlyph2D, double> RotationZ = (obj) => obj.Angle;

    public int LeftEdge() { return PinX - LocPinX(this); }
    public int TopEdge() { return PinY - LocPinY(this); }
    public int BottomEdge() { return TopEdge() + Height; }
    public int RightEdge() { return  LeftEdge() + Width; }


    public Action<FoGlyph2D, int>? ContextLink;
    public Action<Canvas2DContext, FoGlyph2D>? PreDraw;
    public Action<Canvas2DContext, FoGlyph2D>? HoverDraw;
    public Action<Canvas2DContext, FoGlyph2D>? PostDraw;
    public Action<Canvas2DContext, FoGlyph2D>? ShapeDraw;

    public Action<FoGlyph2D>? DoOnOpenCreate;
    public Action<FoGlyph2D>? DoOnOpenEdit;

    protected Matrix2D? _matrix;
    protected Matrix2D? _invMatrix;

    public FoGlyph2D() : base("")
    {
        PinX = PinY = 0;
        Width = Height = 0;
        Color = "Green";
        ShapeDraw = DrawRect;
    }
    public FoGlyph2D(string name, string color) : base(name)
    {
        PinX = PinY = 0;
        Width = Height = 0;
        Color = color;
        ShapeDraw = DrawRect;
    }

    public FoGlyph2D(string name, int width, int height, string color) : base(name)
    {
        PinX = PinY = 0;
        (Width, Height, Color) = (width, height, color);
        ShapeDraw = DrawRect;
    }

    public static async Task<MeasuredText> ComputeMeasuredText(Canvas2DContext ctx, string Fragment,string FontSize, string Font)
    {

        int count = 0;
        var FontSpec = $"{FontSize}px {Font}";

        var result = new MeasuredText()
        {
            Failure = true,
            Text = Fragment,
            Height = int.Parse(FontSize)
        };

        await ctx.SaveAsync();
        do
        {
            count++;
            await ctx.SetFontAsync(FontSpec);
            var textMetrics = await ctx.MeasureTextAsync(Fragment);
            result.Failure = !FontSpec.Matches(ctx.Font);
            var color = result.Failure ? ConsoleColor.Red : ConsoleColor.Green;

            result.Width = (int)textMetrics.Width;
            $"FoText2D ComputeSize {count} {Fragment} {result.Width} {result.Height}  FONT:{ctx.Font}  SPEC:{FontSpec}".WriteLine(color);
        }
        while (result.Failure && count < 3);

        await ctx.RestoreAsync();

        return result;
    }

    public Point ResetLocalPin(Func<FoGlyph2D, int> locX, Func<FoGlyph2D, int> locY)
    {
        LocPinX = locX;
        LocPinY = locY;
        Smash();
        return PinLocation();
    }

    public virtual List<T> CollectMembers<T>(List<T> list, bool deep=true) where T: FoGlyph2D
    {
        var members = GetMembers<T>();
        if ( members != null)
            list.AddRange(members);

        return list;
    }

    public virtual FoGlyph2D MarkSelected(bool value)
    {
        this.IsSelected = value;
        return this;
    }

    public virtual bool LocalMouseHover(CanvasMouseArgs args, Action<Canvas2DContext, FoGlyph2D>? OnHover)
    {
        return false;
    }

    public virtual Rectangle Rect()
    {
        var pt = GetMatrix().TransformPoint(0, 0);
        var sz = new Size(Width, Height);
        var result = new Rectangle(pt, sz);
        return result;
    }

    public virtual FoGlyph2D ResizeTo(int width, int height) { (Width, Height) = (width, height); return this; }
    public void ResizeBy(int dx, int dy) => (Width, Height) = (Width + dx, Height + dy);
    public FoGlyph2D MoveTo(int x, int y) { (PinX, PinY) = (x, y); return this; }
    public void MoveBy(int dx, int dy) => (PinX, PinY) = (PinX + dx, PinY + dy);
    public void RotateBy(double da) => Angle += da;
    public virtual FoGlyph2D ZoomBy(double factor) { return this; }
    public FoGlyph2D RotateTo(double a) { Angle = a; return this; }
    public virtual Point PinLocation()
    {
        return new(LocPinX(this), LocPinY(this));
    }

    public int FractionX(double fraction) => (int)(fraction * Width);
    public int FractionY(double fraction) => (int)(fraction * Height);
    public int CenterX() => FractionX(0.5);
    public int LeftX() => 0;
    public int RightX() => Width;
    public int CenterY() => FractionY(0.5);
    public int TopY() => 0;
    public int BottomY() => Height;

   public virtual FoGlyph2D ResizeToBox(Rectangle rect) 
   {
        var dx = LocPinX(this);
        var dy = LocPinY(this);
        Width = Math.Abs(rect.Width);
        Height = Math.Abs(rect.Height);

        dx = dx > 0 ? dx : 0;
        dy = dy > 0 ? dy : 0;
        PinX += LocPinX(this) - dx; 
        PinY += LocPinY(this) - dy;
        return this; 
    }

    public Tween AnimatedMoveTo(int x, int y, float duration=2.0F, float delay=0) 
    { 
        return Animations.Tween(this, new { PinX = x, PinY = y }, duration, delay).Ease(Ease.ElasticInOut);
    }

    public Tween AnimatedRotateTo(double angle, float duration=2.0F, float delay=0) 
    {
        return Animations.Tween(this, new { Angle = angle }, duration, delay);
    }
     public Tween AnimatedResizeTo(int w, int h, float duration=2.0F, float delay=0) 
    {
        return Animations.Tween(this, new { Width = w, Height = h }, duration, delay).Ease(Ease.ElasticInOut);
    }   
     public Tween AnimatedGrowFromZero(float duration=2.0F) 
    {
        var w = Width;
        var h = Height;
        Width = 0;
        Height = 0;
        return Animations.Tween(this, new { Width = w, Height = h }, duration).Ease(Ease.ElasticInOut);
    } 
    public virtual async Task UpdateContext(Canvas2DContext ctx, int tick)
    {
        ContextLink?.Invoke(this, tick);

        
        var mtx = this.GetMatrix();
        //you must use Transform so the context can acumlate the positions
        await ctx.TransformAsync(mtx.a, mtx.b, mtx.c, mtx.d, mtx.tx, mtx.ty);

        await ctx.SetGlobalAlphaAsync(ctx.GlobalAlpha * this.Opacity);

        await ctx.SetFillStyleAsync(Color);
    }


    public virtual List<FoHandle2D>? GetHandles()
    {
        if (!this.HasSlot<FoHandle2D>())
            return this.Members<FoHandle2D>();
        return null;
    }
    public virtual List<FoConnectionPoint2D>? GetConnectionPoints()
    {
        if (!this.HasSlot<FoConnectionPoint2D>())
            return this.Members<FoConnectionPoint2D>();
        return null;
    }

    public bool CannotRender()
    {
        if ( !IsVisible ) return true;
        if ( !ShouldRender ) return true;
        return false;
    }

    public bool IsInRegion(Rectangle region)
    {
        if (LeftEdge() < region.X ) return false;
        if (RightEdge() > region.Width ) return false;  

        if (TopEdge() < region.Y ) return false;
        if (BottomEdge() > region.Height ) return false;
        return true;
    }

    public bool ComputeShouldRender(Rectangle region)
    {
        ShouldRender = IntersectsRegion(region);
        return ShouldRender;
    }

    public bool IntersectsRegion(Rectangle region)
    {
        return region.IntersectsWith(Rect());
    }
    public virtual async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        if ( !IsVisible ) return false;

        if ( !IsInRegion (region)) return false;

        try
        {
            await ctx.SaveAsync();
            await UpdateContext(ctx, 0);

            if ( ShouldRender )
                await ctx.FillRectAsync(0, 0, Width, Height);
            else
                await ctx.StrokeRectAsync(0, 0, Width, Height);
        }
        catch (System.Exception)
        {
            throw;
        }
        finally{
            await ctx.RestoreAsync();
        }

        return true;
    }

    public virtual async Task Draw(Canvas2DContext ctx, int tick)
    {
        await ctx.SaveAsync();
        ShapeDraw?.Invoke(ctx, this);
        await ctx.RestoreAsync();
        //await DrawPin(ctx);
    }

    public virtual async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if ( CannotRender() ) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        if ( !IsSelected )
            HoverDraw?.Invoke(ctx, this);
            

        if ( !string.IsNullOrEmpty(Tag))
        {
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);

            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync(Tag, LeftX() + 2, TopY() + 3);
        }

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
        {
            await ctx.SaveAsync();
            DrawSelected?.Invoke(ctx, this);
            GetHandles()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            //await DrawPin(ctx);
            await ctx.RestoreAsync();
        }

        if (deep)
        {
            GetMembers<FoShape1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
            GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        }
        await ctx.RestoreAsync();
        return true;
    }

    public async Task DrawOutline(Canvas2DContext ctx)
    {
        await ctx.BeginPathAsync();
        await ctx.SetLineDashAsync(new float[] { 15, 5 });
        await ctx.StrokeRectAsync(0, 0, Width, Height);
        await ctx.StrokeAsync();
    }

    public async Task DrawPin(Canvas2DContext ctx)
    {
        var loc = PinLocation();
        var cx = loc.X;
        var cy = loc.Y;

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync("#98AFC7");
        await ctx.ArcAsync(cx, cy, 6.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }

    public async Task DrawFancyPin(Canvas2DContext ctx)
    {
        var loc = PinLocation();
        var cx = loc.X;
        var cy = loc.Y;

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync("Green");
        await ctx.ArcAsync(cx, cy, 16.0, 0.0, 2 * Math.PI);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }

    public async Task DrawOrigin(Canvas2DContext ctx)
    {
        await ctx.BeginPathAsync();
        await ctx.SetLineDashAsync(new float[] { 15, 5 });
        await ctx.StrokeRectAsync(0, 0, Width, Height);
        await ctx.StrokeAsync();
    }

    public void AddToParent<T>(T parent) where T : FoGlyph2D
    {
        parent.Add(this);
    }

    public Action<Canvas2DContext, FoGlyph2D> DrawRect = async (ctx, obj) =>
    {
        //await ctx.FillRectAsync(obj.X, obj.Y, obj.Width, obj.Height);
        await ctx.FillRectAsync(0, 0, obj.Width, obj.Height);
    };

    public Action<Canvas2DContext, FoGlyph2D> DrawCircle = async (ctx, obj) =>
    {
        await ctx.BeginPathAsync();
        //await ctx.ArcAsync(obj.X, obj.Y, obj.Width, 0*Math.PI,2*Math.PI);
        await ctx.ArcAsync(obj.Width / 2, obj.Height / 2, obj.Width / 3, 0 * Math.PI, 2 * Math.PI);
        await ctx.FillAsync();
        await ctx.StrokeAsync();
    };

    public Action<Canvas2DContext, FoGlyph2D> DrawSelected = async (ctx, obj) =>
    {
        await ctx.SetFillStyleAsync("White");
        await ctx.SetLineWidthAsync(4);
        await obj.DrawOutline(ctx);
        await obj.DrawPin(ctx);
    };

    public List<T>? ExtractSelected<T>() where T : FoGlyph2D
    {
        var list = ExtractWhere<T>(child => child.IsSelected);
        return list;
    }

    public List<T>? FindGlyph<T>(string GlyphId) where T : FoGlyph2D
    {
        var list = FindWhere<T>(child => child.GlyphId == GlyphId);
        return list;
    }
    public List<T>? CaptureSelected<T>(FoGlyph2D source) where T : FoGlyph2D
    {
        var members = source.ExtractSelected<T>();
        if (members != null) Slot<T>().AddRange(members);
        return members;
    }

    public T CaptureShape<T>(T source, bool inPosition = false) where T : FoGlyph2D
    {
        if (inPosition)
        {
            var dx = -LeftEdge();
            var dy = -TopEdge();
            source.MoveBy(dx, dy);
        }

        return Add<T>(source);
    }

    protected int AssignInt(int newValue, int oldValue)
    {
        if (_matrix != null && Math.Abs(newValue - oldValue) > 2)
            Smash();

        return newValue;
    }

    protected double AssignDouble(double newValue, double oldValue)
    {
        if (_matrix != null && Math.Abs(newValue - oldValue) > 2)
            Smash();

        return newValue;
    }

    public virtual void Smash()
    {
        if ( _matrix == null ) return;
        $"Smashing {Name} {GetType().Name}".WriteInfo(2);

        this._matrix = null;
        this._invMatrix = null;
        ResetHitTesting = true;

        GetMembers<FoGlue2D>()?.ForEach(item =>
        {
            if ( !item.HasTarget(this) ) return;
            item.TargetMoved(this);
        });
    }

    public virtual Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            _matrix = new Matrix2D();
            _matrix.AppendTransform(this.PinX, this.PinY, 1.0, 1.0, RotationZ(this), 0.0, 0.0, LocPinX(this), LocPinY(this));
            //ResetHitTesting = true;
            //$"GetMatrix  {Name}".WriteLine(ConsoleColor.DarkBlue);
        }
        return _matrix;
    }

    public Matrix2D GetInvMatrix()
    {
        _invMatrix ??= this.GetMatrix().InvertCopy();
        return _invMatrix;
    }

    public void UnglueAll()
    {
        GetMembers<FoGlue2D>()?.ForEach(item => item.UnGlue() );
    }


    public void AddGlue(FoGlue2D glue)
    {

        //$"adding glue to {Name} glue {glue.Name}".WriteLine(ConsoleColor.DarkBlue);
        this.Add<FoGlue2D>(glue);
    }

    public void RemoveGlue(FoGlue2D glue)
    {
        //$"remove glue from {Name} glue {glue.Name}".WriteLine(ConsoleColor.DarkBlue);;
        this.Remove<FoGlue2D>(glue);
    }

    public virtual List<T>? ExtractWhere<T>(Func<T, bool> whereClause) where T : FoBase
    {
        var target = GetSlot<T>();
        return target?.ExtractWhere(whereClause);
    }

    public virtual List<T>? FindWhere<T>(Func<T, bool> whereClause) where T : FoBase
    {
        var target = GetSlot<T>();
        return target?.FindWhere(whereClause);
    }

    public override bool OpenCreate()
    {
        DoOnOpenCreate?.Invoke(this);
        return DoOnOpenCreate != null;
    }

    public override bool OpenEdit()
    {
        DoOnOpenEdit?.Invoke(this);
        return DoOnOpenEdit != null;
    }
}
