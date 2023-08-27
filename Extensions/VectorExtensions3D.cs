

using BlazorThreeJS.Maths;
using FoundryRulesAndUnits.Models;


namespace FoundryBlazor.Extensions;

public static class Vector3Extensions
{
    //BoundingBox


    public static Vector3 BoxAsVector3(this BoundingBox box)
    {
        return new Vector3(box.width.Value(), box.height.Value(), box.depth.Value());
    }

    public static Vector3 BoxAsVector3(this UDTO_BoundingBox box)
    {
        return new Vector3(box.width, box.height, box.depth);
    }

    public static Vector3 PinAsVector3(this BoundingBox box)
    {
        return new Vector3(box.pinX.Value(), box.pinY.Value(), box.pinZ.Value());
    }

    public static Vector3 PinAsVector3(this UDTO_BoundingBox box)
    {
        return new Vector3(box.pinX, box.pinY, box.pinZ);
    }

    public static Vector3 ScaleAsVector3(this BoundingBox box)
    {
        return new Vector3(box.scaleX, box.scaleY, box.scaleZ);
    }

    public static Vector3 ScaleAsVector3(this UDTO_BoundingBox box)
    {
        return new Vector3(box.scaleX, box.scaleY, box.scaleZ);
    }

    //HighResPosition 

    public static Vector3 LocAsVector3(this HighResPosition pos)
    {
        return new Vector3(pos.xLoc.Value(), pos.yLoc.Value(), pos.zLoc.Value());
    }

    public static Vector3 LocAsVector3(this UDTO_HighResPosition pos)
    {
        return new Vector3(pos.xLoc, pos.yLoc, pos.zLoc);
    }

    public static Euler AngAsVector3(this HighResPosition pos)
    {
        return new Euler() {
            X = pos.xAng.Value(), 
            Y = pos.yAng.Value(), 
            Z = pos.zAng.Value(),
            Order = "XYZ"
        };
    }

    public static Euler AngAsVector3(this UDTO_HighResPosition pos)
    {
        return new Euler()
        {
            X = pos.xAng,
            Y = pos.yAng,
            Z = pos.zAng,
            Order = "XYZ"
        };
    }
    //public static Vector3 AsVector3(this FoVector3D obj)
    //{
    //    return new Vector3(obj.X, obj.Y, obj.Z);
    //}

    //public static Euler AsEuler(this FoVector3D obj)
    //{
    //    return new Euler() { X = obj.X, Y = obj.Y, Z = obj.Z, Order = "XYZ" };
    //}


}
