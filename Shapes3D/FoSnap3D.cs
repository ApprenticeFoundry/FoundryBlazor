
using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;


namespace FoundryBlazor.Shape;

public class FoSnap3D : FoGlyph3D, ISnap3D
{


    public FoSnap3D() : base()
    {
    }
    public FoSnap3D(string name) : base(name)
    {
    }

    public FoSnap3D(string name, string color) : base(name, color)
    {
    }

    public override Vector3 GetPosition(int x = 0, int y = 0, int z = 0)
    {
        if (Position == null)
            return base.GetPosition(x, y, z);
        return Position;
    }

    public override Vector3 GetPivot(int x = 0, int y = 0, int z = 0)
    {
        if (Pivot == null)
            return base.GetPivot(x, y, z);
        return Pivot;
    }

    public override Euler GetRotation(int x = 0, int y = 0, int z = 0)
    {
        if (Rotation == null)
            return base.GetRotation(x, y, z);
        return Rotation;
    }


    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
  


        return true;
    }
}