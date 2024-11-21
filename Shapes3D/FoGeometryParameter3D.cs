using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using System.Text.Json.Serialization;


namespace FoundryBlazor.Shape;

public class FoGeometryComponent3D : FoComponent
{
    public string GlyphId { get; set; } = "";
    protected Object3D Value3D { get; set; }

    public FoGeometryComponent3D(FoGlyph3D owner) : base(owner.GetName())
    {
        GetParent = () => owner;
        Value3D = null!;
    }

    public bool HasValue3D => Value3D != null;
    

    public Object3D GetValue3D()
    {
        return Value3D;
    }

    public Object3D SetValue3D(Object3D value)
    {
        Value3D = value;
        return Value3D;
    }

    public bool Smash()
    {
        Value3D = null!;
        return Value3D == null;
    }


    public string GetName()
    {
        return Key;
    }


    public string GetGlyphId()
    {
        if (string.IsNullOrEmpty(GlyphId))
            GlyphId = Guid.NewGuid().ToString();

        return GlyphId;
    }

    public bool GlyphIdCompare(string other)
    {
        var id = GetGlyphId();
        var result = id == other;
        // $"GlyphIdCompare {result}  {id} {other}".WriteNote();
        return result;
    }

    public bool UpdateMeshPosition(double xLoc, double yLoc, double zLoc)
    {
        //"Update mesh position".WriteSuccess();
        if (Value3D != null)
        {
            Value3D.Position.Set(xLoc, yLoc, zLoc);
            return true;
        }
        return false;
    }

    public void RemoveFromScene(Scene scene, bool deep = true)
    {
        if (HasValue3D)
        {
            scene.RemoveChild(Value3D);
            Smash();
        }
    }

    public async Task<bool> PreRender(FoModel3D source, FoArena3D arena, bool deep = true)
    {
        if ( Value3D != null)
            return true;
            
        var result = source.GeomType switch
        {
            "Collada" => await PreRenderImport(source, arena, Import3DFormats.Collada),
            "Fbx" => await PreRenderImport(source, arena, Import3DFormats.Fbx),
            "Obj" => await PreRenderImport(source, arena, Import3DFormats.Obj),
            "Stl" => await PreRenderImport(source, arena, Import3DFormats.Stl),
            "Glb" => await PreRenderImport(source, arena, Import3DFormats.Gltf),
            _ => false
        };

        return result;
    }
    private async Task<bool> PreRenderImport(FoModel3D source, FoArena3D arena, Import3DFormats format)
    {
        var settings = AsImportSettings(source, arena, format);

        if (string.IsNullOrEmpty(source.Url)) return false;
        $"PreRenderImport url [{source.Url}] ".WriteInfo(1);

        var scene = arena.CurrentScene();
        var uuid = await scene.Request3DModel(settings);
        //arena.Add<FoShape3D>(uuid, source);
        return true;
    }

    public ImportSettings AsImportSettings(FoModel3D source, FoArena3D arena, Import3DFormats format)
    {
        //LoadingURL = Url;
        var scene = arena.CurrentScene();

        var setting = new ImportSettings
        {
            Uuid = GetGlyphId(),

            Format = format,
            FileURL = source.Url,
            Position = source.GetPosition(),
            Rotation = source.GetRotation(),
            Pivot = source.GetPivot(),
            Scale = source.GetScale(),

            OnClick = async (ImportSettings self) =>
            {
                self.Increment();
                //$"FoundryBlazor OnClick handler for self.Uuid={self.Uuid}, self.IsShow={self.IsShow()}".WriteInfo();
                //source.UserHit?.Invoke(self);
                await arena.UpdateArena();
                //$"FoundryBlazor OnClick handler UpdateArena called".WriteInfo();
            },

            OnComplete = () =>
            {
                Value3D = new Group3D()
                {
                    Name = GetName(),
                    Uuid = GetGlyphId(),
                };
                scene.AddChild(Value3D);
                $"OnComplete for object3D.Uuid={Value3D.Uuid}, body.LoadingURL={source.Url}, position.x={source.Position?.X}".WriteInfo();
            }
        };

        return setting;
    }
   

   public (FoGeometryComponent3D obj, Object3D value) ComputeValue(FoGlyph3D source)
    {
        Value3D = source.GeomType switch
        {
            "GROUP" => AsGroup(source),
            "Box" => AsBox(source),
            "Boundary" => AsBoundary(source),
            "Circle" => AsCircle(source),
            "Cylinder" => AsCylinder(source),
            "Sphere" => AsSphere(source),
            "Plane" => AsPlane(source),
            "Capsule" => AsCapsule(source),
            "Cone" => AsCone(source),
            "Tube" => AsTube(source),
            "Ring" => AsRing(source),
            "Dodecahedron" => AsDodecahedron(source),
            "Icosahedron" => AsIcosahedron(source),
            "Octahedron" => AsOctahedron(source),
            "Tetrahedron" => AsTetrahedron(source),
            _ => Value3D,
        };
        return (this, Value3D);
    }

    public Mesh3D CreateMesh(FoGlyph3D source, BufferGeometry geometry)
    {
        return new Mesh3D
        {
            Name = Key,
            Uuid = GetGlyphId(),
            Geometry = geometry,
            Position = source.GetPosition(),
            Pivot = source.GetPivot(),
            Scale = source.GetScale(),
            Rotation = source.GetRotation(),
            Material = source.GetMaterial()
        };
    }

    public Object3D AsBox(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        Value3D = CreateMesh(source, new BoxGeometry(box.X, box.Y, box.Z));
        return Value3D;
    }

    
    public Object3D AsBoundary(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        Value3D = CreateMesh(source, new BoxGeometry(box.X, box.Y, box.Z));
        //Value3D!.Material.Wireframe = true;
        return Value3D;
    }

    public Object3D AsCylinder(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        Value3D = CreateMesh(source, new CylinderGeometry(box.X / 2, box.X / 2, box.Y));
        return Value3D;
    }



    public Object3D AsSphere(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        Value3D = CreateMesh(source, new SphereGeometry(box.X / 2));
        return Value3D;
    }    


    public Object3D AsTube(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        var geometry = new TubeGeometry(source.Radius, source.Path!, 8, 10);
        Value3D = CreateMesh(source, geometry);
        return Value3D;
    }
    
    public Object3D AsCircle(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        Value3D = CreateMesh(source, new CircleGeometry(box.X / 2));
        return Value3D;
    }
 
 
    public Object3D AsCapsule(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        var box = source.BoundingBox ?? new Vector3(1, 1, 1);
        Value3D = CreateMesh(source, new CapsuleGeometry(box.X / 2, box.Y));
        return Value3D;
    }

    public Object3D AsCone(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new ConeGeometry(box.X / 2, box.Y));
        return Value3D;
    }

    public Object3D AsDodecahedron(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new DodecahedronGeometry(box.X / 2));
        return Value3D;
    }

    public Object3D AsIcosahedron(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new IcosahedronGeometry(box.X / 2));
        return Value3D;
    }

    public Object3D AsOctahedron(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new OctahedronGeometry(box.X / 2));
        return Value3D;
    }

    public Object3D AsTetrahedron(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new TetrahedronGeometry(box.X / 2));
        return Value3D;
    }

    public Object3D AsPlane(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new PlaneGeometry(box.X, box.Y));
        return Value3D;
    }

    public Object3D AsRing(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;
        var box = source.BoundingBox ?? new Vector3(1, 1, 1);

        Value3D = CreateMesh(source, new RingGeometry(box.X / 2, box.Y / 2));
        return Value3D;
    }
      
    public Object3D AsGroup(FoGlyph3D source)
    {
        if (Value3D != null) return Value3D;

        Value3D = new Group3D()
        {
            Name = Key,
            Uuid = GetGlyphId(),
            Position = source.GetPosition(),
            Pivot = source.GetPivot(),
            Scale = source.GetScale(),
            Rotation = source.GetRotation(),
        };

        return Value3D;
    }


}