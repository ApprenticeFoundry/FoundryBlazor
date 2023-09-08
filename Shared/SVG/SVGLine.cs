using System.Drawing;

namespace FoundryBlazor.Shared.SVG;

public class SVGLine {
    protected Point start { get; set; };
    protected Point finish { get; set; };

    public SVGLine(Point start, Point finish)
    {
        this.start = start;
        this.finish = finish;
    }

    public SVGLine(int x1, int y1, int x2, int y2)
    {
        this.start = new Point(x1, y1);
        this.finish = new Point(x2, y2);
    }
    public int X1 { get { return start.X; } }
    public int Y1 { get { return start.Y; } }
    public int X2 { get { return finish.X; } }
    public int Y2 { get { return finish.Y; } }
}
