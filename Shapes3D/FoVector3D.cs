using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoundryBlazor.Shape;

public class FoVector3D
{
    public string units = "m";
    public double X = 0;
    public double Y = 0;
    public double Z = 0;

    public FoVector3D()
    {
    }
    public FoVector3D(double xLoc, double yLoc, double zLoc)
    {
        this.X = xLoc;
        this.Y = yLoc;
        this.Z = zLoc;
    }

    public double distanceXZ()
    {
        return Math.Sqrt(this.X * this.X + this.Z * this.Z);
    }

    public double bearingXZ()
    {
        return Math.Atan2(this.X, this.Z);
    }


    public FoVector3D copyFrom(FoVector3D pos)
    {
        this.units = pos.units;
        this.X = pos.X;
        this.Y = pos.Y;
        this.Z = pos.Z;

        return this;
    }
    public FoVector3D Loc(double xLoc, double yLoc, double zLoc, string units = "m")
    {
        this.units = units;
        this.X = xLoc;
        this.Y = yLoc;
        this.Z = zLoc;
        return this;
    }
}
