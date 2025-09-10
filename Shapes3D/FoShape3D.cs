
using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Viewers;
using FoundryRulesAndUnits.Extensions;


namespace FoundryBlazor.Shape;

public class FoShape3D : FoGlyph3D, IShape3D
{

    public FoShape3D() : base()
    {
    }
    public FoShape3D(string name) : base(name)
    {
    }
    public FoShape3D(string name, string color) : base(name, color)
    {
    }

    public FoShape3D SetOwner(string owner)
    {
        Transform!.OwnerName = owner;
        return this;
    }



    public FoShape3D CreateBox(string name, double width, double height, double depth)
    {
        GeomType = "Box";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    public FoShape3D CreateBoundary(string name, double width, double height, double depth)
    {
        GeomType = "Boundary";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    public FoShape3D CreateGroup(string name, double width, double height, double depth)
    {
        GeomType = "Group";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    public FoShape3D CreateCylinder(string name, double width, double height, double depth)
    {
        GeomType = "Cylinder";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }

    //CreateDodecahedron
    public FoShape3D CreateDodecahedron(string name, double width, double height, double depth)
    {
        GeomType = "Dodecahedron";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    //CreateIcosahedron
    public FoShape3D CreateIcosahedron(string name, double width, double height, double depth)
    {
        GeomType = "Icosahedron";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    //CreateOctahedron
    public FoShape3D CreateOctahedron(string name, double width, double height, double depth)
    {
        GeomType = "Octahedron";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    //CreateTorus
    public FoShape3D CreateTetrahedron(string name, double width, double height, double depth)
    {
        GeomType = "Tetrahedron";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    //CreateTorusKnot
    public FoShape3D CreateTorusKnot(string name, double width, double height, double depth)
    {
        GeomType = "TorusKnot";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
  
    public FoShape3D CreateTorus(string name, double width, double height, double depth)
    {
        GeomType = "Torus";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }


    public FoShape3D CreateSphere(string name, double width, double height, double depth)
    {
        GeomType = "Sphere";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }

    public FoShape3D CreateCircle(string name, double width, double height, double depth)
    {
        GeomType = "Circle";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    public FoShape3D CreatePlane(string name, double width, double height, double depth)
    {
        GeomType = "Plane";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }

    public FoShape3D CreateRing(string name, double width, double height, double depth)
    {
        GeomType = "Ring";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }

    public FoShape3D CreateCapsule(string name, double width, double height, double depth)
    {
        GeomType = "Capsule";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
    public FoShape3D CreateCone(string name, double width, double height, double depth)
    {
        GeomType = "Cone";
        Key = name;
        Width = width;
        Height = height;
        Depth = depth;
        return SetOwner(name);
    }
  

    public override MeshStandardMaterial GetMaterial()
    {
        var result = GetWireframe();
        result.Wireframe = false;
        return result;
    }


    public Mesh3D CreateMesh(BufferGeometry geometry, Material material = null!)
    {
        var result = new Mesh3D
        {
            Name = Key,
            Uuid = GetGlyphId(),
            Geometry = geometry,
            Transform = Transform,
            Material = material != null ? material : GetMaterial()
        };
        return result;
    }

    public override bool RefreshToScene(Scene3D scene, bool deep = true)
    {
        var (success, _) = ComputeValue3D(scene);

        // if (success)
        //     $"FoShape3D RefreshToScene {Name} {GeomType} ".WriteSuccess();
        // else
        //     $"FoShape3D RefreshToScene NO Value3D".WriteError();

        return success;
    }



    public override (bool success, Object3D result) GetValue3D()
    {
        if (!IsDirty && Value3D != null )
            return (true, Value3D);

        if ( Value3D == null )
        {
            Value3D = AsMesh3D();
            return (true, Value3D);
        }

        //at this point we have a Value3D but it requires updating
        var mesh = Value3D as Mesh3D;
        if (mesh != null)
        {
            mesh.Transform = Transform;
            return (true, mesh);
        }
        else
        {
            return (false, Value3D);
        }
    }


    public virtual Mesh3D AsMesh3D()
    {
        var mesh3D = GeomType.ToLower() switch
        {
            "box" => AsBox(),
            "boundry" => AsBoundary(),
            "circle" => AsCircle(),
            "cylinder" => AsCylinder(),
            "sphere" => AsSphere(),
            "plane" => AsPlane(),
            "capsule" => AsCapsule(),
            "cone" => AsCone(),
            "ring" => AsRing(),
            "dodecahedron" => AsDodecahedron(),
            "icosahedron" => AsIcosahedron(),
            "octahedron" => AsOctahedron(),
            "tetrahedron" => AsTetrahedron(),
            "torusKnot" => AsTorusKnot(),
            "torus" => AsTorus(),
            _ => AsBoundary(),
        };

        //$"Created AsMesh3D {GeomType}".WriteSuccess();
        FinaliseValue3D(mesh3D);
        return mesh3D;
    }

    public Mesh3D AsBox()
    {
        var geometry = new BoxGeometry(width, height, depth);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsBoundary()
    {
        var geometry = new BoundaryGeometry(width, height, depth);
        var mesh = CreateMesh(geometry,GetWireframe());
        return mesh;
    }

    public Mesh3D AsCircle()
    {
        var radius = 0.5 * Math.Max(width, Math.Max(height, depth));
        var geometry = new CircleGeometry(radius);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsCylinder()
    {
        var geometry = new CylinderGeometry(width/2.0, depth/2.0, height, 36);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsSphere()
    {
        var radius = 0.5 * Math.Max(width, Math.Max(height, depth));
        var geometry = new SphereGeometry(radius);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsPlane()
    {
        var geometry = new PlaneGeometry(width, height);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsCapsule()
    {
        var geometry = new CapsuleGeometry(width/2, height);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsCone()
    {
        var geometry = new ConeGeometry(width/2, height);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsRing()
    {
        var geometry = new RingGeometry(width/2, height/2);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsDodecahedron()
    {
        var geometry = new DodecahedronGeometry(width);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsIcosahedron()
    {
        var geometry = new IcosahedronGeometry(width);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsOctahedron()
    {
        var geometry = new OctahedronGeometry(width);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsTetrahedron()
    {
        var geometry = new TetrahedronGeometry(width);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsTorusKnot()
    {
        var geometry = new TorusKnotGeometry(width/2, height/2);
        var mesh = CreateMesh(geometry);
        return mesh;
    }

    public Mesh3D AsTorus()
    {
        var geometry = new TorusGeometry(width/2, height/2);
        var mesh = CreateMesh(geometry);
        return mesh;
    }



}