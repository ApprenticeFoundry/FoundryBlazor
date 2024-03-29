using BlazorComponentBus;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Viewers;
 
using FoundryBlazor.Extensions;
using FoundryBlazor.PubSub;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FoundryBlazor.Shape;

public interface IArena
{
    void SetScene(Scene scene, Viewer viewer);
    Task RenderArena(Scene scene, int tick, double fps);
    Task ClearArena();
    Task UpdateArena();
    void SetDoCreate(Action<CanvasMouseArgs> action);

    bool RenderDrawingToScene(IDrawing drawing);
    bool RenderWorld3DToScene(FoWorld3D world);
    bool RenderWorld3D(FoWorld3D world);
    Task<bool> PreRender(FoGlyph3D glyph);

    Task<bool> RemoveShapeFromScene(FoShape3D shape);

    V AddShape<V>(V shape) where V : FoGlyph3D;
    V RemoveShape<V>(V shape) where V : FoGlyph3D;

    FoStage3D CurrentStage();

    //FoWorld3D StressTest3DModelFromFile(string folder, string filename, string baseURL, int count);
    //FoWorld3D Load3DModelFromFile(UDTO_Body spec, string folder, string filename, string baseURL);
    void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav);
    //void SetCanvasSizeInPixels(int width, int height);

}
public class FoArena3D : FoGlyph3D, IArena
{
    public Viewer? Viewer3D { get; set; }
    public Scene? Scene { get; set; }
    private IStageManagement StageManager { get; set; }
    //private int TrueCanvasWidth = 0;
    //private int TrueCanvasHeight = 0;

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

    // public void SetCanvasSizeInPixels(int width, int height)
    // {
    //     TrueCanvasWidth = width;
    //     TrueCanvasHeight = height;
    // }

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

    public V RemoveShape<V>(V shape) where V : FoGlyph3D
    {
        return StageManager.RemoveShape<V>(shape);
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
    public void SetScene(Scene scene, Viewer viewer)
    {
        Viewer3D = viewer;
        Scene = scene;
        CurrentStage().InitScene(scene,viewer);
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



    //public FoWorld3D StressTest3DModelFromFile(string folder, string filename, string baseURL, int count)
    //{
    //    var name = Path.GetFileNameWithoutExtension(filename);


    //    var world3D = new UDTO_World();
    //    var data = new MockDataMaker();
    //    var url = Path.Join(baseURL, folder, filename);

    //    var root = new DT_Hero();

    //    for (int i = 0; i < count; i++)
    //    {
    //        root.name = $"{name}-{i}";
    //        var shape = world3D.CreateGlb(root, url, 1, 2, 3);
    //        shape.EstablishLoc(data.GenerateDouble(-5, 5), data.GenerateDouble(-5, 5), data.GenerateDouble(-5, 5), "m");
    //        shape.EstablishAng(data.GenerateDouble(0, 360), data.GenerateDouble(0, 360), data.GenerateDouble(0, 360), "r");
    //    };


    //    var world = new FoWorld3D(world3D);
    //    RenderWorld3D(world);

    //    //PostRenderplatform

    //    return world;
    //}



    //public FoWorld3D Load3DModelFromFile(UDTO_Body spec, string folder, string filename, string baseURL)
    //{
    //    var name = Path.GetFileNameWithoutExtension(filename);

    //    var url = Path.Join(baseURL, folder, filename);

    //    var root = new DT_Hero
    //    {
    //        name = name,
    //        guid = spec.uniqueGuid,
    //    };


    //    var world3D = new UDTO_World();
    //    var body = world3D.CreateGlb(root, url);
    //    body.boundingBox = spec.boundingBox;
    //    body.position = spec.position;

    //    var world = new FoWorld3D(world3D);
    //    RenderWorld3D(world);

    //    return world;
    //}



    public bool RenderDrawingToScene(IDrawing drawing)
    {
        if (Scene == null)
            return false;

        var north = new FoPanel3D("-Z Wall")
        {
            Position = new Vector3(0, 7, -14.8),
            Rotation = new Euler(0, Math.PI * 0 / 180, 0),
        };
        var south = new FoPanel3D("+Z Wall")
        {
            Position = new Vector3(0, 7, 14.8),
            Rotation = new Euler(0, Math.PI * 180 / 180, 0),
        };
        var east = new FoPanel3D("-X Wall")
        {
            Position = new Vector3(-14.8, 7, 0),
            Rotation = new Euler(0, Math.PI * 90 / 180, 0),
        };
        var west = new FoPanel3D("+X Wall")
        {
            Position = new Vector3(14.8, 7, 0),
            Rotation = new Euler(0, Math.PI * 270 / 180, 0),
        };
        //need to convert pixels to meters
        //Conversion(5000, "px", 1, "m");

        var queue = new Queue<FoPanel3D>();
        queue.Enqueue(north);
        queue.Enqueue(east);
        queue.Enqueue(west);
        queue.Enqueue(south);


        var pixels = 100;
        var z = 0.1;
        foreach (var page in drawing.GetAllPages())
        {

            var shapes = page.AllShapes2D();
            if (shapes.Count == 0)
                continue;

            var wall = queue.Dequeue();
            wall.Width = 16;
            wall.Height = 16;
            wall.Color = page.Color;
            var halfW = wall.Width / 2;
            var halfH = wall.Height / 2;

            shapes?.ForEach(shape =>
            {
                var w = (double)shape.Width / pixels;
                var h = (double)shape.Height / pixels;
                var x = (double)shape.PinX / pixels;
                var y = (double)shape.PinY / pixels;
                var panel = new FoPanel3D(shape.Key)
                {
                    Width = w,
                    Height = h,
                    Color = shape.Color,
                    // Position = Placement(page.Name, wall.Position!, x, y),
                    Position = new Vector3(x - halfW, halfH - y, z),
                };
                var textLines = shape.GetText().Split('_').ToList();
                panel.TextLines.AddRange(textLines);

                wall.Add<FoPanel3D>(panel);
            });

            var lineShapes = page.AllShapes1D();
            var pathZ = z - 0.1;

            foreach (var lineShape in lineShapes)
            {
                var X1 = lineShape.StartX / pixels;
                var Y1 = lineShape.StartY / pixels;
                var X2 = lineShape.FinishX / pixels;
                var Y2 = lineShape.FinishY / pixels;
                var path = new List<Vector3>() {
                    new Vector3(X1 - halfW, halfH - Y1, pathZ),
                    new Vector3(X2 - halfW, halfH - Y2, pathZ)
                };
                var pathway = new FoPathway3D(lineShape.GetName())
                {
                    Path = path,
                    Color = "black"
                };

                wall.Add(pathway);
            }

            wall.Render(Scene, 0, 0);
        }
        return true;
    }

    public bool RenderWorld3D(FoWorld3D world)
    {
        if (world == null) return false;

        $"RenderWorld {world.Key}".WriteNote();

        PreRenderWorld3D(world);
        return RenderWorld3DToScene(world);
    }


    public void PreRenderWorld3D(FoWorld3D? world)
    {
        //$"PreRenderWorld world={world}".WriteInfo();
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


        world.ShapeBodies()?.ForEach(body =>
        {
            // $"RenderPlatformToScene Body Name={body.Name} Type={body.Type}".WriteInfo();
            body.Render(Scene, 0, 0);
        });

        world.Labels()?.ForEach(label =>
        {
            //$"RenderPlatformToScene Label Name={label.Name} Text={label.Text}".WriteInfo();
            label.Render(Scene, 0, 0);
        });

        world.Panels()?.ForEach(panel =>
        {
            //$"RenderPlatformToScene Label Name={label.Name} Text={label.Text}".WriteInfo();
            panel.Render(Scene, 0, 0);
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

    public async Task<bool> RemoveShapeFromScene(FoShape3D shape)
    {
        if ( Scene == null)
            return false;

        return await shape.RemoveFromRender(CurrentScene(), CurrentViewer());
    }


    public async void PreRenderShape3D(List<FoShape3D> shapes)
    {

        var glbBodies = shapes.Where((body) => body.Type.Matches("Glb")).ToList();
        var otherBodies = shapes.Where((body) => !body.Type.Matches("Glb")).ToList();

        var bodyDict = glbBodies
            .GroupBy(item => item.Url)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var keyValuePair in bodyDict)
        {
            await FoShape3D.PreRenderClones(keyValuePair.Value, this, Viewer3D!, Import3DFormats.Gltf);
        }

        foreach (var body in otherBodies)
        {
            //$"PreRenderPlatform Body {body.Name}".WriteInfo();
            await body.PreRender(this, Viewer3D!);
        };

    }




}
