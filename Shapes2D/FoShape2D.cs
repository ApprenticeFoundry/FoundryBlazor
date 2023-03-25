
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





    public override List<FoHandle2D> GetHandles()
    {
        if (!this.HasSlot<FoHandle2D>())
        {
            var lx = LeftX();
            var ty = TopY();
            var rx = RightX();
            var by = BottomY();
            AddHandle2D(new FoHandle2D("UL", lx, ty, "Green"));
            AddHandle2D(new FoHandle2D("UR", rx, ty, "Green"));
            AddHandle2D(new FoHandle2D("LL", lx, by, "Green"));
            AddHandle2D(new FoHandle2D("LR", rx, by, "Green"));
        }
        var result = this.Members<FoHandle2D>();
        return result;
    }



    public override List<FoConnectionPoint2D> GetConnectionPoints()
    {
        if (!this.HasSlot<FoConnectionPoint2D>())
        {
            var lx = LeftX();
            var ty = TopY();
            var rx = RightX();
            var by = BottomY();
            var cx = CenterX();
            var cy = CenterY();
            AddConnectionPoint2D(new FoConnectionPoint2D("LEFT", lx, cy, "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("TOP", cx, ty, "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("RIGHT", rx, cy, "Yellow"));
            AddConnectionPoint2D(new FoConnectionPoint2D("BOTTOM", cx, by, "Yellow"));
        }
        var result = this.Members<FoConnectionPoint2D>();
        return result;
    }

    public override bool SmashGlue()
    {
        GetMembers<FoHandle2D>()?.ForEach(item => item.SmashGlue());
        GetMembers<FoConnectionPoint2D>()?.ForEach(item => item.SmashGlue());
        return base.SmashGlue();
    }

}
