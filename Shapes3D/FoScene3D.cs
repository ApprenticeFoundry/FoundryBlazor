using BlazorThreeJS.Scenes;
using BlazorThreeJS.Settings;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public interface IScene
{
    void Clearall();
    Scene GetScene();
    ViewerSettings GetSettings();
    V AddShape<V>(V shape) where V : FoGlyph3D;


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

    protected FoCollection<FoGlyph3D> Pipes3D = new();
    protected FoCollection<FoGlyph3D> Shapes3D = new();

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
    public T AddShape<T>(T value) where T : FoGlyph3D
    {
        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Name))
        {
            value.Name = collection.NextItemName();
        }

        collection.AddObject(value.Name, value);

        if ( value is IShape3D)
        {
            Shapes3D.Add(value);   
            //$"IShape3D Added {value.Name}".WriteSuccess();
        }
        else if ( value is IPipe3D)
        {
            Pipes3D.Add(value);
            //$"IPipe3D Added {value.Name}".WriteSuccess();
        }

        value.Render(GetScene(), 1, 1);

        return value;
    }

    public override bool Render(Scene ctx, int tick, double fps, bool deep = true)
    {
        //$"Render {tick} {Shapes3D.Count()}".WriteInfo();
        Shapes3D?.ForEach(shape => shape.ContextLink?.Invoke(shape,tick));
        return true;
    }


}