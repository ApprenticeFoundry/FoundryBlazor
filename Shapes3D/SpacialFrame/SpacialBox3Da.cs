using BlazorThreeJS.Maths;



namespace FoundryBlazor.Shape;

//this data is always computed in local space (0,0,0) to (W,H,D) 
//the SpacialBox3D can then be transformed in 3D space with position 
//data is never cashed or transformed, always computed on the fly
public class SpacialBox3D
{
    public string Units { get; set; } = "m";
    public double Width { get; set; } = 1.0;
    public double Height { get; set; } = 1.0;
    public double Depth { get; set; } = 1.0;

    private double HalfWidth { get; set; }
    private double HalfHeight { get; set; }
    private double HalfDepth { get; set; }


    public double Volume => Width * Height * Depth;

    public double SurfaceArea => 2 * (Width * Height + Height * Depth + Width * Depth);

    public SpacialBox3D(double width, double height, double depth, string units = "m")
    {
        Width = width;
        Height = height;
        Depth = depth;
        Units = units;
        HalfWidth = Width / 2;
        HalfHeight = Height / 2;
        HalfDepth = Depth / 2;
    }

    public SpacialBox3D(FoSpec3D spec, string units = "m") : this(spec.W, spec.H, spec.D, units)
    {
    }

    public SpacialBox3D(FoShape3D glyph, string units = "m") : this(glyph.Width, glyph.Height, glyph.Depth, units)
    {
    }

    public Point3D Center => new(HalfWidth-HalfWidth, HalfHeight-HalfHeight, HalfDepth-HalfDepth, "Center");

    public Point3D LeftTopFront => new(0-HalfWidth, Height-HalfHeight, Depth-HalfDepth, "LeftTopFront");
    public Point3D RightTopFront => new(Width-HalfWidth, Height-HalfHeight, Depth-HalfDepth, "RightTopFront");
    public Point3D LeftBottomFront => new(0-HalfWidth, 0-HalfHeight, Depth-HalfDepth, "LeftBottomFront");
    public Point3D RightBottomFront => new(Width-HalfWidth, 0-HalfHeight, Depth-HalfDepth, "RightBottomFront");
    public Point3D LeftTopBack => new(0-HalfWidth, Height-HalfHeight, 0-HalfDepth, "LeftTopBack");
    public Point3D RightTopBack => new(Width-HalfWidth, Height-HalfHeight, 0-HalfDepth, "RightTopBack");
    public Point3D LeftBottomBack => new(0-HalfWidth, 0-HalfHeight, 0-HalfDepth, "LeftBottomBack");
    public Point3D RightBottomBack => new(Width-HalfWidth, 0-HalfHeight, 0-HalfDepth, "RightBottomBack");

    public List<Point3D> GetLocalVertices() => new List<Point3D>
    {
        LeftTopFront,
        RightTopFront,
        LeftBottomFront,
        RightBottomFront,
        LeftTopBack,
        RightTopBack,
        LeftBottomBack,
        RightBottomBack
    };


    public Face3D GetLocalLeftFace()
    {
        return new Face3D("Left", new List<Point3D>
        {
            LeftTopFront,
            LeftTopBack,
            LeftBottomBack,
            LeftBottomFront
        }, new Vector3(-1, 0, 0));
    }

    public Face3D GetLocalRightFace()
    {
        return new Face3D("Right", new List<Point3D>
        {
            RightTopFront,
            RightTopBack,
            RightBottomBack,
            RightBottomFront
        }, new Vector3(1, 0, 0));
    }

    public Face3D GetLocalFrontFace()
    {
        return new Face3D("Front", new List<Point3D>
        {
            LeftTopFront,
            RightTopFront,
            RightBottomFront,
            LeftBottomFront
        }, new Vector3(0, 0, 1));
    }

    public Face3D GetLocalBackFace()
    {
        return new Face3D("Back", new List<Point3D>
        {
            LeftTopBack,
            RightTopBack,
            RightBottomBack,
            LeftBottomBack
        }, new Vector3(0, 0, -1));
    }

    public Face3D GetLocalTopFace()
    {
        return new Face3D("Top", new List<Point3D>
        {
            LeftTopFront,
            RightTopFront,
            RightTopBack,
            LeftTopBack
        }, new Vector3(0, 1, 0));
    }

    public Face3D GetLocalBottomFace()
    {
        return new Face3D("Bottom", new List<Point3D>
        {
            LeftBottomFront,
            RightBottomFront,
            RightBottomBack,
            LeftBottomBack
        }, new Vector3(0, -1, 0));
    }

    public List<Face3D> GetLocalFaces()
    {
        return new List<Face3D>
        {
            GetLocalLeftFace(),
            GetLocalRightFace(),
            GetLocalFrontFace(),
            GetLocalBackFace(),
            GetLocalTopFace(),
            GetLocalBottomFace()
        };
    }






    public Point3D LocalFrontFaceCenter => new(HalfWidth, HalfHeight, 0, "front");
    public Point3D LocalRearFaceCenter => new(HalfWidth, HalfHeight, Depth, "rear");
    public Point3D LocalLeftFaceCenter => new(0, HalfHeight, HalfDepth, "left");
    public Point3D LocalRightFaceCenter => new(Width, HalfHeight, HalfDepth, "right");
    public Point3D LocalTopFaceCenter => new(HalfWidth, Height, HalfDepth, "top");
    public Point3D LocalBottomFaceCenter => new(HalfWidth, 0, HalfDepth, "bottom");

    public virtual List<Point3D> GetLocalFaceCenters() => new List<Point3D>
    {
        LocalFrontFaceCenter,
        LocalRearFaceCenter,
        LocalLeftFaceCenter,
        LocalRightFaceCenter,
        LocalTopFaceCenter,
        LocalBottomFaceCenter
    };



    public Point3D LocalCenterTopFrontEdge => new Point3D(HalfWidth, Height, Depth, "CenterTopFront");
    public Point3D LocalCenterTopBackEdge => new Point3D(HalfWidth, Height, 0, "CenterTopBack");

    public Point3D LocalCenterTopLeftEdge => new Point3D(0, Height, HalfDepth, "CenterTopLeft");
    public Point3D LocalCenterTopRightEdge => new Point3D(Width, Height, HalfDepth, "CenterTopRight");

    public Point3D LocalCenterBottomFrontEdge => new Point3D(HalfWidth, 0, Depth, "CenterBottomFront");
    public Point3D LocalCenterBottomBackEdge => new Point3D(HalfWidth, 0, 0, "CenterBottomBack");

    public Point3D LocalCenterBottomLeftEdge => new Point3D(0, 0, HalfDepth, "CenterBottomLeft");
    public Point3D LocalCenterBottomRightEdge => new Point3D(Width, 0, HalfDepth, "CenterBottomRight");

    public Point3D LocalCenterFrontLeftEdge => new Point3D(0, HalfHeight, Depth, "CenterFrontLeft");
    public Point3D LocalCenterFrontRightEdge => new Point3D(Width, HalfHeight, Depth, "CenterFrontRight");

    public Point3D LocalCenterBackLeftEdge => new Point3D(0, HalfHeight, 0, "CenterBackLeft");
    public Point3D LocalCenterBackRightEdge => new Point3D(Width, HalfHeight, 0, "CenterBackRight");

    public Point3D LocalCenterTopEdge => new Point3D(HalfWidth, Height, HalfDepth, "CenterTop");
    public Point3D LocalCenterBottomEdge => new Point3D(HalfWidth, 0, HalfDepth, "CenterBottom");

    public Point3D LocalCenterFrontEdge => new Point3D(HalfWidth, HalfHeight, Depth, "CenterFront");
    public Point3D LocalCenterBackEdge => new Point3D(HalfWidth, HalfHeight, 0, "CenterBack");

    public virtual List<Point3D> GetLocalEdgeCenters() => new List<Point3D>
    {
        LocalCenterTopFrontEdge,
        LocalCenterTopBackEdge,
        LocalCenterTopLeftEdge,
        LocalCenterTopRightEdge,
        LocalCenterBottomFrontEdge,
        LocalCenterBottomBackEdge,
        LocalCenterBottomLeftEdge,
        LocalCenterBottomRightEdge,
        LocalCenterFrontLeftEdge,
        LocalCenterFrontRightEdge,
        LocalCenterBackLeftEdge,
        LocalCenterBackRightEdge,
        LocalCenterTopEdge,
        LocalCenterBottomEdge,
        LocalCenterFrontEdge,
        LocalCenterBackEdge
    };

    //edges are named in X,Y,Z order so TopFront means Y=Top, Z=Front
    // and TopLeft means Y=Top, X=Left etc.

    public Edge3D GetLocalEdgeTopFront()
    {
        return new Edge3D("TopFront", LeftTopFront, RightTopFront);
    }

    public Edge3D GetLocalEdgeTopBack()
    {
        return new Edge3D("TopBack", LeftTopBack, RightTopBack);
    }

    public Edge3D GetLocalEdgeTopLeft()
    {
        return new Edge3D("LeftTop", LeftTopFront, LeftTopBack);
    }

    public Edge3D GetLocalEdgeTopRight()
    {
        return new Edge3D("RightTop", RightTopFront, RightTopBack);
    }

    public Edge3D GetLocalEdgeBottomFront()
    {
        return new Edge3D("BottomFront", LeftBottomFront, RightBottomFront);
    }

    public Edge3D GetLocalEdgeBottomBack()
    {
        return new Edge3D("BottomBack", LeftBottomBack, RightBottomBack);
    }

    public Edge3D GetLocalEdgeBottomLeft()
    {
        return new Edge3D("LeftBottom", LeftBottomFront, LeftBottomBack);
    }

    public Edge3D GetLocalEdgeBottomRight()
    {
        return new Edge3D("RightBottom", RightBottomFront, RightBottomBack);
    }

    public Edge3D GetLocalEdgeFrontLeft()
    {
        return new Edge3D("LeftFront", LeftBottomFront, LeftTopFront);
    }

    public Edge3D GetLocalEdgeFrontRight()
    {
        return new Edge3D("RightFront", RightBottomFront, RightTopFront);
    }

    public Edge3D GetLocalEdgeBackLeft()
    {
        return new Edge3D("LeftBack", LeftTopBack, LeftBottomBack);
    }

    public Edge3D GetLocalEdgeBackRight()
    {
        return new Edge3D("RightBack", RightTopBack, RightBottomBack);
    }

    public List<Edge3D> GetLocalEdges()
    {
        return new List<Edge3D>
        {
            GetLocalEdgeTopFront(),
            GetLocalEdgeTopBack(),
            GetLocalEdgeTopLeft(),
            GetLocalEdgeTopRight(),
            GetLocalEdgeBottomFront(),
            GetLocalEdgeBottomBack(),
            GetLocalEdgeBottomLeft(),
            GetLocalEdgeBottomRight(),
            GetLocalEdgeFrontLeft(),
            GetLocalEdgeFrontRight(),
            GetLocalEdgeBackLeft(),
            GetLocalEdgeBackRight()
        };
    }

}


