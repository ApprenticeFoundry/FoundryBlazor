using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Lights;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;

namespace FoundryBlazor.Shape;

public class FoGlyph3D : FoComponent
{
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

    public virtual void Render(Viewer viewer, Scene ctx, int tick, double fps, bool deep = true)
    {
        var mesh = GetMesh(viewer);
        ctx.Add(mesh);
    }


    public virtual MeshStandardMaterial GetMaterial()
    {
        var result = new MeshStandardMaterial()
        {
            Color = this.Color
        };
        return result;
    }

    public virtual BufferGeometry GetGeometry(Viewer viewer)
    {
        var result = new BoxGeometry(1F, 2F, 3F);
        return result;
    }

    public virtual Vector3 GetPosition()
    {
        var result = new Vector3(0F, 0F, 0F);
        return result;
    }

    public virtual Mesh GetMesh(Viewer viewer)
    {
        var result = new Mesh
        {
            Geometry = GetGeometry(viewer),
            Position = GetPosition(),
            Material = GetMaterial()
        };
        return result;
    }
}