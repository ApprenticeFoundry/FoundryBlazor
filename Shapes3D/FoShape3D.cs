
using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Scenes;

using BlazorThreeJS.Settings;
using BlazorThreeJS.Enums;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D
{
    public string platformName { get; set; } = "";

    public string? Symbol { get; set; }
    public string? Type { get; set; }
    public FoVector3D? Position { get; set; }
    public FoVector3D? Rotation { get; set; }
    public FoVector3D? Origin { get; set; }
    public FoVector3D? BoundingBox { get; set; }

    public FoShape3D(string name) : base(name)
    {
    }
    public FoShape3D(string name, string color) : base(name, color)
    {
    }

    //https://BlazorThreeJS.com/reference/Index.html

    //  private bodyMeshDict: Record<string, Function> = {
    //     Glb: this.makeGlbFile3D.bind(this),
    //     Stub: this.makeStub3D.bind(this), //this should be a simple transform node
    //     Box: this.makeBox3D.bind(this),
    //     Sphere: this.makeSphere3D.bind(this),
    //     Boid: this.makeBoid3D.bind(this),
    //     Boundry: this.makeBoundry3D.bind(this),
    //     Cylinder: this.makeCylinder3D.bind(this)
    // };

    private BufferGeometry NotImplemented()
    {
        $"symbol [{Symbol}] Body type [{Type}] NotImplemented".WriteLine();
        return (BufferGeometry)(new BoxGeometry(1F, 1F, 1F));
    }

    public BufferGeometry Box()
    {
        var box = BoundingBox ?? new FoVector3D(1, 1, 1);
        return (BufferGeometry)(new BoxGeometry((float)box.X, (float)box.Y, (float)box.Z));
    }



    public FoShape3D CreateBox(string name, double width, double height, double depth, string units="m")
    {
        BoundingBox = new FoVector3D(width, height, depth)
        {
            units = units
        };
        Name = name;
        return this;
    }
    public FoShape3D CreateCylinder(string name, double width, double height, double depth, string units = "m")
    {
        BoundingBox = new FoVector3D(width, height, depth)
        {
            units = units
        };
        Name = name;
        return this;
    }


    private BufferGeometry Cylinder()
    {
        var box = BoundingBox ?? new FoVector3D(1,1,1);
        return (BufferGeometry)(new CylinderGeometry(radiusTop: (float)box.X / 2, height: (float)box.Y, radialSegments: 16));
    }

    private BufferGeometry Glb(Viewer viewer)
    {

        var url = Symbol?.Replace("http", "https");

        // var url = "https://rondtar.azurewebsites.net/storage/StaticFiles/2503172_FWD_TAILCONE_SHELL.glb";

        // var url = "https://threejs.org/examples/models/fbx/Samba%20Dancing.fbx";

        var settings = new ImportSettings
        {
            Format = Import3DFormats.Gltf,
            FileURL = url ?? "",
            //SRS Fix this Position = Position
        };

        Task.Run(async () =>
        {
            $"GLB symbol [{url}] ".WriteLine();
            var guid = await viewer.Import3DModelAsync(settings);
            $"GLB guid [{guid}] ".WriteLine();
        });

        var box = BoundingBox ?? new FoVector3D(1,1,1);
        return (BufferGeometry)(new BoxGeometry((float)0, (float)0, (float)0));
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

    public override BufferGeometry GetGeometry(Viewer viewer)
    {
        if (Type == null) return base.GetGeometry(viewer);


        var result = Type switch
        {
            "Box" => Box(),
            "Cylinder" => Cylinder(),
            "Glb" => Glb(viewer),
            _ => NotImplemented()
        };

        return result;
    }
    public override FoVector3D GetPosition()
    {
        if (Position == null) return base.GetPosition();
        return Position;
    }
    public override void Render(Viewer viewer, Scene ctx, int tick, double fps, bool deep = true)
    {
        var mesh = GetMesh(viewer);
        ctx.Add(mesh);
    }

    internal FoShape3D CreateGlb(string url, double width, double height, double depth, string units)
    {
        throw new NotImplementedException();
    }
}