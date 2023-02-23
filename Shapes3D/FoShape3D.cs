
using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Enums;
using FoundryBlazor.Extensions;
using BlazorThreeJS.Labels;
using static System.Formats.Asn1.AsnWriter;
using BlazorThreeJS.Objects;

namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D
{

    public string Symbol { get; set; } = "";
    public string Type { get; set; } = "";
    public FoVector3D? Position { get; set; }
    public FoVector3D? Rotation { get; set; }
    public FoVector3D? Origin { get; set; }
    public FoVector3D? BoundingBox { get; set; }

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

    private BufferGeometry NotImplemented()
    {
        $"symbol [{Symbol}] Body type [{Type}] NotImplemented".WriteLine();
        return (BufferGeometry)(new BoxGeometry(0, 0, 0));
    }

    public BufferGeometry Box()
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        return (BufferGeometry)(new BoxGeometry(box.X, box.Y, box.Z));
    }

    public BufferGeometry Point()
    {
        return (BufferGeometry)(new BoxGeometry(0, 0, 0));
    }

    private BufferGeometry Cylinder()
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        return (BufferGeometry)(new CylinderGeometry(radiusTop: box.X / 2, radiusBottom: box.X / 2, height: box.Y));
    }

    private BufferGeometry Sphere()
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        return (BufferGeometry)(new SphereGeometry(radius: box.X / 2));
    }

    private BufferGeometry Glb()
    {

   
        return Point();
    }

    private void PreRenderGlb(Viewer viewer, Import3DFormats format)
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
            var guid = await viewer.Import3DModelAsync(settings);
            $"PreRenderGlb guid [{guid}] ".WriteLine();
        });
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

    public override BufferGeometry GetGeometry()
    {
        var box = BoundingBox ?? new FoVector3D(0, 0, 0);
        $"GetGeometry {box.X}, {box.Y}, {box.Z}".WriteInfo();

        if (Type == null) return base.GetGeometry();
        $"GetGeometry Type={Type}".WriteInfo();

        var result = Type switch
        {
            "Box" => Box(),
            "Cylinder" => Cylinder(),
            "Sphere" => Sphere(),
            "Glb" => Glb(),
            _ => NotImplemented()
        };

        return result;
    }
    public override FoVector3D GetPosition()
    {
        if (Position == null) return base.GetPosition();
        return Position;
    }


    public override void PreRender(Viewer viewer, bool deep = true)
    {
        if ((bool)(Type.Matches("Glb")))
            PreRenderGlb(viewer, Import3DFormats.Gltf);

    }

    public override void Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        if ((bool)(Type.Matches("Glb")))
            return;

        var mesh = new Mesh
        {
            Geometry = GetGeometry(),
            Position = GetPosition().AsVector3(),
            Material = GetMaterial()
        };
        ctx.Add(mesh);
    }


}