
using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using static System.Formats.Asn1.AsnWriter;


namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D, IShape3D
{

  
    public List<FoPanel3D>? TextPanels { get; set; }
    public Action<ImportSettings> UserHit { get; set; } = (ImportSettings model3D) => { };



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
        return GeometryParameter3D.UpdateMeshPosition(xLoc, yLoc, zLoc);
    }

    public override string GetTreeNodeTitle()
    {

        var HasMesh = GeometryParameter3D.HasValue3D ? "Ok" : "No Value3D";
        return $"{GeomType}: {Key} {GetType().Name} {HasMesh} => ";
    }


    public FoShape3D CreateBox(string name, double width, double height, double depth)
    {
        GeomType = "Box";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    public FoShape3D CreateBoundry(string name, double width, double height, double depth)
    {
        GeomType = "Boundry";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    public FoShape3D CreateCylinder(string name, double width, double height, double depth)
    {
        GeomType = "Cylinder";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }

    //CreateDodecahedron
    public FoShape3D CreateDodecahedron(string name, double width, double height, double depth)
    {
        GeomType = "Dodecahedron";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    //CreateIcosahedron
    public FoShape3D CreateIcosahedron(string name, double width, double height, double depth)
    {
        GeomType = "Icosahedron";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    //CreateOctahedron
    public FoShape3D CreateOctahedron(string name, double width, double height, double depth)
    {
        GeomType = "Octahedron";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    //CreateTorus
    public FoShape3D CreateTetrahedron(string name, double width, double height, double depth)
    {
        GeomType = "Tetrahedron";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    //CreateTorusKnot
    public FoShape3D CreateTorusKnot(string name, double width, double height, double depth)
    {
        GeomType = "TorusKnot";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    //CreateTorus 
    public FoShape3D CreateTorus(string name, double width, double height, double depth)
    {
        GeomType = "Torus";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }

    public FoShape3D CreateTube(string name, double radius, List<Vector3> path)
    {
        GeomType = "Tube";
        Radius = radius;
        BoundingBox = new Vector3(radius, 0, 0);
        Key = name;
        Path = path;
        return this;
    }



    public FoShape3D CreateSphere(string name, double width, double height, double depth)
    {
        GeomType = "Sphere";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }

    public FoShape3D CreateCircle(string name, double width, double height, double depth)
    {
        GeomType = "Circle";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    public FoShape3D CreatePlane(string name, double width, double height, double depth)
    {
        GeomType = "Plane";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }

    public FoShape3D CreateRing(string name, double width, double height, double depth)
    {
        GeomType = "Ring";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }

    public FoShape3D CreateCapsule(string name, double width, double height, double depth)
    {
        GeomType = "Capsule";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
    public FoShape3D CreateCone(string name, double width, double height, double depth)
    {
        GeomType = "Cone";
        BoundingBox = new Vector3(width, height, depth);
        Key = name;
        return this;
    }
  

    // public override async Task<bool> PreRender(FoArena3D arena, bool deep = true)
    // {
    //     if (GeometryParameter3D.HasValue3D)
    //         return true;
            
    //     //is symbol ends with ....
    //     //LoadingURL = Symbol.Replace("http:", "https:");
    //     //await Task.CompletedTask;

    //     //LoadingURL = Url;
    //     $"Shape PRERENDER {Name} => {GetTreeNodeTitle()} {Url}".WriteWarning();

    //     var result = await GeometryParameter3D.PreRender(this, arena, deep);
    //     //if (arena.Scene != null)
    //     //    SetupHitTest(arena.Scene);

    //     return result;
    // }



    // public static async Task<bool> PreRenderClones(List<FoShape3D> bodies, FoArena3D arena, Import3DFormats format)
    // {
    //     var settings = new List<ImportSettings>();

    //     foreach (var body in bodies)
    //     {
    //         var setting = body.AsImportSettings(arena, format);
    //         arena.Add<FoShape3D>(body.GetGlyphId(), body);
    //         settings.Add(setting);

    //         $"AsImportSettings body.Symbol {body.Url} X = {setting.FileURL}".WriteSuccess();
    //     }

    //     var source = settings.ElementAt(0);
    //     settings.RemoveAt(0);

    //     var sourceBody = bodies.ElementAt(0);
    //     bodies.RemoveAt(0);

    //     // source.OnComplete = async () =>
    //     // {
    //     //     if (object3D != null)
    //     //     {
    //     //         sourceBody.ShapeObject3D = object3D;
    //     //         if (settings.Count > 0)
    //     //             await scene.Clone3DModel(object3D.Uuid!, settings);
    //     //     }
    //     //     else
    //     //         "Unexpected empty object3D".WriteError(1);
    //     // };

    //     var scene = arena.Scene!;
    //     await scene.Request3DModel(source);
    //     return true;
    // }





    public MeshStandardMaterial GetWireframe()
    {
        var result = new MeshStandardMaterial()
        {
            Name = Key,
            Uuid = GetGlyphId(),
            Color = this.Color,
            Wireframe = true
        };
        return result;
    }

    public override MeshStandardMaterial GetMaterial()
    {
        var result = GetWireframe();
        result.Wireframe = false;
        return result;
    }



    public override bool Render(Scene scene, int tick, double fps, bool deep = true)
    {
        RenderPrimitives(scene);

        // SetupHitTest(scene, tick, fps, deep);
        return true;
    }

    public override FoGeometryComponent3D RenderPrimitives(Scene? scene)
    {
        if (!GeometryParameter3D.HasValue3D)
            GeometryParameter3D.ComputeValue(this);
       

        if (GeometryParameter3D.HasValue3D)
        {
            scene?.AddChild(GeometryParameter3D.GetValue3D());
        }

        //delete mesh if you are invisible
        if (GeometryParameter3D.HasValue3D && !IsVisible)
        {
            scene?.RemoveChild(GeometryParameter3D.GetValue3D());
            GeometryParameter3D.Smash();
        }
        return GeometryParameter3D;
    }

    public override async Task<bool> RemoveFromRender(Scene scene, bool deep = true)
    {
        GeometryParameter3D.RemoveFromScene(scene);
        await Task.CompletedTask;
        return true;
    }


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

        //var lines = Targets?.Where(item => item.address.Length < 20 )
        //            .Select((item) => $"{item.domain}: {item.address}").ToList() ?? new List<string>();

        var center = new FoPanel3D("Threads")
        {
            Width = 2.5,
            Height = 1.5,
            Color = "Gray",
            TextLines = new() { "Thread Links" },
            Position = centerPos
        };

        //if this is Mk48 then add a panel for the process steps
        var left = new FoPanel3D("Process")
        {
            Width = 2.5,
            Height = 1.5,
            Color = "Wisteria",
            TextLines = new() { "Process Steps" },
            Position = leftPos
        };
        //if this is Mk48 then add a panel for the BOM
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

    // public bool SetupHitTest(Scene ctx, int tick = 0, double fps = 0, bool deep = true)
    // {
    //     //$"SetupHitTest for {Name}".WriteInfo();
    //     UserHit = (ImportSettings model3D) =>
    //     {
    //         //$"In UserHit".WriteInfo();

    //         var list = EstablishTextPanels(model3D);
    //         foreach (var item in list)
    //         {
    //             item.IsVisible = model3D.IsShow();
    //             item.Render(ctx, tick, fps, deep);  
    //         }

    //     };
    //     return true;
    // }




}