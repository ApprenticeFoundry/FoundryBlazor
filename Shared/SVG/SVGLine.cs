using System.Drawing;

namespace FoundryBlazor.Shared.SVG;

public class SVGLine {
     protected Point Start { get; set; }
     protected Point Finish { get; set; }

    public SVGLine(Point start, Point finish)
    {
        this.Start = start;
        this.Finish = finish;
    }

    public SVGLine(int x1, int y1, int x2, int y2)
    {
        this.Start = new Point(x1, y1);
        this.Finish = new Point(x2, y2);
    }
    public int X1 { get { return Start.X; } }
    public int Y1 { get { return Start.Y; } }
    public int X2 { get { return Finish.X; } }
    public int Y2 { get { return Finish.Y; } }
}
