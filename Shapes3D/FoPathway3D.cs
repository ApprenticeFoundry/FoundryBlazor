
using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Shape;


public class FoPathway3D : FoGlyph3D, IPipe3D
{


    public FoPathway3D(string name) : base(name, "Grey")
    {
        Color = "pink";
    }

    public Object3D EstablishPathway3D()
    {
        if (GeometryParameter3D.HasValue3D) 
            return GeometryParameter3D.GetValue3D();

        GeometryParameter3D.AsTube(this);
        return GeometryParameter3D.GetValue3D();
    }

    public override bool Render(Scene scene, int tick, double fps, bool deep = true)
    {
        $"RenderPathway {Key}".WriteNote();
        scene.AddChild(EstablishPathway3D());
        return true;
    }

}