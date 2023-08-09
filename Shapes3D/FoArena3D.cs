using BlazorComponentBus;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Canvas;
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using FoundryBlazor.Solutions;
using IoBTMessage.Extensions;
using IoBTMessage.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoundryBlazor.Shape;

public interface IArena
{
    void SetViewer(Viewer viewer, Scene scene);
    Task RenderArena(Scene scene, int tick, double fps);
    Task ClearArena();
    Task UpdateArena();
    void SetDoCreate(Action<CanvasMouseArgs> action);

    bool RenderDrawingToScene(IDrawing drawing);
    bool RenderWorld3DToScene(FoWorld3D world);
    bool RenderWorld3D(FoWorld3D world);
    Task<bool> PreRender(FoGlyph3D glyph);

    //FoWorld3D MapToWorld3D(UDTO_World world);

    V AddShape<V>(V shape) where V : FoGlyph3D;

    FoStage3D CurrentStage();
    FoWorld3D StressTest3DModelFromFile(string folder, string filename, string baseURL, int count);
    FoWorld3D Load3DModelFromFile(string folder, string filename, string baseURL);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);

}
public class FoArena3D : FoGlyph3D, IArena
{
    public Viewer? Viewer3D { get; set; }
    public Scene? Scene { get; set; }
    private IStageManagement StageManager { get; set; }

    public ComponentBus PubSub { get; set; }

    public Action<CanvasMouseArgs>? DoCreate { get; set; }

    public FoArena3D(
        IStageManagement manager,
        ComponentBus pubSub)
    {
        StageManager = manager;
        PubSub = pubSub;
    }

    public FoStage3D CurrentStage()
    {
        var stage = StageManager.CurrentStage();
        return stage;
    }

    public async Task RenderArena(Scene scene, int tick, double fps)
    {
        await StageManager.RenderDetailed(scene, tick, fps);

        //if the stage is dirty call to update
        //$"Arean Render Scene {tick}".WriteInfo();
    }

    public V AddShape<V>(V shape) where V : FoGlyph3D
    {
        return StageManager.AddShape<V>(shape);
    }

    public async Task ClearArena()
    {
        if (Viewer3D == null) return;

        //"ClearArena".WriteInfo();
        await Viewer3D.ClearSceneAsync();
        await UpdateArena();
    }

    public async Task UpdateArena()
    {
        if (Viewer3D == null) return;

        //"UpdateArena".WriteInfo();
        await Viewer3D.UpdateScene();
    }
    public void SetViewer(Viewer viewer, Scene scene)
    {
        Viewer3D = viewer;
        Scene = scene;
        CurrentStage().InitScene(scene);
    }

    public Viewer CurrentViewer()
    {
        return Viewer3D!;
    }
    public Scene CurrentScene()
    {
        return Scene!;
    }
    public async Task<bool> PreRender(FoGlyph3D glyph)
    {
        return await glyph.PreRender(this, Viewer3D!);
    }

    public virtual void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
    }

    public void SetDoCreate(Action<CanvasMouseArgs> action)
    {

        try
        {
            DoCreate = action;

            DoCreate?.Invoke(new CanvasMouseArgs()
            {
                OffsetX = 0,
                OffsetY = 0
            });

            //var region = _helper.UserWindow();
            //page.ComputeShouldRender(region);
        }
        catch (System.Exception ex)
        {
            $" DoCreate {action.Method.Name} {ex.Message}".WriteNote();
        }
    }



    public FoWorld3D StressTest3DModelFromFile(string folder, string filename, string baseURL, int count)
    {
        var name = Path.GetFileNameWithoutExtension(filename);


        var world3D = new UDTO_World();
        var data = new MockDataMaker();
        var url = Path.Join(baseURL, folder, filename);

        var root = new DT_Hero();

        for (int i = 0; i < count; i++)
        {
            root.name = $"{name}-{i}";
            var shape = world3D.CreateGlb(root, url, 1, 2, 3);
            shape.EstablishLoc(data.GenerateDouble(-5, 5), data.GenerateDouble(-5, 5), data.GenerateDouble(-5, 5), "m");
            shape.EstablishAng(data.GenerateDouble(0, 360), data.GenerateDouble(0, 360), data.GenerateDouble(0, 360), "r");
        };


        var world = new FoWorld3D(world3D);
        RenderWorld3D(world);

        //PostRenderplatform

        return world;
    }



    public FoWorld3D Load3DModelFromFile(string folder, string filename, string baseURL)
    {
        var name = Path.GetFileNameWithoutExtension(filename);

        var url = Path.Join(baseURL, folder, filename);
        // var url = $"{baseURL}/{folder}/{filename}";

        var root = new DT_Hero
        {
            name = name
        };

        var world3D = new UDTO_World();
        world3D.CreateGlb(root, url, 1, 2, 3);


        var text = name.Replace("_", " ");
        world3D.CreateLabel(root, text, 0.0, 5.0, 0.0);



        var world = new FoWorld3D(world3D);
        RenderWorld3D(world);


        return world;
    }

    public bool RenderDrawingToScene(IDrawing drawing)
    {
        if ( Scene == null) 
            return false;

        //need to convert pixels to meters
        //Conversion(5000, "px", 1, "m");

        int loc = 0;
        int pixels = 300;
        foreach (var page in drawing.GetAllPages())
        {
            var shapes = page.AllShapes2D();
            if ( shapes.Count == 0 ) 
                continue;

            var wall = new FoPanel3D(page.Name)
            {
                Width = 10,
                Height = 10,
                Position = new Vector3(loc, 0, loc),
            };
            loc += 3;
            
            shapes?.ForEach(shape =>
            {
                var w = (double)shape.Width / pixels;
                var h = (double)shape.Height / pixels;
                var x = (double)shape.PinX / pixels;
                var y = (double)shape.PinY / pixels;
                var panel = new FoPanel3D(shape.Name)
                {
                    Width = w,
                    Height = h,
                    Color = shape.Color,
                    Position = new Vector3(x, 3 - y, loc - 0.1),
                };
                panel.TextLines.Add(shape.GetText());

                wall.Add<FoPanel3D>(panel);
            });
            wall.Render(Scene, 0, 0);
        }
        return true;
    }

    public bool RenderWorld3D(FoWorld3D world)
    {
        if (world == null) return false;

        $"RenderWorld {world.Name}".WriteNote();

        PreRenderWorld3D(world);
        return RenderWorld3DToScene(world);
    }


    public void PreRenderWorld3D(FoWorld3D? world)
    {
        $"PreRenderWorld world={world}".WriteInfo();
        if (world == null)
        {
            $"world is empty or viewer is not preent".WriteError();
            return;
        }

        var bodies = world.ShapeBodies();
        if (bodies != null)
            PreRenderShape3D(bodies);
    }


    public bool RenderWorld3DToScene(FoWorld3D? world)
    {

        if (world == null || Scene == null)
        {
            $"world is empty or viewer is not present".WriteError();
            return false;
        }

        //$"RenderPlatformToScene Bodies() {platform.Bodies()?.Count}  Labels() {platform.Labels()?.Count}  ".WriteSuccess();

        world.ShapeBodies()?.ForEach(body =>
        {
            // $"RenderPlatformToScene Body Name={body.Name} Type={body.Type}".WriteInfo();
            body.Render(Scene, 0, 0);
            var pos = body.Position;
            var x = (int)(pos?.X ?? 0) + 3;
            var y = (int)(pos?.Y ?? 0) + 2;
            body.NavMenu?.Render(Scene, x, y);
            body.TextPanel?.Render(Scene, x, y);
        });

        world.Labels()?.ForEach(label =>
        {
            //$"RenderPlatformToScene Label Name={label.Name} Text={label.Text}".WriteInfo();
            label.Render(Scene, 0, 0);
        });

        world.Datums()?.ForEach(datum =>
        {
            // $"RenderPlatformToScene Datum {datum.Name}".WriteInfo();
            datum.Render(Scene, 0, 0);
        });



        //RefreshUI();
        //PubSub!.Publish<RefreshUIEvent>(new RefreshUIEvent("RenderPlatformToScene"));
        return true;
    }


    public void PreRenderShape3D(List<FoShape3D> shapes)
    {

        var glbBodies = shapes.Where((body) => body.Type.Matches("Glb")).ToList();
        var otherBodies = shapes.Where((body) => !body.Type.Matches("Glb")).ToList();

        var bodyDict = glbBodies
            .GroupBy(item => item.Symbol)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var keyValuePair in bodyDict)
        {
            FoShape3D.PreRenderClones(keyValuePair.Value, this, Viewer3D!, Import3DFormats.Gltf);
        }

        foreach (var body in otherBodies)
        {
            $"PreRenderPlatform Body {body.Name}".WriteInfo();
            body.PreRender(this, Viewer3D!);
        };

    }

}
