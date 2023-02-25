
using BlazorThreeJS.Core;
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
using static System.Formats.Asn1.AsnWriter;

namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D
{

    public string Symbol { get; set; } = "";
    public string Type { get; set; } = "";
    public FoVector3D? Position { get; set; }
    public FoVector3D? Rotation { get; set; }
    public FoVector3D? Origin { get; set; }
    public FoVector3D? BoundingBox { get; set; }

    public Guid? PromiseGUID { get; set; }
    public Guid? LoadingGUID { get; set; }

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
        message.WriteLine();
        var label = new LabelText(message)
        {
            Color = "White",
            Position = GetPosition().AsVector3()
        };
        ctx.Add(label);
        return false;
    }

    public bool Loading(Scene ctx, string message)
    {
        Random rnd = new Random();
        int y = rnd.Next(-5, 7);
        var label = new LabelText(message)
        {
            Color = "Yellow",
            Position = new FoVector3D(-3, y, -2).AsVector3()
        };
        LoadingGUID = label.Uuid;
        ctx.Add(label);
        return true;
    }

    public bool Box(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        var mesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }

    private bool Cylinder(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        var mesh = new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: box.X / 2, radiusBottom: box.X / 2, height: box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }

    private bool Sphere(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new SphereGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }


    private bool Circle(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new CircleGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }

    private bool Capsule(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new CapsuleGeometry(radius: box.X / 2, box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }

    private bool Cone(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new ConeGeometry(radius: box.X / 2, height: box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }

    private bool Dodecahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }
 
     private bool Icosahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }  

      private bool Octahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new OctahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }  
      private bool Tetrahedron(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: box.X / 2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    }  
    private bool Plane(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new PlaneGeometry(width: box.X, height: box.Y),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
        return true;
    } 

    private bool Ring(Scene ctx)
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);

        var mesh = new Mesh
        {
            Geometry = new RingGeometry(innerRadius: box.X/2, outerRadius: box.Y/2),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
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
        if (!string.IsNullOrEmpty(Symbol)) return false;

        var url = Symbol.Replace("http", "https");

        var settings = new ImportSettings
        {
            Format = format,
            FileURL = url,
            Position = Position?.AsVector3() ?? new Vector3(),
            Rotation = Rotation?.AsEuler() ?? new Euler()
        };

        Task.Run(async () =>
        {
            $"PreRenderImport symbol [{url}] ".WriteLine();
            PromiseGUID = await viewer.Import3DModelAsync(settings);
            $"PreRenderImport guid [{PromiseGUID}] ".WriteLine();
            arena.Add<FoShape3D>(PromiseGUID.Value.ToString(), this);
        });
        return true;
    }

    private bool RenderImport(Scene ctx, Import3DFormats format)
    {
        if (!string.IsNullOrEmpty(Symbol)) return false;

        var url = Symbol.Replace("http", "https");
        var last = url.Split('/').Last();

        Loading(ctx, $"Loading... {last}");
        // Loading(ctx, $"PH");

        return true;
    }

    public override MeshStandardMaterial GetMaterial()
    {

        if (!string.IsNullOrEmpty(Symbol)) return base.GetMaterial();

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
            "Cylinder" => Cylinder(ctx),
            "Sphere" => Sphere(ctx),
            "Plane" => Plane(ctx),
            "Capsule" => Capsule(ctx),
            "Cone" => Cone(ctx),
            "Collada" => RenderImport(ctx, Import3DFormats.Collada),
            "Fbx" => RenderImport(ctx, Import3DFormats.Fbx),
            "Obj" => RenderImport(ctx, Import3DFormats.Obj),
            "Stl" => RenderImport(ctx, Import3DFormats.Stl),
            "Glb" => RenderImport(ctx, Import3DFormats.Gltf),
            _ => NotImplemented(ctx)
        };
        return result;
    }

    public override bool PostRender(Scene ctx, Guid guid)
    {
        //add code to remove the 'loading...'  and then 
        //resolve the guid that was the promise

        var result = Type switch
        {
            "Collada" => true,
            "Fbx" => true,
            "Obj" => true,
            "Stl" => true,
            "Glb" => true,
            _ => false
        };

        PromiseGUID = null;
        LoadingGUID = null;
        return result;
    }

}