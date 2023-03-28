using BlazorThreeJS.Scenes;
using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;

public interface IStageManagement
{

    FoStage3D CurrentStage();
    FoStage3D SetCurrentStage(FoStage3D page);
    FoStage3D AddStage(FoStage3D page);
    List<IFoMenu> CollectMenus(List<IFoMenu> list);

    void ClearAll();
    int StageCount();

    Task RenderDetailed(Scene scene, int tick, double fps);

    T Add<T>(T value) where T : FoGlyph3D;
    //T Duplicate<T>(T value) where T : FoGlyph3D;
    //U MorphTo<T, U>(T value) where T : FoGlyph3D where U : FoGlyph3D;
    //T? GroupSelected<T>() where T : FoGlyph3D;
    U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D;
}


public class StageManagementService : IStageManagement
{

    private bool RenderHitTestTree = false;
    private FoStage3D ActiveStage { get; set; }
    private readonly FoCollection<FoStage3D> Stages = new();
    private readonly IHitTestService _hitTestService;
    private readonly ISelectionService _selectService;
    private readonly IScaledArena _ScaledArena;

    public StageManagementService
    (
        IHitTestService hit,
        IScaledArena scaled,
        ISelectionService sel)
    {
        _hitTestService = hit;
        _selectService = sel;
        _ScaledArena = scaled;

        ActiveStage = CurrentStage();
    }



    public int StageCount()
    {
        return 1;
    }

    public async Task RenderDetailed(Scene scene, int tick, double fps)
    {
        await CurrentStage().RenderDetailed(scene, tick, fps);
    }



    public void ClearAll()
    {
        FoGlyph2D.ResetHitTesting = true;
       // CurrentStage().ClearAll();
    }

    public bool ToggleHitTestRender()
    {
        FoGlyph2D.ResetHitTesting = true;
        RenderHitTestTree = !RenderHitTestTree;
        return RenderHitTestTree;
    }


    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        CurrentStage().GetMembers<FoMenu3D>()?.ForEach(item =>
        {
            list.Add(item);
        });
        return list;
    }   



    public T Add<T>(T value) where T : FoGlyph3D
    {
        var found = CurrentStage().Add(value);
        //_hitTestService.Insert(value);
        return found;

    }

    public FoStage3D CurrentStage()
    {
        if (ActiveStage == null)
        {
            var found = Stages.Values().Where(page => page.IsActive).FirstOrDefault();
            if (found == null)
            {
                found = new FoStage3D("Stage-1",10,10,10,"Red");
                found.SetScaledArena(_ScaledArena);
                AddStage(found);
            }
            ActiveStage = found;
            ActiveStage.IsActive = true;
        }

        return ActiveStage;
    }
    public FoStage3D SetCurrentStage(FoStage3D page)
    {
        ActiveStage = page;
        Stages.Values().ForEach(item => item.IsActive = false);
        ActiveStage.IsActive = true;
        return ActiveStage!;
    }

    public FoStage3D AddStage(FoStage3D scene)
    {
        var found = Stages.Values().Where(item => item == scene).FirstOrDefault();
        if (found == null)
            Stages.Add(scene);
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
        var menu = CurrentStage().Find<U>(name);
        menu?.Clear();
    }

    public U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D
    {
        var menu = CurrentStage().Find<U>(name);
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