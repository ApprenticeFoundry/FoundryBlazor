using BlazorThreeJS.Maths;
using IoBTMessage.Models;
using System.Runtime.CompilerServices;

namespace FoundryBlazor.Shape;

public static class FoVector3DExtensions
{



    public static Vector3 AsVector3(this FoVector3D obj)
    {
        return new Vector3(obj.X, obj.Y, obj.Z);
    }

    public static Euler AsEuler(this FoVector3D obj)
    {
        return new Euler() { X = obj.X, Y = obj.Y, Z = obj.Z, Order = "XYZ" };
    }


}
