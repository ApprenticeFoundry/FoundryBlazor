using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared;
using FoundryBlazor.Shared.SVG;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using System.Drawing;
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

public enum ClickStyle
{
    None,
    MouseDown,
    MouseUp,
    DoubleClick
}



public interface IRender
{
    public Task Draw(Canvas2DContext ctx, int tick);
    public Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true);
    public bool RenderDeepDetailed(Canvas2DContext ctx, int tick);
    public Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region);

}


public class MeasuredText
{
    public string Text = "";
    public int Width = 0;
    public int Height = 0;
    public bool Failure = false;

}

public interface IGlyph2D : ICanHitTarget
{
    FoGlyph2D MarkSelected(bool selected);
    bool IsSelectable();
}

public class FoGlyph2D : FoComponent, IGlyph2D, IRender
{
    public static Tweener Animations { get; set; } = new Tweener();
    private static bool _resetHitTesting = false;

    public float Thickness { get; set; }
    public bool Selectable 
    { 
        get { return this.StatusBits.IsSelectable; } 
        set { this.StatusBits.IsSelectable = value; } 
    }
    public bool IsSelected
    {
        get { return this.StatusBits.IsSelected; }
        set { this.StatusBits.IsSelected = value; }
    }
    public bool IsVisible
    {
        get { return this.StatusBits.IsVisible; }
        set { this.StatusBits.IsVisible = value; }
    }
    public bool ShouldRender { 
        get { return this.StatusBits.ShouldRender; } 
        set { this.StatusBits.ShouldRender = value; } 
    }
    
    public string Tag { get; set; } = "";
    public int Level { get; set; } = 0;
    public int Index { get; set; } = 0;

    public string id = Guid.NewGuid().ToString(); //use this to trap changes in GlyphId
    public string GlyphId
    {
        get { return this.id; }
        set { this.id = value; }
    }


    protected Rectangle rectangle = new(0, 0, 0, 0);
    public static bool PeekResetHitTesting()
    {
        var result = _resetHitTesting;
        return result;
    }
    public static bool MustResetHitTesting()
    {
        var result = _resetHitTesting;
        _resetHitTesting = false;
        return result;
    }
    public static void ResetHitTesting(bool value, string note = "")
    {
        if (_resetHitTesting == value)
            return;

        _resetHitTesting = value;
        // if (_resetHitTesting)
        //     $"ResetHitTesting on next itteration {note}".WriteInfo();
    }


    protected int x = 0;
    public int PinX { 
        get { return this.x; } 
        set { this.x = AssignInt(value, x); } }
    protected int y = 0;
    public int PinY { get { return this.y; } set { this.y = AssignInt(value, y); } }
    protected int width = 0;
    public int Width { get { return this.width; } set { this.width = AssignInt(value, width); } }
    protected int height = 0;
    public int Height { get { return this.height; } set { this.height = AssignInt(value, height); } }

    private double angle = 0;
    public double Angle { get { return this.angle; } set { this.angle = AssignDouble(value, angle); } }
    public float Opacity { get; set; } = 1.0F;

    public string color = "pink";
    public string Color
    {
        get { return this.color; }
        set
        {
            this.color = value;
            // $"{GetType().Name} {Name} Color Change {color}".WriteNote();
        }
    }


    protected FoDynamicRender? foDynamicRender;
    public virtual FoDynamicRender GetDynamicRender()
    {
        foDynamicRender ??= new FoDynamicRender(typeof(Shape2D), this);
        return foDynamicRender;
    }



    public Func<FoGlyph2D, int> LocPinX = (obj) => obj.Width / 2;
    public Func<FoGlyph2D, int> LocPinY = (obj) => obj.Height / 2;
    public Func<FoGlyph2D, double> RotationZ = (obj) => obj.Angle;

    public int LeftEdge() { return PinX - LocPinX(this); }
    public int TopEdge() { return PinY - LocPinY(this); }
    public int BottomEdge() { return TopEdge() + Height; }
    public int RightEdge() { return LeftEdge() + Width; }

    public virtual void OnShapeClick(ClickStyle style, CanvasMouseArgs args)
    {
    }



    protected Action<FoGlyph2D>? OnMatrixRefresh;
    protected Action<FoGlyph2D>? OnMatrixSmash;

    public Action<FoGlyph2D, int>? ContextLink;
    protected Action<Canvas2DContext, FoGlyph2D>? PreDraw;
    protected Action<Canvas2DContext, FoGlyph2D>? HoverDraw;
    protected Action<Canvas2DContext, FoGlyph2D>? PostDraw;
    public Action<Canvas2DContext, FoGlyph2D>? ShapeDraw;
    public Action<Canvas2DContext, FoGlyph2D>? ShapeDrawSelected;

    protected Matrix2D? _matrix;
    protected Matrix2D? _invMatrix;

    public List<TreeNodeAction> DefaultActions = [];
    public void AddAction(string name, string color, Action action)
    {
        DefaultActions.AddAction(name, color, action);
    }

    public override IEnumerable<TreeNodeAction> GetTreeNodeActions()
    {
        var result = new List<TreeNodeAction>();
        result.AddRange(DefaultActions);
        return result;
    }
    
    public FoGlyph2D() : base("")
    {
        Color = "Green";
        ShapeDraw = DrawRect;
    }
    public FoGlyph2D(string name, string color) : base(name)
    {
        Color = color;
        ShapeDraw = DrawRect;
    }

    public FoGlyph2D(string name, int width, int height, string color) : base(name)
    {
        this.width = width;
        this.height = height;
        Color = color;
        ShapeDraw = DrawRect;
    }

    public string GetName()
    {
        return Key;
    }
    public virtual string GetText()
    {
        return Tag;
    }
    public string GetGlyphId()
    {
        if (string.IsNullOrEmpty(GlyphId))
            GlyphId = Guid.NewGuid().ToString();

        return GlyphId;
    }

    public bool GlyphIdCompare(string other)
    {
        var id = GetGlyphId();
        var result = id == other;
        // $"GlyphIdCompare {result}  {id} {other}".WriteNote();
        return result;
    }

    public Point ParentAttachTo(Point source)
    {
        if (Level > 0 && GetParent() is FoGlyph2D parent)
        {
            var matrix = parent.GetMatrix();
            source = matrix.TransformToPoint(source.X, source.Y);
            return parent.ParentAttachTo(source);
        }

        return source;
    }

    public FoGlyph2D ClearPreDraw()
    {
        PreDraw = null;
        return this;
    }

    public bool IsHovering()
    {
        var result = HoverDraw != null;
        return result;
    }
    
    public FoGlyph2D ClearHoverDraw()
    {
        HoverDraw = null;
        return this;
    }
    public FoGlyph2D ClearPostDraw()
    {
        PostDraw = null;
        return this;
    }

    public FoGlyph2D SetPreDraw(Action<Canvas2DContext, FoGlyph2D> action)
    {
        PreDraw = action;
        return this;
    }

    public FoGlyph2D SetHoverDraw(Action<Canvas2DContext, FoGlyph2D> action)
    {
        HoverDraw = action;
        return this;
    }
    public FoGlyph2D SetPostDraw(Action<Canvas2DContext, FoGlyph2D> action)
    {
        PostDraw = action;
        return this;
    }
    
    public virtual Point AttachTo()
    {
        var point = new Point(PinX, PinY);

        //you need to compute where that point is on the parent !!
        //do this for real using a matrix

        return ParentAttachTo(point);
    }

    public static async Task<MeasuredText> ComputeMeasuredText(Canvas2DContext ctx, string Fragment, string FontSize, string Font)
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
            $"FoText2D ComputeSize {count} {Fragment} {result.Width} {result.Height}  FONT:{ctx.Font}  SPEC:{FontSpec}".WriteInfo();
        }
        while (result.Failure && count < 3);

        await ctx.RestoreAsync();

        return result;
    }

    public Point ResetLocalPin(Func<FoGlyph2D, int> locX, Func<FoGlyph2D, int> locY)
    {
        LocPinX = locX;
        LocPinY = locY;
        Smash(false);
        return PinLocation();
    }

    public virtual List<T> CollectMembers<T>(List<T> list, bool deep = true) where T : FoGlyph2D
    {
        var members = GetMembers<T>();
        if (members != null)
            list.AddRange(members);

        return list;
    }

    public bool IsSelectable()
    {
        return Selectable;
    }
    public virtual FoGlyph2D MarkSelected(bool value)
    {
        if (Selectable)
            this.IsSelected = value;
        else
            $"Shape {Key} is not selectable".WriteNote();
        return this;
    }

    public virtual bool LocalMouseHover(CanvasMouseArgs args, Rectangle loc, Action<Canvas2DContext, FoGlyph2D>? OnHover)
    {
        return false;
    }

    public virtual Point[] HitTestSegment()
    {
        var mat = GetMatrix();
        var p1 = mat.TransformToPoint(0, 0);
        var p2 = mat.TransformToPoint(Width, 0);
        var p3 = mat.TransformToPoint(Width, Height);
        var p4 = mat.TransformToPoint(0, Height);
        return [p1, p2, p3, p4];
    }
    
    public virtual Rectangle HitTestRect()
    {
        //this does not work for rotated objects
        var mat = GetMatrix();
        mat.TransformRectangle(0, 0, Width, Height, ref rectangle);
        return rectangle;
    }

    public virtual FoGlyph2D ResizeTo(int width, int height) { (Width, Height) = (width, height); return this; }
    public void ResizeBy(int dx, int dy) => (Width, Height) = (Width + dx, Height + dy);
    public FoGlyph2D MoveTo(int x, int y) { (PinX, PinY) = (x, y); return this; }
    public virtual void MoveBy(int dx, int dy) 
    {
        (PinX, PinY) = (PinX + dx, PinY + dy);
    }
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
    public int LeftX() => FractionX(0.0);
    public int RightX() => FractionX(1.0);
    public int CenterY() => FractionY(0.5);
    public int TopY() => FractionY(0.0);
    public int BottomY() => FractionY(1.0);

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

    public Tween AnimatedMoveFrom(int xStart, int yStart, float duration = 2.0F, float delay = 0)
    {
        var x = PinX;
        var y = PinY;
        PinX = xStart;
        PinY = yStart;
        return Animations.Tween(this, new { PinX = x, PinY = y }, duration, delay).Ease(Ease.ElasticInOut);
    }

    public Tween AnimatedMoveTo(int x, int y, float duration = 2.0F, float delay = 0)
    {
        return Animations.Tween(this, new { PinX = x, PinY = y }, duration, delay).Ease(Ease.ElasticInOut);
    }

    public Tween AnimatedRotateTo(double angle, float duration = 2.0F, float delay = 0)
    {
        return Animations.Tween(this, new { Angle = angle }, duration, delay);
    }
    public Tween AnimatedResizeTo(int w, int h, float duration = 2.0F, float delay = 0)
    {
        return Animations.Tween(this, new { Width = w, Height = h }, duration, delay).Ease(Ease.ElasticInOut);
    }
    public Tween AnimatedGrowFromZero(float duration = 2.0F)
    {
        var w = Width;
        var h = Height;
        Width = 0;
        Height = 0;
        return Animations.Tween(this, new { Width = w, Height = h }, duration).Ease(Ease.ElasticInOut);
    }

    public FoGlyph2D BeforeShapeRefresh(Action<FoGlyph2D, int> action)
    {
        ContextLink = action;
        return this;
    }

    public FoGlyph2D AfterMatrixRefresh(Action<FoGlyph2D> action)
    {
        OnMatrixRefresh = action;
        return this;
    }
    public FoGlyph2D AfterMatrixSmash(Action<FoGlyph2D> action)
    {
        OnMatrixSmash = action;
        return this;
    }



    public virtual async Task UpdateContext(Canvas2DContext ctx, int tick)
    {
        ContextLink?.Invoke(this, tick);

        var mtx = this.GetMatrix();
        //you must use Transform so the context can acumlate the positions
        if (mtx != null)  //this should NEVER be the case unless cleared by another process
            await ctx.TransformAsync(mtx.a, mtx.b, mtx.c, mtx.d, mtx.tx, mtx.ty);

        //await ctx.SetGlobalAlphaAsync(ctx.GlobalAlpha * this.Opacity);

        await ctx.SetFillStyleAsync(Color);
    }

    public FoHandle2D AddHandle2D(FoHandle2D point)
    {
        point.GetParent = () => this;
        point.Level = Level + 1;
        Add<FoHandle2D>(point);
        return point;
    }
    public FoConnectionPoint2D AddConnectionPoint2D(FoConnectionPoint2D point)
    {
        point.GetParent = () => this;
        point.Level = Level + 1;
        Add<FoConnectionPoint2D>(point);
        return point;
    }
    public FoHandle2D? FindHandle(string key, bool force = false)
    {
        if (force) GetHandles();
        return this.Find<FoHandle2D>(key);
    }

    public virtual List<FoHandle2D>? GetHandles()
    {
        if (!this.HasSlot<FoHandle2D>())
            return this.Members<FoHandle2D>();
        return null;
    }

    public FoConnectionPoint2D? FindConnectionPoint(string key, bool force = false)
    {
        if (force) GetConnectionPoints();
        return this.Find<FoConnectionPoint2D>(key);
    }

    public virtual List<FoConnectionPoint2D>? GetConnectionPoints()
    {
        if (!this.HasSlot<FoConnectionPoint2D>())
            return this.Members<FoConnectionPoint2D>();
        return null;
    }

    public bool CannotRender()
    {
        if (!IsVisible) return true;
        if (!ShouldRender) return true;
        return false;
    }



    public bool ComputeShouldRender(Rectangle region)
    {
        ShouldRender = true; // IntersectsRegion(region);
        return ShouldRender;
    }

    public bool IntersectsRegion(Rectangle region)
    {
        return region.IntersectsWith(HitTestRect());
    }

    public bool IsInRegion(Rectangle region)
    {
        if (LeftEdge() < region.X) return false;
        if (RightEdge() > region.Width) return false;

        if (TopEdge() < region.Y) return false;
        if (BottomEdge() > region.Height) return false;
        return true;
    }

    public virtual async Task<bool> RenderConcise(Canvas2DContext ctx, double scale, Rectangle region)
    {
        if (!IsVisible) return false;

        if (!IsInRegion(region)) return false;

        try
        {
            await ctx.SaveAsync();
            await UpdateContext(ctx, 0);

            if (ShouldRender)
                await ctx.FillRectAsync(0, 0, Width, Height);
            else
                await ctx.StrokeRectAsync(0, 0, Width, Height);
        }
        catch (System.Exception)
        {
            throw;
        }
        finally
        {
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

    public virtual bool RenderDeepDetailed(Canvas2DContext ctx, int tick)
    {
        GetMembers<FoShape1D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick));
        GetMembers<FoShape2D>()?.ForEach(async child => await child.RenderDetailed(ctx, tick));
        return true;
    }
    public virtual async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (CannotRender()) return false;

        await ctx.SaveAsync();
        await UpdateContext(ctx, tick);

        PreDraw?.Invoke(ctx, this);
        await Draw(ctx, tick);
        if (!IsSelected)
            HoverDraw?.Invoke(ctx, this);

        await DrawTag(ctx);

        PostDraw?.Invoke(ctx, this);

        if (IsSelected)
            await DrawWhenSelected(ctx, tick, deep);

        if (deep)
            RenderDeepDetailed(ctx, tick);


        // if (GetMembers<FoGlue2D>()?.Count > 0)
        //     await DrawTriangle(ctx, "Black");


        await ctx.RestoreAsync();
        return true;
    }



    public async virtual Task DrawWhenSelected(Canvas2DContext ctx, int tick, bool deep)
    {
        await ctx.SaveAsync();
        ShapeDrawSelected?.Invoke(ctx, this);
        GetHandles()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        GetConnectionPoints()?.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));
        //await DrawPin(ctx);
        await ctx.RestoreAsync();
    }

    public async Task DrawTag(Canvas2DContext ctx, float rotation = 0)
    {
        if (!string.IsNullOrEmpty(Tag))
        {
            await ctx.SaveAsync();
            await ctx.SetFontAsync("12px Segoe UI");
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);

            await ctx.RotateAsync(rotation);
            await ctx.SetFillStyleAsync("Black");
            await ctx.FillTextAsync(Tag, LeftX() + 2, TopY() + 3);
            await ctx.RestoreAsync();
        }
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


    public async Task DrawStar(Canvas2DContext ctx, string color)
    {
        var loc = PinLocation();

        int StarWidth = 40;
        int StarHeight = 40;
        int StarCenterX = loc.X;
        int StarCenterY = loc.Y;

        var scale = Math.Min((double)StarWidth / 400, (double)StarHeight / 400);
        var starWidth = (int)(400 * scale);
        var starHeight = (int)(400 * scale);
        var top = StarCenterY - starHeight / 2;
        var left = StarCenterX - starWidth / 2;

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.MoveToAsync(left + 200 * scale, top + 100 * scale);
        await ctx.LineToAsync(left + 240 * scale, top + 180 * scale);
        await ctx.LineToAsync(left + 330 * scale, top + 180 * scale);
        await ctx.LineToAsync(left + 260 * scale, top + 230 * scale);
        await ctx.LineToAsync(left + 300 * scale, top + 310 * scale);
        await ctx.LineToAsync(left + 200 * scale, top + 260 * scale);
        await ctx.LineToAsync(left + 100 * scale, top + 310 * scale);
        await ctx.LineToAsync(left + 140 * scale, top + 230 * scale);
        await ctx.LineToAsync(left + 70 * scale, top + 180 * scale);
        await ctx.LineToAsync(left + 160 * scale, top + 180 * scale);
        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }

    public async Task DrawArrow(Canvas2DContext ctx, int HeadWidth, int HeadHeight, int BodyWidth, int BodyHeight, int BodyThickness, string color)
    {
        //var loc = PinLocation();

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);

        await ctx.MoveToAsync(0, Height / 2);
        await ctx.LineToAsync(BodyWidth, Height / 2);
        await ctx.LineToAsync(BodyWidth, (Height / 2) - (BodyHeight / 2) + (BodyThickness / 2));
        await ctx.FillRectAsync(BodyWidth, (Height / 2) - (BodyHeight / 2), Width - HeadWidth - BodyWidth, BodyThickness);
        await ctx.LineToAsync(Width - HeadWidth, (Height / 2) - (HeadHeight / 2));
        await ctx.LineToAsync(Width, Height / 2);
        await ctx.LineToAsync(Width - HeadWidth, (Height / 2) + (HeadHeight / 2));
        await ctx.LineToAsync(BodyWidth, (Height / 2) + (BodyHeight / 2) - (BodyThickness / 2));
        await ctx.FillRectAsync(BodyWidth, (Height / 2) + (BodyHeight / 2) - BodyThickness, Width - HeadWidth - BodyWidth, BodyThickness);
        await ctx.LineToAsync(BodyWidth, Height / 2);

        await ctx.ClosePathAsync();

        await ctx.FillAsync();

        await ctx.SetLineWidthAsync(1);
        await ctx.SetStrokeStyleAsync("#003300");
        await ctx.StrokeAsync();

        await ctx.RestoreAsync();
    }

    public async Task DrawTriangle(Canvas2DContext ctx, string color)
    {
        var loc = PinLocation();
        var cx = loc.X;
        var cy = loc.Y;
        var d = 8;

        await ctx.SaveAsync();
        await ctx.BeginPathAsync();

        await ctx.SetFillStyleAsync(color);
        await ctx.MoveToAsync(cx, cy + 2 * d);
        await ctx.LineToAsync(cx + d, cy);
        await ctx.LineToAsync(cx - d, cy);
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



    public Action<Canvas2DContext, FoGlyph2D> DrawBox = async (ctx, obj) =>
    {
        await ctx.BeginPathAsync();
        await ctx.StrokeRectAsync(0, 0, obj.Width, obj.Height);
        await ctx.StrokeAsync();
    };

    public Action<Canvas2DContext, FoGlyph2D> DrawRect = async (ctx, obj) =>
    {
        await ctx.FillRectAsync(0, 0, obj.Width, obj.Height);
    };

    public Action<Canvas2DContext, FoGlyph2D> DrawCircle = async (ctx, obj) =>
    {
        await ctx.BeginPathAsync();
        await ctx.ArcAsync(obj.Width / 2, obj.Height / 2, obj.Width / 3, 0 * Math.PI, 2 * Math.PI);
        await ctx.FillAsync();
        await ctx.StrokeAsync();
    };

    public Action<Canvas2DContext, FoGlyph2D> DrawAsSelected = async (ctx, obj) =>
    {
        await ctx.SetFillStyleAsync("White");
        await ctx.SetLineWidthAsync(4);
        await obj.DrawOutline(ctx);
        await obj.DrawPin(ctx);
    };

    public virtual List<FoImage2D> CollectImages(List<FoImage2D> list, bool deep = true)
    {
        return list;
    }
    public virtual List<FoVideo2D> CollectVideos(List<FoVideo2D> list, bool deep = true)
    {
        return list;
    }

    public List<T>? ExtractSelectedShapes<T>(List<T> foGlyph2Ds) where T : FoGlyph2D
    {
        var list = ExtractWhere<T>(child => child.IsSelected);
        return list;
    }

    public List<T>? FindGlyph<T>(string GlyphId) where T : FoGlyph2D
    {
        var list = FindWhere<T>(child => child.GlyphIdCompare(GlyphId));
        return list;
    }
    //public List<T>? CaptureSelected<T>(FoGlyph2D source) where T : FoGlyph2D
    //{
    //    var members = source.ExtractSelected<T>();
    //    if (members != null) Slot<T>().AddRange(members);
    //    return members;
    //}

    public virtual T CaptureShape<T>(T source, bool inPosition = false) where T : FoGlyph2D
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
        if (Math.Abs(newValue - oldValue) > 0)
        {
            Smash(true);
            //$"AssignInt {Name} {GetType().Name} {oldValue} -> {newValue}".WriteInfo(2);
        }

        return newValue;
    }

    protected double AssignDouble(double newValue, double oldValue)
    {
        if (Math.Abs(newValue - oldValue) > 0)
            Smash(true);

        return newValue;
    }

    // public virtual bool SmashParents()
    // {
    //     if ( _globalMatrix == null ) return false;
    //     this._globalMatrix = null;

    //     return true;
    // }

    public virtual bool SmashGlue()
    {
        var list = GetMembers<FoGlue2D>();
        if (list == null) return false;
        //$"Smashing Glue {Name} {GetType().Name}".WriteInfo(2);

        list.ForEach(item => item.TargetMoved(this));
        return true;
    }

    public bool IsSmashed()
    {
        return _matrix == null;
    }

    public virtual bool Smash(bool force)
    {
        if (_matrix == null && !force) return false;
        //$"Smashing {Name} {GetType().Name}".WriteInfo(2);

        OnMatrixSmash?.Invoke(this);

        //SRS SET THIS IN ORDER TO Do ANY HITTEST!!!!
        ResetHitTesting(true, "Glyph Smashed");

        this._matrix = Matrix2D.SmashMatrix(this._matrix);
        this._invMatrix = Matrix2D.SmashMatrix(this._invMatrix);

        return this.SmashGlue();
    }

    public virtual Matrix2D GetMatrix()
    {
        if (_matrix == null)
        {
            _matrix = Matrix2D.NewMatrix();
            try
            {
                _matrix.AppendTransform(this.PinX, this.PinY, 1.0, 1.0, RotationZ(this), LocPinX(this), LocPinY(this));
            }
            catch (Exception ex)
            {
                $"Error in GetMatrix {ex.Message}".WriteError();
            }
            OnMatrixRefresh?.Invoke(this);
        }
        return _matrix;
    }

    // public virtual Matrix2D GetGlobalMatrix()
    // {
    //     if ( _globalMatrix == null )
    //     {
    //         _globalMatrix = GetMatrix().Clone();
    //         var parent = GetParent();
    //         if (parent != null) 
    //         {
    //             _globalMatrix.PrependMatrix(parent.GetGlobalMatrix());
    //             $"PrePending {Name} to parent {parent.Name}".WriteInfo();
    //         } else
    //         {
    //              $"No Parent {Name}".WriteInfo(); 
    //         }

    //     }
    //     return _globalMatrix;
    // }


    // public void GlobalMatrixComputeTest(FoGlyph2D source, Matrix2D mat, int X, int Y, int level, string path)
    // {
    //     $"{level}]{path} Source {source.Name}: {X} {Y}".WriteSuccess(level);
    //     var point = mat.TransformPoint(X,Y);
    //     $"{level}]{path} TForm {source.Name}: {point.X} {point.Y}".WriteSuccess(level);

    //     var parent = source.GetParent();
    //     if (parent == null) return;

    //     var newPath = $"{parent.Name}.{path}";
    //     var pMat = parent.GetMatrix();
    //     pMat = mat.PrependMatrix(pMat);
    //     $"PrePending {Name} to parent {parent.Name}".WriteInfo(level);
    //     GlobalMatrixComputeTest(parent, pMat, X, Y, level + 1, newPath);
    // }



    public Matrix2D GetInvMatrix()
    {
        _invMatrix ??= this.GetMatrix().InvertCopy();
        return _invMatrix;
    }

    // public Matrix2D GetInvGlobalMatrix()
    // {
    //     _invGlobalMatrix ??= this.GetGlobalMatrix().InvertCopy();
    //     return _invGlobalMatrix;
    // }

    public void UnglueAll()
    {
        GetMembers<FoGlue2D>()?.ForEach(item => item.UnGlue());
        this.SmashGlue();
    }



    public void AddGlue(FoGlue2D glue)
    {
        //$"adding glue to {Name} glue {glue.Name}".WriteLine(ConsoleColor.DarkBlue);
        this.Add<FoGlue2D>(glue);
        this.SmashGlue();
    }

    public void RemoveGlue(FoGlue2D glue)
    {
        //$"remove glue from {Name} glue {glue.Name}".WriteLine(ConsoleColor.DarkBlue);;
        this.Remove<FoGlue2D>(glue);
        this.SmashGlue();
    }




}
