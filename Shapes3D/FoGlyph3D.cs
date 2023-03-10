using BlazorThreeJS.Materials;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
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

    public virtual MeshStandardMaterial GetMaterial()
    {
        var result = new MeshStandardMaterial()
        {
            Color = this.Color
        };
        return result;
    }


    public virtual FoVector3D GetPosition()
    {
        var result = new FoVector3D(0, 0, 0);
        return result;
    }


    public virtual bool PreRender(FoArena3D arena, Viewer viewer, bool deep = true)
    {
        return false;
    }

    public virtual bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        return false;
    }
    public virtual bool PostRender(Scene ctx, Guid guid)
    {
        return false;
    }



}