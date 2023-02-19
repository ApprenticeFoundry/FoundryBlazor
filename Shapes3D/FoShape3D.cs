
using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Scenes;
using IoBTMessage.Models;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Enums;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D
{
    public UDTO_Body? Body { get; set; }

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

    private BufferGeometry NotImplemented(UDTO_Body body)
    {
        $"symbol [{body.symbol}] Body type [{body.type}] NotImplemented".WriteLine();
        return (BufferGeometry)(new BoxGeometry(1F, 1F, 1F));
    }

    private BufferGeometry Box(UDTO_Body body)
    {
        var box = body.boundingBox;
        return (BufferGeometry)(new BoxGeometry((float)box.width, (float)box.height, (float)box.depth));
    }
    private BufferGeometry Cylinder(UDTO_Body body)
    {
        var box = body.boundingBox;
        return (BufferGeometry)(new CylinderGeometry(radiusTop: (float)box.width / 2, height: (float)box.height, radialSegments: 16));
    }

    private BufferGeometry Glb(Viewer viewer, UDTO_Body body)
    {

        var url = body.symbol.Replace("http", "https");

        // var url = "https://rondtar.azurewebsites.net/storage/StaticFiles/2503172_FWD_TAILCONE_SHELL.glb";

        // var url = "https://threejs.org/examples/models/fbx/Samba%20Dancing.fbx";

        var pos = body.position;
        var settings = new ImportSettings
        {
            Format = Import3DFormats.Gltf,
            FileURL = url,
            Position = new Vector3((float)pos.xLoc, (float)pos.yLoc, (float)pos.zLoc)
        };

        Task.Run(async () =>
        {
            $"GLB symbol [{url}] ".WriteLine();
            var guid = await viewer.Import3DModelAsync(settings);
            $"GLB guid [{guid}] ".WriteLine();
        });

        var box = body.boundingBox;
        return (BufferGeometry)(new BoxGeometry((float)0, (float)0, (float)0));
    }

    public override MeshStandardMaterial GetMaterial()
    {
        if (Body == null) return base.GetMaterial();

        if (!string.IsNullOrEmpty(Body.symbol)) return base.GetMaterial();

        var result = new MeshStandardMaterial()
        {
            Color = this.Color,
            //Wireframe = true
        };
        return result;
    }

    public override BufferGeometry GetGeometry(Viewer viewer)
    {
        if (Body == null) return base.GetGeometry(viewer);


        var result = Body.type switch
        {
            "Box" => Box(Body),
            "Cylinder" => Cylinder(Body),
            "Glb" => Glb(viewer, Body),
            _ => NotImplemented(Body)
        };

        return result;
    }
    public override Vector3 GetPosition()
    {
        if (Body == null) return base.GetPosition();

        var pos = Body.position;

        if (pos == null) return base.GetPosition();
        var result = new Vector3((float)pos.xLoc, (float)pos.yLoc, (float)pos.zLoc);
        return result;
    }
    public override void Render(Viewer viewer, Scene ctx, int tick, double fps, bool deep = true)
    {
        var mesh = GetMesh(viewer);
        ctx.Add(mesh);
    }
}