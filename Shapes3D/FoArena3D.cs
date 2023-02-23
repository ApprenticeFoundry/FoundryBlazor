using BlazorThreeJS.Events;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Lights;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Scenes;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Viewers;

using BlazorComponentBus;

using FoundryBlazor.PubSub;

using FoundryBlazor.Shared;

using FoundryBlazor.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryBlazor.Canvas;
using System.Runtime.CompilerServices;

namespace FoundryBlazor.Shape;

public interface IArena
{
    Scene GetScene();
    Scene InitScene();
    Task ClearViewer3D();
    ViewerSettings GetSettings();

    void RefreshUI();
    void SetViewer(Viewer viewer);
    void SetDoCreate(Action<CanvasMouseArgs> action);

    void RenderWorld(FoWorld3D? world);
    void RenderPlatformToScene(FoGroup3D? platform);
    FoGroup3D MakeTestPlatform();

    List<IFoMenu> CollectMenus(List<IFoMenu> list);
    FoMenu3D EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu3D;
    void CreateMenus(IJSRuntime js, NavigationManager nav);
}
public class FoArena3D : FoGlyph3D, IArena
{
    private IToast Toast { get; set; }
    private ICommand Command { get; set; }
    private IJSRuntime JsRuntime { get; set; }

    private ISceneManagement SceneManager { get; set; }

    public Action<CanvasMouseArgs>? DoCreate { get; set; }

    private Viewer? Viewer3D { get; set; }
    private ComponentBus PubSub { get; set; }


    public ViewerSettings settings = new()
    {
        CanSelect = true,// default is false
        SelectedColor = "black",
        WebGLRendererSettings = new WebGLRendererSettings
        {
            Antialias = false // if you need poor quality for some reasons
        }
    };

    public FoArena3D(
        ICommand cmd,
        IToast toast,
        ISceneManagement sceneManagement,
        ComponentBus pubSub,
        IJSRuntime jsRuntime)
    {
        Command = cmd;
        Toast = toast;
        SceneManager = sceneManagement;
        JsRuntime = jsRuntime;
        PubSub = pubSub;
    }

    public Scene GetScene()
    {
        return SceneManager.CurrentScene().GetScene();
    }
    public Scene InitScene()
    {
        var scene = this.GetScene();
        if (scene != null)
        {
            scene.Add(new AmbientLight());
            scene.Add(new PointLight()
            {
                Position = new Vector3(1, 3, 0)
            });
        }

        return SceneManager.CurrentScene().GetScene();
    }

    public async Task ClearViewer3D()
    {
        if (Viewer3D != null)
            await Viewer3D.ClearSceneAsync();
    }

    public ViewerSettings GetSettings()
    {
        return settings;
    }

    public void SetViewer(Viewer viewer)
    {
        Viewer3D = viewer;
    }

    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        return SceneManager.CollectMenus(list);
    }

    public FoMenu3D EstablishMenu<T>(string name, Dictionary<string, Action> menu, bool clear) where T : FoMenu3D
    {
        var result = SceneManager.EstablishMenu3D<T, FoButton3D>(name, menu, clear);
        return result;
    }

    public void CreateMenus(IJSRuntime js, NavigationManager nav)
    {
        //var plan = "ProcessPlan";
        //ProcessPlan = new DTAR_Drawing(DTARRestService, PageManager, this, ActiveScene, PubSub);
        //EstablishMenu<FoMenu2D>(plan, ProcessPlan.ProcessMenuItems(plan, js), true);

        // var name = "World";
        // AllWorlds = new DTARSolution(DTARRestService, null, this);
        // EstablishMenu<FoMenu3D>(name, AllWorlds.WorldMenuItems(name, js), true);

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
            $" DoCreate {action.Method.Name} {ex.Message}".WriteLine();
        }
    }

    public void RefreshUI()
    {
        PubSub!.Publish<RefreshUIEvent>(new RefreshUIEvent());
    }

    public FoGroup3D MakeTestPlatform()
    {
        var platform = new FoGroup3D()
        {
            UniqueGuid = Guid.NewGuid().ToString(),
            PlatformName = "RonTest",
            Name = "RonTest"
        };
        platform.EstablishBox("Platform", 1, 1, 1);

        var largeBlock = platform.CreateUsing<FoShape3D>("LargeBlock").CreateBox("Large", 3, 1, 2);
        largeBlock.Position = new FoVector3D();

        var smallBlock = platform.CreateUsing<FoShape3D>("SmallBlock").CreateBox("SmallBlock", 1.5, .5, 1);
        smallBlock.Position = new FoVector3D()
        {
            X = -2.25,  //might need to changes sign
            Y = 0.75,
            Z = 1.5,
        };

        platform.CreateUsing<FoText3D>("Label-1").CreateTextAt("Hello", -1.0, 2.0, 1.0)
            .Position = new FoVector3D();


        return platform;
    }

    public void RenderWorld(FoWorld3D? world)
    {
        if (world == null) return;
        world.FillPlatforms();

        $"RenderWorld {world.Name}".WriteLine(ConsoleColor.Blue);

        Task.Run(async () =>
        {
            await RenderWorldToScene(world);
            await Viewer3D!.UpdateScene();
        });

    }



    public async Task RenderWorldToScene(FoWorld3D? world)
    {
        $"world={world}".WriteInfo();
        if (world == null || Viewer3D == null)
        {
            $"world is empty or viewer is not preent".WriteError();
            return;
        }

        var scene = GetScene();
        $"scene={scene}".WriteInfo();

        await ClearViewer3D();
        $"cleared scene".WriteInfo();

        $"Platforms Count={world.Platforms()?.Count}".WriteInfo();

        world.Platforms()?.ForEach(RenderPlatformToScene);
    }

    public void RenderPlatformToScene(FoGroup3D? platform)
    {
        $"platform={platform}".WriteInfo();
        if (platform == null || Viewer3D == null)
        {
            $"platform is empty or viewer is not present".WriteError();
            return;
        }

        var scene = GetScene();
        $"scene={scene}".WriteInfo();


        platform.Bodies()?.ForEach(body =>
        {
            $"RenderPlatformToScene Body {body.Name}".WriteInfo();
            body.Render(Viewer3D!, scene, 0, 0);
        });

        platform.Labels()?.ForEach(label =>
        {
            $"RenderPlatformToScene Label {label.Name}".WriteInfo();
            label.Render(Viewer3D!, scene, 0, 0);
        });

        platform.Datums()?.ForEach(datum =>
        {
            $"RenderPlatformToScene Datum {datum.Name}".WriteInfo();
            datum.Render(Viewer3D!, scene, 0, 0);
        });

    }

    public void FillScene()
    {
        var scene = GetScene();
        scene.Add(new AmbientLight());
        scene.Add(new PointLight()
        {
            Position = new Vector3(1, 3, 0)
        });
        scene.Add(new Mesh());
        scene.Add(new Mesh
        {
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(-2, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "magenta"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CircleGeometry(radius: 0.75f, segments: 12),
            Position = new Vector3(2, 0, 0),
            Scale = new Vector3(1, 0.75f, 1),
            Material = new MeshStandardMaterial()
            {
                Color = "#98AFC7"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CapsuleGeometry(radius: 0.5f, length: 2),
            Position = new Vector3(-4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Position = new Vector3(4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "green",
                FlatShading = true,
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: 0.5f, height: 1.2f, radialSegments: 16),
            Position = new Vector3(0, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "red",
                Wireframe = true
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: 0.8f),
            Position = new Vector3(-2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "darkviolet",
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: 0.8f),
            Position = new Vector3(-4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "violet"
            }
        });

        scene.Add(new Mesh
        {

            Geometry = new OctahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "aqua"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new PlaneGeometry(width: 0.5f, height: 2),
            Position = new Vector3(4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "purple"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new RingGeometry(innerRadius: 0.6f, outerRadius: 0.7f),
            Position = new Vector3(0, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "DodgerBlue"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new SphereGeometry(radius: 0.6f),
            Position = new Vector3(-2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            },
        });
        scene.Add(new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightblue"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new TorusGeometry(radius: 0.6f, tube: 0.4f, radialSegments: 12, tubularSegments: 12),
            Position = new Vector3(4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightgreen"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new TorusKnotGeometry(radius: 0.6f, tube: 0.1f),
            Position = new Vector3(-4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "RosyBrown"
            }
        });
    }


}