
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





    private bool PreRenderGlb(FoArena3D arena, Viewer viewer, Import3DFormats format)
    {

        var url = Symbol?.Replace("http", "https");

        var settings = new ImportSettings
        {
            Format = format,
            FileURL = url ?? "",
            Position = Position?.AsVector3() ?? new Vector3()
        };

        Task.Run(async () =>
        {
            $"PreRenderGlb symbol [{url}] ".WriteLine();
            PromiseGUID = await viewer.Import3DModelAsync(settings);
            $"PreRenderGlb guid [{PromiseGUID}] ".WriteLine();
            arena.Add<FoShape3D>(PromiseGUID.Value.ToString(), this);
        });
        return true;
    }

    private bool RenderGlb(Scene ctx, Import3DFormats format)
    {

        var url = Symbol?.Replace("http", "https");

        Loading(ctx, $"Loading... {url}");
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
        if ((bool)(Type.Matches("Glb")))
        {
            return PreRenderGlb(arena, viewer, Import3DFormats.Gltf);
        }
        return false;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {

        var result = Type switch
        {
            "Box" => Box(ctx),
            "Cylinder" => Cylinder(ctx),
            "Sphere" => Sphere(ctx),
            "Glb" => RenderGlb(ctx, Import3DFormats.Gltf),
            _ => NotImplemented(ctx)
        };
        return result;
    }
    public override bool PostRender(Scene ctx, Guid guid)
    {
        //add code to remove the 'loading...'  and then 
        //resolve the guid that was the promise
        PromiseGUID = null;
        LoadingGUID = null;
        return true;
    }

}