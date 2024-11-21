
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

public class FoModel3D : FoShape3D
{

    public string Url { get; set; } = "";

    

    public FoModel3D() : base()
    {
    }
    public FoModel3D(string name) : base(name)
    {
    }
    public FoModel3D(string name, string color) : base(name, color)
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


    public FoModel3D CreateGlb(string url, double width, double height, double depth)
    {
        CreateGlb(url);
        BoundingBox = new Vector3(width, height, depth);
        return this;
    }

    public FoModel3D CreateGlb(string url)
    {
        GeomType = "Glb";
        Url = url;
        $"CreateGlb url [{Url}] ".WriteSuccess();
        return this;
    }



    public override async Task<bool> PreRender(FoArena3D arena, bool deep = true)
    {
        if (GeometryParameter3D.HasValue3D)
            return true;
            
        //is symbol ends with ....
        //LoadingURL = Symbol.Replace("http:", "https:");
        //await Task.CompletedTask;

        //LoadingURL = Url;
        $"Shape PRERENDER {Name} => {GetTreeNodeTitle()} {Url}".WriteWarning();

        var result = await GeometryParameter3D.PreRender(this, arena, deep);
        //if (arena.Scene != null)
        //    SetupHitTest(arena.Scene);

        return result;
    }



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






    public override MeshStandardMaterial GetMaterial()
    {
        if (!string.IsNullOrEmpty(Url))
            return base.GetMaterial();

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




}