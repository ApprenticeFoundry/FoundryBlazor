using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public interface ISceneManagement
{

    FoScene3D CurrentScene();
    FoScene3D SetCurrentScene(FoScene3D page);
    FoScene3D AddScene(FoScene3D page);
    List<IFoMenu> CollectMenus(List<IFoMenu> list);

    void ClearAll();
    int SceneCount();



    T Add<T>(T value) where T : FoGlyph3D;
    //T Duplicate<T>(T value) where T : FoGlyph3D;
    //U MorphTo<T, U>(T value) where T : FoGlyph3D where U : FoGlyph3D;
    //T? GroupSelected<T>() where T : FoGlyph3D;
    U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D;
}


public class SceneManagementService : ISceneManagement
{

    private bool RenderHitTestTree = false;
    private FoScene3D ActiveScene { get; set; }
    private readonly FoCollection<FoScene3D> _scenes = new();
    private readonly IHitTestService _hitTestService;
    private readonly ISelectionService _selectService;
    private readonly IScaledArena _ScaledArena;

    public SceneManagementService
    (
        IHitTestService hit,
        IScaledArena scaled,
        ISelectionService sel)
    {
        _hitTestService = hit;
        _selectService = sel;
        _ScaledArena = scaled;

        ActiveScene = CurrentScene();
    }



    public int SceneCount()
    {
        return 1;
    }



    public void ClearAll()
    {
        FoGlyph2D.ResetHitTesting = true;
        CurrentScene().ClearAll();
    }

    public bool ToggleHitTestRender()
    {
        FoGlyph2D.ResetHitTesting = true;
        RenderHitTestTree = !RenderHitTestTree;
        return RenderHitTestTree;
    }


    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        CurrentScene().GetMembers<FoMenu3D>()?.ForEach(item =>
        {
            list.Add(item);
        });
        return list;
    }   



    public T Add<T>(T value) where T : FoGlyph3D
    {
        var found = CurrentScene().Add(value);
        //_hitTestService.Insert(value);
        return found;

    }

    public FoScene3D CurrentScene()
    {
        if (ActiveScene == null)
        {
            var found = _scenes.Values().Where(page => page.IsActive).FirstOrDefault();
            if (found == null)
            {
                found = new FoScene3D("Scene-1",10,10,10,"Red");
                found.SetScaledArena(_ScaledArena);
                AddScene(found);
            }
            ActiveScene = found;
            ActiveScene.IsActive = true;
        }

        return ActiveScene;
    }
    public FoScene3D SetCurrentScene(FoScene3D page)
    {
        ActiveScene = page;
        _scenes.Values().ForEach(item => item.IsActive = false);
        ActiveScene.IsActive = true;
        return ActiveScene!;
    }

    public FoScene3D AddScene(FoScene3D scene)
    {
        var found = _scenes.Values().Where(item => item == scene).FirstOrDefault();
        if (found == null)
            _scenes.Add(scene);
        return scene;
    }

 

    public U MorphTo<T, U>(T value) where T : FoGlyph3D where U : FoGlyph3D
    {
        var body = StorageHelpers.Dehydrate<T>(value, false);
        var shape = StorageHelpers.Hydrate<U>(body, false);

        shape!.Name = "";
        //shape!.GlyphId = "";

        return Add<U>(shape);
    }



    public void ClearMenu3D<U>(string name) where U : FoMenu3D
    {
        var menu = CurrentScene().Find<U>(name);
        menu?.Clear();
    }

    public U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D
    {
        var menu = CurrentScene().Find<U>(name);
        if (menu == null)
        {
            menu = Activator.CreateInstance(typeof(U), name) as U;
            this.Add<U>(menu!);
        }
        if ( clear )
            menu?.Clear();

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                menu?.Add<T>(shape);
        }

        //menu!.LayoutHorizontal();

        return menu!;
    }







}