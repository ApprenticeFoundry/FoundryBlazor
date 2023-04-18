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
    public FoVector3D? Position { get; set; }
    public FoVector3D? Rotation { get; set; }
    public FoVector3D? Origin { get; set; }
    public FoVector3D? BoundingBox { get; set; }
    private Guid? LoadingGUID { get; set; }
    public string? LoadingURL { get; set; }

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
            ShapeMesh.Position.Loc(xLoc, yLoc, zLoc);
            return true;
        }
        else if (ShapeObject3D != null)
        {
            //$"ShapeObject3D Update mesh position {xLoc}, {yLoc}, {zLoc}".WriteSuccess();
            ShapeObject3D.Position.Loc(xLoc, yLoc, zLoc);
            return true;
        }

        return false;
    }

    public FoShape3D CreateBox(string name, double width, double height, double depth, string units = "m")
    {
        Type = "Box";
        BoundingBox = new FoVector3D(width, height, depth)
        {
            units = units
        };
        Name = name;
        return this;
    }
    public FoShape3D CreateCylinder(string name, double width, double height, double depth, string units = "m")
    {
        Type = "Cylinder";
        BoundingBox = new FoVector3D(width, height, depth)
        {
            units = units
        };
        Name = name;
        return this;
    }

    public FoShape3D CreateGlb(string url, double width, double height, double depth, string units = "m")
    {
        Type = "Glb";
        BoundingBox = new FoVector3D(width, height, depth)
        {
            units = units
        };
        Symbol = url;
        $"CreateGlb symbol [{Symbol}] ".WriteSuccess();
        return this;
    }

    public FoShape3D CreateSphere(string name, double width, double height, double depth, string units = "m")
    {
        Type = "Sphere";
        BoundingBox = new FoVector3D(width, height, depth)
        {
            units = units
        };
        Name = name;
        return this;
    }

    private bool NotImplemented(Scene ctx)
    {
        var message = $"symbol [{Symbol}] Body type [{Type}] NotImplemented";
        message.WriteError();
        var label = new LabelText(message)
        {
            Color = "White",
            Position = GetPosition().AsVector3()
        };
        ctx.Add(label);
        return false;
    }



    public bool Box(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    public bool Boundary(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition().AsVector3(),
            Material = GetWireframe()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Cylinder(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: box.X / 2, radiusBottom: box.X / 2, height: box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Sphere(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new SphereGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }


    private bool Circle(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new CircleGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Capsule(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new CapsuleGeometry(radius: box.X / 2, box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Cone(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new ConeGeometry(radius: box.X / 2, height: box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Dodecahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Icosahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Octahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new OctahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }
    private bool Tetrahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }
    private bool Plane(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new PlaneGeometry(width: box.X, height: box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(ShapeMesh);
        return true;
    }

    private bool Ring(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new RingGeometry(innerRadius: box.X / 2, outerRadius: box.Y / 2),
            Position = GetPosition().AsVector3(),
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

    private bool PreRenderImport(FoArena3D arena, Viewer viewer, Import3DFormats format)
    {
        if (string.IsNullOrEmpty(LoadingURL)) return false;
        $"PreRenderImport symbol [{LoadingURL}] ".WriteInfo(1);

        var settings = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = format,
            FileURL = LoadingURL,
            Position = Position?.AsVector3() ?? new Vector3(),
            Rotation = Rotation?.AsEuler() ?? new Euler(),
            OnComplete = async (Scene scene, Object3D object3D) =>
            {
                if (object3D != null) ShapeObject3D = object3D;
                else "Unexpected empty object3D".WriteError(1);

                if (LoadingGUID != null) await viewer.RemoveByUuidAsync((Guid)(LoadingGUID));
            }
        };

        Task.Run(async () =>
        {
            await viewer.Request3DModel(settings);
            arena.Add<FoShape3D>(settings.Uuid.ToString(), this);
        });
        return true;
    }


    private bool RenderImportPromise(Scene scene, Import3DFormats format)
    {
        if (string.IsNullOrEmpty(LoadingURL)) return false;

        var message = $"{format} Loading... {LoadingURL}";
        message.WriteInfo(1);

        Random rnd = new();
        int y = rnd.Next(-5, 7);
        var label = new LabelText(message)
        {
            Color = "Yellow",
            Position = new FoVector3D(-3, y, -2).AsVector3()
        };

        LoadingGUID = label.Uuid;

        scene.Add(label);
        return true;
    }


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


    public override FoVector3D GetPosition()
    {
        if (Position == null) return base.GetPosition();
        return Position;
    }



    public override bool PreRender(FoArena3D arena, Viewer viewer, bool deep = true)
    {
        //is symbol ends with ....
        //LoadingURL = Symbol.Replace("http:", "https:");
        LoadingURL = Symbol;
        var result = Type switch
        {
            "Collada" => PreRenderImport(arena, viewer, Import3DFormats.Collada),
            "Fbx" => PreRenderImport(arena, viewer, Import3DFormats.Fbx),
            "Obj" => PreRenderImport(arena, viewer, Import3DFormats.Obj),
            "Stl" => PreRenderImport(arena, viewer, Import3DFormats.Stl),
            "Glb" => PreRenderImport(arena, viewer, Import3DFormats.Gltf),
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
            "Collada" => RenderImportPromise(ctx, Import3DFormats.Collada),
            "Fbx" => RenderImportPromise(ctx, Import3DFormats.Fbx),
            "Obj" => RenderImportPromise(ctx, Import3DFormats.Obj),
            "Stl" => RenderImportPromise(ctx, Import3DFormats.Stl),
            "Glb" => RenderImportPromise(ctx, Import3DFormats.Gltf),
            _ => NotImplemented(ctx)
        };
        return result;
    }
}