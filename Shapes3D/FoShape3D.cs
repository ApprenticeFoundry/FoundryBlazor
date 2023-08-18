
using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using IoBTMessage.Models;
using Radzen.Blazor.Rendering;

namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D, IShape3D
{

    public string Symbol { get; set; } = "";
    public string Type { get; set; } = "";
    public List<DT_Target>? Targets { get; set; }

    public Vector3? Position { get; set; }
    public List<Vector3>? Path { get; set; }
    public Vector3? Pivot { get; set; }
    public Euler? Rotation { get; set; } // replace with Quaternion
    public Vector3? BoundingBox { get; set; }
    public Vector3? Scale { get; set; }
    public string? LoadingURL { get; set; }

    public List<FoPanel3D>? TextPanels { get; set; }
    public Action<ImportSettings> UserHit { get; set; } = (ImportSettings model3D) => { };

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



    public Mesh Box()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    public Mesh Boundary()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new BoxGeometry(box.X, box.Y, box.Z),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetWireframe()
        };
        return ShapeMesh;
    }

    private Mesh Cylinder()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);
        ShapeMesh = new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: box.X / 2, radiusBottom: box.X / 2, height: box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Sphere()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new SphereGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Tube()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);


        ShapeMesh = new Mesh
        {
            Geometry = new TubeGeometry(radius: box.X / 2, path: Path!, 8, 10),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }
    private Mesh Circle()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new CircleGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Capsule()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new CapsuleGeometry(radius: box.X / 2, box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Cone()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new ConeGeometry(radius: box.X / 2, height: box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Dodecahedron()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Icosahedron()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Octahedron()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new OctahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }
    private Mesh Tetrahedron()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: box.X / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }
    private Mesh Plane()
    {
        if (ShapeMesh != null) return ShapeMesh;

        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new PlaneGeometry(width: box.X, height: box.Y),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }

    private Mesh Ring()
    {
        if (ShapeMesh != null) return ShapeMesh;
        var box = BoundingBox ?? new Vector3(1, 1, 1);

        ShapeMesh = new Mesh
        {
            Geometry = new RingGeometry(innerRadius: box.X / 2, outerRadius: box.Y / 2),
            Position = GetPosition(),
            Pivot = GetPivot(),
            Scale = GetScale(),
            Rotation = GetRotation(),
            Material = GetMaterial()
        };
        return ShapeMesh;
    }



    private async Task<bool> PreRenderImport(FoArena3D arena, Viewer viewer, Import3DFormats format)
    {
        var settings = AsImportSettings(arena, format);

        if (string.IsNullOrEmpty(LoadingURL)) return false;
        $"PreRenderImport symbol [{LoadingURL}] ".WriteInfo(1);

        var uuid = await viewer.Request3DModel(settings);
        arena.Add<FoShape3D>(uuid.ToString(), this);
        return true;
    }

    public ImportSettings AsImportSettings(FoArena3D arena, Import3DFormats format)
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
            Scale = GetScale(),
            OnClick = async (ImportSettings self) =>
            {
                self.Increment();
                $"FoundryBlazor OnClick handler for self.Uuid={self.Uuid}, self.IsShow={self.IsShow()}".WriteInfo();
                UserHit?.Invoke(self);
                await arena.UpdateArena();
                $"FoundryBlazor OnClick handler UpdateArena called".WriteInfo();
            },
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
            var setting = body.AsImportSettings(arena, format);
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

    public override Vector3 GetScale(double x = 1, double y = 1, double z = 1)
    {
        if (Scale == null)
            return base.GetScale(x, y, z);
        return Scale;
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

        if (arena.Scene != null)
            SetupHitTest(arena.Scene);

        return result;
    }
    public void RenderPrimitives(Scene ctx)
    {
        if (ShapeMesh == null && IsVisible)
        {
            ShapeMesh = Type switch
            {
                "Box" => Box(),
                "Boundary" => Boundary(),
                "Circle" => Circle(),
                "Cylinder" => Cylinder(),
                "Sphere" => Sphere(),
                "Plane" => Plane(),
                "Capsule" => Capsule(),
                "Cone" => Cone(),
                "Tube" => Tube(),
                _ => null
            };

            if (ShapeMesh != null)
                ctx.Add(ShapeMesh);
        };

        if (ShapeMesh != null && !IsVisible)
        {
            ctx.Remove(ShapeMesh);
            ShapeMesh = null;
        }
    }

            // "PIN" => "Pink",
            // "PROC" => "Wisteria",
            // "DOC" => "Gray",
            // "ASST" => "Aqua",
            // "CAD" => "Orange",
            // "WRLD" => "Green",

    public List<FoPanel3D> EstablishTextPanels(ImportSettings model3D)
    {
        if ( TextPanels != null && TextPanels.Count > 0)
            return TextPanels;

        var root = model3D.Position.CreatePlus(0, 1, 0);
        if (Position != null && BoundingBox != null)
            root = Position.CreatePlus(0, BoundingBox.Y, 0);

        var leftPos = root.CreatePlus(-3, 1, 0);
        var centerPos = root.CreatePlus(0, 1, 0);  
        var rightPos = root.CreatePlus(3, 1, 0);

        var center = new FoPanel3D("Threads")
        {
            Width = 2.5,
            Height = 1.5,
            Color = "Gray",
            TextLines = Targets?.Select((item) => $"Address: {item.address}").ToList() ?? new List<string>(),
            Position = centerPos
        };

        var left = new FoPanel3D("Process")
        {
            Width = 2.5,
            Height = 1.5,
            Color = "Wisteria",
            TextLines = new() { "Process Steps" },
            Position = leftPos
        };

        var right = new FoPanel3D("BOM")
        {
            Width = 2.5,
            Height = 1.5,
            Color = "Pink",
            TextLines = new() { "BOM Structure" },
            Position = rightPos
        };

        TextPanels = new List<FoPanel3D>() { left, center, right };

        return TextPanels;
    }

    public bool SetupHitTest(Scene ctx, int tick = 0, double fps = 0, bool deep = true)
    {
        $"SetupHitTest for {Name}".WriteInfo();
        UserHit = (ImportSettings model3D) =>
        {
            $"In UserHit".WriteInfo();

            var list = EstablishTextPanels(model3D);
            foreach (var item in list)
            {
                item.IsVisible = model3D.IsShow();
                $"TextPanel.IsVisible = {item.IsVisible}".WriteInfo(1);
                item.Render(ctx, tick, fps, deep);
                $"TextPanel.Render complete".WriteInfo(1);     
            }

        };
        return true;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        // if ( ShapeObject3D != null && IsVisible)
        // {
        //     ctx.Add(ShapeObject3D);
        //     return true;
        // }
        // else if ( ShapeObject3D != null && !IsVisible)
        // {
        //     ctx.Remove(ShapeObject3D);
        //     ShapeObject3D = null;
        //     return true;
        // }

        RenderPrimitives(ctx);

        SetupHitTest(ctx, tick, fps, deep);


        return true;
    }
}