using BlazorThreeJS.Core;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using System.Text.Json.Serialization;


namespace FoundryBlazor.Shape;

public class FoGlyph3D : FoComponent
{
    public string GlyphId { get; set; } = "";
    //public float Opacity { get; set; } = 1.0F;

    public string GeomType { get; set; } = "";
    public List<Vector3>? Path { get; set; }
    public Action<FoGlyph3D, int>? ContextLink;

    public string Color { get; set; } = "Green";


    public Vector3? Position { get; set; }
    public Vector3? Pivot { get; set; }
    public Euler? Rotation { get; set; } // replace with Quaternion
    public Vector3? Scale { get; set; }

    public FoGeometryComponent3D GeometryParameter3D { get; set; }

    private Vector3 bounds = null!;
    public Vector3 BoundingBox 
    { 
        get 
        { 
            bounds ??= new Vector3(Width, Height, Depth);
            return bounds; 
        }
        set { bounds = value; }
    }

    public double radius { get; set; } = 0.025;
    public double Radius { get { return this.radius; } set { this.radius = AssignDouble(value, radius); } }
    protected double width = 0;
    public double Width { get { return this.width; } set { this.width = AssignDouble(value, width); } }
    protected double height = 0;
    public double Height { get { return this.height; } set { this.height = AssignDouble(value, height); } }
    protected double depth = 0;
    public double Depth { get { return this.depth; } set { this.depth = AssignDouble(value, depth); } }
    public bool IsVisible
    {
        get { return this.StatusBits.IsVisible; }
        set { this.StatusBits.IsVisible = value; }
    }

    [JsonIgnore]
    public Action<FoGlyph3D>? OnDelete { get; set; }

    public List<TreeNodeAction> DefaultActions = [];
 
    public FoGlyph3D() : base("")
    {
        GeometryParameter3D = new FoGeometryComponent3D(this);
    }
    public FoGlyph3D(string name) : base(name)
    {
         GeometryParameter3D = new FoGeometryComponent3D(this);
    }
    public FoGlyph3D(string name, string color) : this(name)
    {
        Color = color;
    }


    public string GetGlyphId()
    {
        if (string.IsNullOrEmpty(GlyphId))
            GlyphId = Guid.NewGuid().ToString();

        return GlyphId;
    }

    public bool GlyphIdCompare(string other)
    {
        var id = GetGlyphId();
        var result = id == other;
        // $"GlyphIdCompare {result}  {id} {other}".WriteNote();
        return result;
    }

   public void AddAction(string name, string color, Action action)
    {
        DefaultActions.AddAction(name, color, action);
    }

    public override IEnumerable<TreeNodeAction> GetTreeNodeActions()
    {
        var result = new List<TreeNodeAction>();
        result.AddRange(DefaultActions);
        
        if ( OnDelete != null )
            result.AddAction("Delete", "btn-danger", () =>
            {
                OnDelete?.Invoke(this);
            });

        return result;
    }



    public virtual void DeleteFromStage(FoStage3D stage, Scene scene)
    {
        $"Deleting {GetTreeNodeTitle()}".WriteWarning();

        var mesh = GeometryParameter3D.GetValue3D();
        if (mesh != null)
            scene.RemoveChild(mesh);

        stage.RemoveShape<FoGlyph3D>(this);
    }





    public FoGlyph3D SetBoundry(int width, int height, int depth)
    {
        (Width, Height, Depth) = (width, height, depth);
        return this;
    }

    protected double AssignDouble(double newValue, double oldValue)
    {
        if (Math.Abs(newValue - oldValue) > 0)
            Smash(true);

        return newValue;
    }



    public virtual bool Smash(bool force)
    {
        //if ( _matrix == null && !force) return false;
        //$"Smashing {Name} {GetType().Name}".WriteInfo(2);

        // ResetHitTesting = true;
        // this._matrix = null;
        // this._invMatrix = null;

        return true;
    }



    public string GetName()
    {
        return Key;
    }

    public virtual MeshStandardMaterial GetMaterial()
    {
        var result = new MeshStandardMaterial()
        {
            Color = this.Color,
            //Opacity = this.Opacity,
        };
        return result;
    }

    public virtual bool UpdateMeshPosition(double xLoc, double yLoc, double zLoc)
    {
        return false;
    }

    //public virtual async Task Draw(Scene ctx, int tick)
    //{
    //    //await ctx.SaveAsync();
    //    ShapeDraw?.Invoke(ctx, this);
    //    //await ctx.RestoreAsync();
    //    //await DrawPin(ctx);
    //    await Task.CompletedTask;
    //}

    public FoGlyph3D MoveTo(int x, int y, int z)
    {
        if ( Position == null )
            Position = new Vector3(x, y, z);
        else
            Position.Set(x, y, z);
        return this;
    }
    public virtual Vector3 GetPosition(int x = 0, int y = 0, int z = 0)
    {
        Position ??= new Vector3(x, y, z);
        return Position;
    }

    public virtual Vector3 GetPivot(int x = 0, int y = 0, int z = 0)
    {
        Pivot ??= new Vector3(x, y, z);
        return Pivot;
    }

    public virtual Vector3 GetScale(double x = 1, double y = 1, double z = 1)
    {
        Scale ??= new Vector3(x, y, z);
        return Scale;
    }

    public virtual Euler GetRotation(int x = 0, int y = 0, int z = 0)
    {
        Rotation ??= new Euler(x, y, z);
        return Rotation;
    }

    public virtual bool OnModelLoadComplete(Guid PromiseGuid)
    {
        return false;
    }
    public virtual async Task<bool> PreRender(FoArena3D arena, bool deep = true)
    {
        await Task.CompletedTask;
        return GeometryParameter3D.HasValue3D;
    }

    public virtual FoGeometryComponent3D RenderPrimitives(Scene scene)
    {
        return GeometryParameter3D;
    }

    public virtual bool Render(Scene scene, int tick, double fps, bool deep = true)
    {
        scene.ForceSceneRefresh();
        return false;
    }
    public virtual async Task<bool> RemoveFromRender(Scene scene, bool deep = true)
    {
        await scene.ClearScene();
        return false;
    }




}