
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Lights;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Settings;


namespace FoundryBlazor.Shape;

public interface IScene
{
    void Clearall();
    Scene GetScene();
    ViewerSettings GetSettings();
}

public class FoScene3D : FoGlyph3D, IScene
{
    public bool IsActive { get; set; } = false;

    public Scene scene = new();

    public ViewerSettings settings = new()
    {
        CanSelect = true,// default is false
        SelectedColor = "black",
        WebGLRendererSettings = new WebGLRendererSettings
        {
            Antialias = false // if you need poor quality for some reasons
        }
    };

    public FoScene3D()
    {
    }

    public Scene GetScene()
    {
        return scene;
    }

    public void Clearall()
    {
        //scene.cl;
    }
    public ViewerSettings GetSettings()
    {
        return settings;
    }

}