using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Events;
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

public class FoShape3D : FoGlyph3D, IShape3D
{

    public string Symbol { get; set; } = "";
    public string Type { get; set; } = "";
    public Vector3? Position { get; set; }
    public Vector3? Pivot { get; set; }
    public Euler? Rotation { get; set; }
    public Vector3? BoundingBox { get; set; }
    public List<Vector3>? Path { get; set; }
    public string? LoadingURL { get; set; }
    public FoMenu3D? NavMenu { get; set; }
    public FoPanel3D? TextPanel { get; set; }
    private Mesh? ShapeMesh { get; set; }
    private Object3D? ShapeObject3D { get; set; }
    public FoShape3D() : base()
    {
    }
    public FoShape3D(string name) : base(name)
    {
    }
    public FoShape3D(string name, string color) : base(name, color)
    {
    }

    //https://BlazorThreeJS.com/reference/Index.html

    public override bool UpdateMeshPosition(double xLoc, double yLoc, double zLoc)
    {
        //"Update mesh position".WriteSuccess();
        if (ShapeMesh != null)
        {
            ShapeMesh.Position.Set(xLoc, yLoc, zLoc);
            return true;
        }
        else if (ShapeObject3D != null)
        {
            //$"ShapeObject3D Update mesh position {xLoc}, {yLoc}, {zLoc}".WriteSuccess();
            ShapeObject3D.Position.Set(xLoc, yLoc, zLoc);
            return true;
        }

        return false;
    }

    public FoShape3D CreateBox(string name, double width, double height, double depth)
    {
        Type = "Box";
        BoundingBox = new Vector3(width, height, depth);
        Name = name;
        return this;
    }
    public FoShape3D CreateCylinder(string name, double width, double height, double depth)
    {
        Type = "Cylinder";
        BoundingBox = new Vector3(width, height, depth);
        Name = name;
        return this;
    }
    public FoShape3D CreateTube(string name, double radius, List<Vector3> path)
    {
        Type = "Tube";
        BoundingBox = new Vector3(radius, 0, 0);
        Name = name;
        Path = path;
        return this;
    }

    public FoShape3D CreateGlb(string url, double width, double height, double depth)
    {
        Type = "Glb";
        BoundingBox = new Vector3(width, height, depth);
        Symbol = url;
        $"CreateGlb symbol [{Symbol}] ".WriteSuccess();
        return this;
    }

    public FoShape3D CreateSphere(string name, double width, double height, double depth)
    {
        Type = "Sphere";
        BoundingBox = new Vector3(width, height, depth);
        Name = name;
        return this;
    }

    private bool NotImplemented(Scene ctx)
    {
        // var message = $"symbol [{Symbol}] Body type [{Type}] NotImplemented";
        // message.WriteError();
        // var label = new LabelText(message)
        // {
        //     Color = "White",
        //     Position = GetPosition().AsVector3()
        // };
        // ctx.Add(label);
        return false;
    }



    public bool Box(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    public bool Boundary(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetWireframe()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Cylinder(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: box.X / 2, radiusBottom: box.X / 2, height: box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Sphere(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new SphereGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Tube(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);


        ShapeMesh = new Mesh
        {
            Geometry = new TubeGeometry(radius: box.X / 2, path: Path!, 8, 10),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }
    private bool Circle(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new CircleGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Capsule(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new CapsuleGeometry(radius: box.X / 2, box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Cone(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new ConeGeometry(radius: box.X / 2, height: box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Dodecahedron(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Icosahedron(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Octahedron(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new OctahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }
    private bool Tetrahedron(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }
    private bool Plane(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new PlaneGeometry(width: box.X, height: box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Ring(Scene ctx)
    {
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new RingGeometry(innerRadius: box.X / 2, outerRadius: box.Y / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    public static void FillScene(Scene scene)
    {

        scene.Add(new Mesh
        {
            Geometry = new TorusGeometry(radius: 0.6f, tube: 0.4f, radialSegments: 12, tubularSegments: 12),
            Position = new Vector3(4, 0, -4),
            Rotation = new Euler(0, 0, 0),
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

    private async Task<bool> PreRenderImport(FoArena3D arena, Viewer viewer, Import3DFormats format)
    {
        var settings = AsImportSettings(format);

        if (string.IsNullOrEmpty(LoadingURL)) return false;
        $"PreRenderImport symbol [{LoadingURL}] ".WriteInfo(1);

        await viewer.Request3DModel(settings);
        arena.Add<FoShape3D>(settings.Uuid.ToString(), this);
        return true;
    }

    public ImportSettings AsImportSettings(Import3DFormats format)
    {
        LoadingURL = Symbol;

        var setting = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = format,
            FileURL = LoadingURL,
            Position = GetPosition(),
            Rotation = GetRotation(),
            Pivot = GetPivot(),
            OnComplete = (Scene scene, Object3D object3D) =>
            {
                $"OnComplete for object3D.Uuid={object3D.Uuid}, body.LoadingURL={LoadingURL}, position.x={Position?.X}".WriteInfo();
                if (object3D != null)
                    ShapeObject3D = object3D;
                else
                    "Unexpected empty object3D".WriteError(1);
            }
        };
        GlyphId = setting.Uuid.ToString();
        return setting;
    }

    public static async Task<bool> PreRenderClones(List<FoShape3D> bodies, FoArena3D arena, Viewer viewer, Import3DFormats format)
    {
        var settings = new List<ImportSettings>();

        foreach (var body in bodies)
        {
            var setting = body.AsImportSettings(format);
            arena.Add<FoShape3D>(body.GetGlyphId(), body);
            settings.Add(setting);

            $"AsImportSettings body.Symbol {body.Symbol} X = {setting.FileURL}".WriteSuccess();

        }

        var source = settings.ElementAt(0);
        settings.RemoveAt(0);

        var sourceBody = bodies.ElementAt(0);
        bodies.RemoveAt(0);

        source.OnComplete = async (Scene scene, Object3D object3D) =>
        {
            if (object3D != null)
            {
                sourceBody.ShapeObject3D = object3D;
                if (settings.Count > 0)
                    await viewer.Clone3DModel(object3D.Uuid, settings);
            }
            else
                "Unexpected empty object3D".WriteError(1);
        };

        await viewer.Request3DModel(source);
        return true;
    }


    //private bool RenderImportPromise(Scene scene, Import3DFormats format)
    //{
    //    if (string.IsNullOrEmpty(LoadingURL)) return false;

    //    // var message = $"{format} Loading... {LoadingURL}";
    //    var message = $"{format} Loading... {LoadingURL}";
    //    message.WriteInfo(1);

    //    Random rnd = new();
    //    int y = rnd.Next(-5, 7);
    //    var label = new LabelText(message)
    //    {
    //        Color = "Yellow",
    //        Position = new Vector3(-3, y, -2)
    //    };

    //    LoadingGUID = label.Uuid;

    //    scene.Add(label);
    //    return true;
    //}


    public MeshStandardMaterial GetWireframe()
    {
        var result = new MeshStandardMaterial()
        {
            Color = this.Color,
            Wireframe = true
        };
        return result;
    }

    public override MeshStandardMaterial GetMaterial()
    {
        if (!string.IsNullOrEmpty(Symbol))
            return base.GetMaterial();

        var result = new MeshStandardMaterial()
        {
            Color = this.Color,
            //Wireframe = true
        };
        return result;
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

    public override async Task<bool> PreRender(FoArena3D arena, Viewer viewer, bool deep = true)
    {
        //is symbol ends with ....
        //LoadingURL = Symbol.Replace("http:", "https:");
        //await Task.CompletedTask;

        LoadingURL = Symbol;
        var result = Type switch
        {
            "Collada" => await PreRenderImport(arena, viewer, Import3DFormats.Collada),
            "Fbx" => await PreRenderImport(arena, viewer, Import3DFormats.Fbx),
            "Obj" => await PreRenderImport(arena, viewer, Import3DFormats.Obj),
            "Stl" => await PreRenderImport(arena, viewer, Import3DFormats.Stl),
            "Glb" => await PreRenderImport(arena, viewer, Import3DFormats.Gltf),
            _ => false
        };
        return result;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {

        var result = Type switch
        {
            "Box" => Box(ctx),
            "Boundary" => Boundary(ctx),
            "Circle" => Circle(ctx),
            "Cylinder" => Cylinder(ctx),
            "Sphere" => Sphere(ctx),
            "Plane" => Plane(ctx),
            "Capsule" => Capsule(ctx),
            "Cone" => Cone(ctx),
            "Tube" => Tube(ctx),
            _ => NotImplemented(ctx)
        };
        return result;
    }
}