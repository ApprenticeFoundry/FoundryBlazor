using BlazorThreeJS.Geometires;
using BlazorThreeJS.Lights;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using FoundryRulesAndUnits;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;

public interface IStage
{
    FoStage3D ClearAll();
    Scene SetScene(Scene scene, Viewer viewer);
    V AddShape<V>(V shape) where V : FoGlyph3D;
    T RemoveShape<T>(T value) where T : FoGlyph3D;
}

public class FoStage3D : FoGlyph3D, IStage
{
    public static bool RefreshMenus { get; set; } = true;
    public bool IsActive { get; set; } = false;
    public bool IsDirty { get; set; } = false;  // dirty TRUE means we need to update the scene after render
    public double StageMargin { get; set; } = .50;  //meters
    public double StageWidth { get; set; } = 30.0;  //meters
    public double StageHeight { get; set; } = 30.0;  //meters
    public double StageDepth { get; set; } = 30.0;  //meters


    private Scene? CurrentScene { get; set; }
    private Viewer? CurrentViewer { get; set; }
    private Mesh? ShapeMesh { get; set; }



    protected FoCollection<FoGlyph3D> Pipes3D = new();
    protected FoCollection<FoGlyph3D> Shapes3D = new();

    public FoStage3D() : base()
    {
    }

    public FoStage3D(string name) : base(name)
    {
    }
    public FoStage3D(string name, string color) : base(name, color)
    {
    }


    public FoStage3D(string name, int width, int height, int depth, string color) : base(name, color)
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
        SetBoundry(width, height, depth);
    }


    public Scene SetScene(Scene scene, Viewer viewer)
    {
        CurrentScene = scene;
        CurrentViewer = viewer;
        return CurrentScene;
    }


    public Scene InitScene(Scene scene, Viewer viewer)
    {
        scene.Add(new AmbientLight());
        scene.Add(new PointLight()
        {
            Position = new Vector3(1, 3, 0)
        });

        return SetScene(scene,viewer);
    }

    public FoStage3D ClearAll()
    {
        Shapes3D.Clear();
        Pipes3D.Clear();
        return this;
    }

    public bool EstablishBoundry()
    {
        if (ShapeMesh != null) return false;


        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(Width, Height, Depth),
            Position = new Vector3(0, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "red",
                Wireframe = true
            }
        };

        CurrentScene?.Add(ShapeMesh);


        //$"EstablishBoundry {Width} {Height} {Depth}".WriteSuccess();
        return true;
    }

    public T AddShape<T>(T value) where T : FoGlyph3D
    {

        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Key))
        {
            value.Key = collection.NextItemName();
        }

        collection.AddObject(value.Key, value);

        if (value is IShape3D)
        {
            Shapes3D.Add(value);
            $"IShape3D Added {value.Key}".WriteSuccess();
        }
        else if (value is IPipe3D)
        {
            Pipes3D.Add(value);
            //$"IPipe3D Added {value.Name}".WriteSuccess();
        }

        if (CurrentScene != null)
        {

            value.Render(CurrentScene, 0, 0);
            //IsDirty = true;
            //FillStage();
        }

        return value;
    }

    public T RemoveShape<T>(T value) where T : FoGlyph3D
    {

        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Key))
        {
            value.Key = collection.NextItemName();
        }

        collection.RemoveObject(value.Key);

        if (value is IShape3D)
        {
            Shapes3D.Remove(value);
            $"IShape3D Added {value.Key}".WriteSuccess();
        }
        else if (value is IPipe3D)
        {
            Pipes3D.Remove(value);
            //$"IPipe3D Added {value.Name}".WriteSuccess();
        }

        return value;
    }
    public async Task RenderDetailed(Scene scene, int tick, double fps)
    {
        //$"RenderDetailed {tick} {Shapes3D.Count()}".WriteInfo();
        Shapes3D?.ForEach(shape => shape.ContextLink?.Invoke(shape, tick));
        await Task.CompletedTask;
    }
    // public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    // {
    //     $"Render {tick} {Shapes3D.Count()}".WriteInfo();
    //     Shapes3D?.ForEach(shape => shape.ContextLink?.Invoke(shape,tick));
    //     return true;
    // }


    private void FillStage()
    {
        if (CurrentScene == null) return;

        $"FillStage {CurrentScene.Name}".WriteSuccess();

        //IsDirty = true;
        var scene = CurrentScene;
        scene.Add(new AmbientLight());
        scene.Add(new PointLight()
        {
            Position = new Vector3(1, 3, 0)
        });
        scene.Add(new Mesh());
        scene.Add(new Mesh
        {
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(-2, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "magenta"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CircleGeometry(radius: 0.75f, segments: 12),
            Position = new Vector3(2, 0, 0),
            Scale = new Vector3(1, 0.75f, 1),
            Material = new MeshStandardMaterial()
            {
                Color = "#98AFC7"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CapsuleGeometry(radius: 0.5f, length: 2),
            Position = new Vector3(-4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Position = new Vector3(4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "green",
                FlatShading = true,
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: 0.5f, height: 1.2f, radialSegments: 16),
            Position = new Vector3(0, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "red",
                Wireframe = true
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: 0.8f),
            Position = new Vector3(-2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "darkviolet",
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: 0.8f),
            Position = new Vector3(-4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "violet"
            }
        });

        scene.Add(new Mesh
        {

            Geometry = new OctahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "aqua"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new PlaneGeometry(width: 0.5f, height: 2),
            Position = new Vector3(4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "purple"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new RingGeometry(innerRadius: 0.6f, outerRadius: 0.7f),
            Position = new Vector3(0, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "DodgerBlue"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new SphereGeometry(radius: 0.6f),
            Position = new Vector3(-2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            },
        });
        scene.Add(new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightblue"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new TorusGeometry(radius: 0.6f, tube: 0.4f, radialSegments: 12, tubularSegments: 12),
            Position = new Vector3(4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightgreen"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new TorusKnotGeometry(radius: 0.6f, tube: 0.1f),
            Position = new Vector3(-4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "RosyBrown"
            }
        });
    }


}