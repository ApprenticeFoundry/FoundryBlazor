using System.Drawing;

namespace FoundryBlazor.Shape;

public class FoShape2D : FoGlyph2D, IShape2D
{

    public FoShape2D() : base()
    {
        ShapeDraw = DrawRect;
    }

    public FoShape2D(int width, int height, string color) : base("", width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawRect;
    }

    public FoShape2D(string name, int width, int height, string color) : base(name, width, height, color)
    {
        PinX = PinY = 0;
        ShapeDraw = DrawRect;
    }

    public override Point AttachTo()
    {
        var point = base.AttachTo();
     
        //you need to compute where that point is on the parent !!
        //do this for real using a matrix

        if (Level > 0 && GetParent() is FoGlyph2D parent)
        {
            //$"------------------------------++++++AttachTo OLD {point.X}  {point.Y}".WriteInfo(2);

            //var matrix = GetMatrix();
            //point = matrix.TransformPoint(point);

            var matrix = parent.GetMatrix();
            point = matrix.TransformPoint(point);
            //$"AttachTo NEW {point.X}  {point.Y}".WriteInfo(2);
        }
        //else
        //    $"No Parent AttachTo {Name}".WriteError();

        return point;
    }

    public FoHandle2D AddHandle2D(FoHandle2D point)
    {
        point.GetParent = () => this;
        point.Level = Level + 1;
        Add<FoHandle2D>(point);
        return point;
    }

    public override List<FoHandle2D> GetHandles() 
    {
        if ( !this.HasSlot<FoHandle2D>()) 
        {
            AddHandle2D(new FoHandle2D("UL", LeftX(), TopY(), "Green"));
            AddHandle2D(new FoHandle2D("UR", RightX(), TopY(), "Green"));
            AddHandle2D(new FoHandle2D("LL", LeftX(), BottomY(), "Green"));
            AddHandle2D(new FoHandle2D("LR", RightX(), BottomY(), "Green"));
        }
        var result = this.Members<FoHandle2D>();
        return result;
    }

    public FoConnectionPoint2D AddConnectionPoint2D(FoConnectionPoint2D point)
    {
        point.GetParent = () => this;
        point.Level = Level + 1;
        Add<FoConnectionPoint2D>(point);
        return point;
    }

    public override List<FoConnectionPoint2D> GetConnectionPoints() 
    {
        if ( !this.HasSlot<FoConnectionPoint2D>()) 
        {
            AddConnectionPoint2D(new FoConnectionPoint2D("LEFT", LeftX(), CenterY(), "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("TOP", CenterX(), TopY(), "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("RIGHT", RightX(), CenterY(), "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("BOTTOM", CenterX(), BottomY(), "Yellow"));
        }
        var result = this.Members<FoConnectionPoint2D>();
        return result;
    }

    public override bool SmashGlue()
    {
        GetHandles().ForEach(item => item.SmashGlue());
        GetConnectionPoints().ForEach(item => item.SmashGlue());
        return base.SmashGlue();
    }

}
