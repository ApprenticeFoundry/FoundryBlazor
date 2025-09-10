
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

    private Model3DFormats format = Model3DFormats.Gltf;
    private string url = "";
    public string Url
    {
        get { return this.url; }
        set { this.url = string.Intern(ComputeFormat(AssignText(value, url))); }
    }

    private string ComputeFormat(string text)
    {
        if (text.EndsWith(".glb")) 
            format =  Model3DFormats.Gltf;

        return text;
    }

    public FoModel3D() : base()
    {
         GeomType = "Model";
    }
    public FoModel3D(string name) : base(name)
    {
         GeomType = "Model";
    }
    public FoModel3D(string name, string color) : base(name, color)
    {
         GeomType = "Model";
    }

    public FoModel3D CreateModel(string name, string url, double width, double height, double depth)
    {
        Key = name;
        Url = url;
        Width = width;
        Height = height;
        Depth = depth;
        SetOwner(name);
        return this;
    }

    public Model3D AsModel3D()
    {
        ComputeFormat(Url);
        var model = new Model3D()
        {
            Uuid = GetGlyphId(),
            Name = GetName(),
            //Color = Color ?? "Yellow",
            Url = Url,
            Format = format,
            Transform = Transform,
        };
        FinaliseValue3D(model);
        return model;
    }



    public override (bool success, Object3D result) GetValue3D()
    {
        if ( !IsDirty && Value3D != null )
            return (true, Value3D);

        if ( Value3D == null )
        {
            Value3D = AsModel3D();
            return (true, Value3D);
        }

        //at this point we have a Value3D but it requires updating
        var model = Value3D as Model3D;
        if (model != null)
        {
            model.Url = Url;
            model.Format = format;
            model.Transform = Transform;
            return (true, model);
        }
        else
        {
            return (false, Value3D);
        }
    }

    public override bool RefreshToScene(Scene3D scene, bool deep = true)
    {
        var (success, _) = ComputeValue3D(scene);

        if (success)
            $"FoModel3D RefreshToScene {Name} {Url}".WriteSuccess();
        else
            $"FoModel3D RefreshToScene NO Value3D".WriteError();

        return success;
    }

    public override string GetTreeNodeTitle()
    {
        return $"{base.GetTreeNodeTitle()} {Url}";
    }

}