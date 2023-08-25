using BlazorThreeJS.Scenes;
using FoundryBlazor.Extensions;
using IoBTMessage.Extensions;

namespace FoundryBlazor.Shape;

public interface IStageManagement
{

    FoStage3D CurrentStage();
    FoStage3D SetCurrentStage(FoStage3D page);
    FoStage3D AddStage(FoStage3D page);

    V AddShape<V>(V shape) where V : FoGlyph3D;

    void ClearAll();
    int StageCount();

    Task RenderDetailed(Scene scene, int tick, double fps);

    //T Add<T>(T value) where T : FoGlyph3D;
    //T Duplicate<T>(T value) where T : FoGlyph3D;
    //U MorphTo<T, U>(T value) where T : FoGlyph3D where U : FoGlyph3D;
    //T? GroupSelected<T>() where T : FoGlyph3D;
 }


public class StageManagementService : FoComponent, IStageManagement
{

    private bool RenderHitTestTree = false;
    private FoStage3D ActiveStage { get; set; }
    //private readonly FoCollection<FoStage3D> Stages = new();
    private readonly IHitTestService _hitTestService;
    private readonly ISelectionService _selectService;


    public StageManagementService
    (
        IHitTestService hit,
        ISelectionService sel)
    {
        _hitTestService = hit;
        _selectService = sel;

        ActiveStage = CurrentStage();
    }



    public int StageCount()
    {
        return Members<FoStage3D>().Count;
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




     public T AddShape<T>(T value) where T : FoGlyph3D
    {
        var found = CurrentStage().AddShape(value);
        //if ( found != null)
        //    _hitTestService.Insert(value);

        return found!;

    }

    public FoStage3D CurrentStage()
    {
        if (ActiveStage == null)
        {
            var found = Members<FoStage3D>().Where(page => page.IsActive).FirstOrDefault();
            if (found == null)
            {
                found = new FoStage3D("Stage-1",10,10,10,"Red");
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
        Members<FoStage3D>().ForEach(item => item.IsActive = false);
        ActiveStage.IsActive = true;
        return ActiveStage!;
    }

    public FoStage3D AddStage(FoStage3D scene)
    {
        var found = Members<FoStage3D>().Where(item => item == scene).FirstOrDefault();
        if (found == null)
            Add(scene);
        return scene;
    }

 

    public U MorphTo<T, U>(T value) where T : FoGlyph3D where U : FoGlyph3D
    {
        var body = CodingExtensions.Dehydrate<T>(value, false);
        var shape = CodingExtensions.Hydrate<U>(body, false);

        shape!.Name = "";
        //shape!.GlyphId = "";

        return Add<U>(shape);
    }



    public void ClearMenu3D<U>(string name) where U : FoMenu3D
    {
        var stage = CurrentStage();
        var menu = stage.Find<U>(name);
        menu?.Clear();
    }


}