using BlazorThreeJS.Enums;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Labels;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public interface IScene
{
    FoScene3D ClearAll();
    Scene GetScene();
    ViewerSettings GetSettings();
    V AddShape<V>(V shape) where V : FoGlyph3D;


}

public class FoScene3D : FoGlyph3D, IScene
{
    public bool IsActive { get; set; } = false;

    public double PageMargin { get; set; } = .50;  //inches
    public double PageWidth { get; set; } = 10.0;  //inches
    public double PageHeight { get; set; } = 4.0;  //inches
    public double PageDepth { get; set; } = 4.0;  //inches
    protected IScaledArena? _ScaledDArena;

    private Scene? Scene { get; set; }
    private Mesh? ShapeMesh { get; set; }

    public ViewerSettings settings = new()
    {
        CanSelect = true,// default is false
        SelectedColor = "black",
        WebGLRendererSettings = new WebGLRendererSettings
        {
            Antialias = false // if you need poor quality for some reasons
        }
    };

    protected FoCollection<FoGlyph3D> Pipes3D = new();
    protected FoCollection<FoGlyph3D> Shapes3D = new();

    public FoScene3D(): base()
    {
    }

    public FoScene3D(string name) : base(name)
    {
    }
    public FoScene3D(string name, string color) : base(name, color)
    {
    }


    public FoScene3D(string name, int width, int height, int depth, string color) : base(name, color)
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
        SetBoundry(width, height, depth);
    }



    public int DrawingWidth()
    {
        return _ScaledDArena?.ToPixels(PageWidth) ?? 0;
    }
    public int DrawingHeight()
    {
        return _ScaledDArena?.ToPixels(PageHeight) ?? 0;
    }
    public int DrawingDepth()
    {
        return _ScaledDArena?.ToPixels(PageDepth) ?? 0;
    }
    public int DrawingMargin()
    {
        return _ScaledDArena?.ToPixels(PageMargin) ?? 0;  //margin all around
    }

    public virtual void SetScaledArena(IScaledArena scaledArena)
    {
        _ScaledDArena = scaledArena;
        scaledArena.SetPageDefaults(this);
    }

    public Scene GetScene()
    {
        if ( Scene == null)
        {
            Scene = new();
            ShapeMesh = null;
            ClearAll();
        }
        return Scene;
    }

    public FoScene3D ClearAll()
    {
        Shapes3D.Clear();
        Pipes3D.Clear();
        return this;
    }

    public bool EstablishBoundry()
    {
        $"try EstablishBoundry".WriteSuccess();
        if ( ShapeMesh != null) return false;
        
        Width = DrawingWidth();
        Height = DrawingHeight();
        Depth = DrawingDepth();
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(Width, Height, Depth),
            Position = new Vector3(0, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "red",
                //Wireframe = true
            }
        };

        Scene ??= new();
        Scene.Add(ShapeMesh);
        
        $"EstablishBoundry {Width} {Height} {Depth}".WriteSuccess();
        return true;
    }
    public ViewerSettings GetSettings()
    {
        return settings;
    }
    public T AddShape<T>(T value) where T : FoGlyph3D
    {

        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Name))
        {
            value.Name = collection.NextItemName();
        }

        collection.AddObject(value.Name, value);

        if ( value is IShape3D)
        {
            Shapes3D.Add(value);   
            //$"IShape3D Added {value.Name}".WriteSuccess();
        }
        else if ( value is IPipe3D)
        {
            Pipes3D.Add(value);
            //$"IPipe3D Added {value.Name}".WriteSuccess();
        }

        var scene = GetScene();
        value.Render(scene, 0, 0);

        return value;
    }


    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        //$"Render {tick} {Shapes3D.Count()}".WriteInfo();
        Shapes3D?.ForEach(shape => shape.ContextLink?.Invoke(shape,tick));
        return true;
    }


}