using BlazorThreeJS.Materials;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoGlyph3D : FoComponent
{
    public string GlyphId { get; set; } = "";
    public string PlatformName { get; set; } = "";
    public float Opacity { get; set; } = 1.0F;
    public string Color { get; set; } = "Green";

    protected int width = 0;
    public int Width { get { return this.width; } set { this.width = AssignInt(value, width); } }
    protected int height = 0;
    public int Height { get { return this.height; } set { this.height = AssignInt(value, height); } }
    protected int depth = 0;
    public int Depth { get { return this.depth; } set { this.depth = AssignInt(value, depth); } }

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

    public FoGlyph3D SetBoundry(int width, int height, int depth)
    {
        (Width, Height, Depth) = (width, height, depth);
        return this;
    }

    protected int AssignInt(int newValue, int oldValue)
    {
        if ( Math.Abs(newValue - oldValue) > 0)
            Smash(true);

        return newValue;
    }


    public virtual bool Smash(bool force)
    {
        //if ( _matrix == null && !force) return false;
        $"Smashing {Name} {GetType().Name}".WriteInfo(2);

        // ResetHitTesting = true;
        // this._matrix = null;
        // this._invMatrix = null;

        return true;
    }

    public Action<FoGlyph3D, int>? ContextLink;

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

    public virtual bool UpdateMeshPosition(double xLoc, double yLoc, double zLoc)
    {
        return false;
    }

    public FoGlyph3D MoveTo(int x, int y, int z) 
    {
        var pos = GetPosition();
        pos.Loc(x, y, z);
        return this; 
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