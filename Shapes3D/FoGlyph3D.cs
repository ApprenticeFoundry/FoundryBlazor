using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Lights;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoGlyph3D : FoComponent
{
    public string UniqueGuid { get; set; } = "";
    public string PlatformName { get; set; } = "";
    public float Opacity { get; set; } = 1.0F;
    public string Color { get; set; } = "Green";

    public FoGlyph3D() : base("")
    {
    }
    public FoGlyph3D(string name) : base(name)
    {
    }
    public FoGlyph3D(string name, string color) : base(name)
    {
        Color = color;
    }

    public bool IsSamePlatform(FoGlyph3D obj)
    {
        return PlatformName.Matches(obj.PlatformName);
    }

    public virtual void PreRender(Viewer viewer, bool deep = true)
    {
    }

    public virtual void Render(Scene ctx, int tick, double fps, bool deep = true)
    {
    }


    public virtual MeshStandardMaterial GetMaterial()
    {
        var result = new MeshStandardMaterial()
        {
            Color = this.Color
        };
        return result;
    }

    public virtual BufferGeometry GetGeometry()
    {
        var result = new BoxGeometry(1F, 2F, 3F);
        return result;
    }

    public virtual FoVector3D GetPosition()
    {
        var result = new FoVector3D(0, 0, 0);
        return result;
    }

}