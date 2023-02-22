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


namespace FoundryBlazor.Shape;

public interface IArena
{
    Scene GetScene();
    Scene InitScene();
    ViewerSettings GetSettings();
    void RefreshUI();
    void SetViewer(Viewer viewer);
    void SetDoCreate(Action<CanvasMouseArgs> action);

    void RenderWorld(DT_World3D? world);
    DT_World3D MakeWorld();

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

        // var world = MakeWorld();
        // RenderWorld(world);

        // FillScene();
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

    public DT_World3D MakeWorld()
    {
        var platform = new UDTO_Platform()
        {
            uniqueGuid = Guid.NewGuid().ToString(),
            platformName = "RonTest",
            name = "RonTest"
        };
        platform.EstablishBox("Platform", 1, 1, 1);

        var largeBlock = platform.CreateUsing<UDTO_Body>("LargeBlock").CreateBox("Large", 3, 1, 2);
        largeBlock.position = new HighResPosition();

        var smallBlock = platform.CreateUsing<UDTO_Body>("SmallBlock").CreateBox("SmallBlock", 1.5, .5, 1);
        smallBlock.position = new HighResPosition()
        {
            xLoc = -2.25,  //might need to changes sign
            yLoc = 0.75,
            zLoc = 1.5,
            xAng = Math.PI / 180 * 0,
        };

        platform.CreateUsing<UDTO_Label>("Label-1").CreateTextAt("Hello", -1.0, 2.0, 1.0)
            .position = new HighResPosition();


        var world = new DT_World3D()
        {
            title = "Sample world",
            description = "First test of Canvas 3D"
        };
        world.FillWorldFromPlatform(platform);


        return world;
    }

    public void RenderWorld(DT_World3D? world)
    {
        if (world == null) return;
        world.FillPlatforms();

        $"RenderWorld {world.name}".WriteLine(ConsoleColor.Blue);

        Task.Run(async () =>
        {
            await RenderToScene(world);
            await Viewer3D!.UpdateScene();
        });

    }

    public async Task RenderToScene(DT_World3D? world)
    {
        if (world == null) return;

        var scene = GetScene();
        await Viewer3D!.ClearSceneAsync();

        world.platforms?.ForEach(platform =>
        {

            platform.bodies?.ForEach(body =>
            {
                $"RenderToScene Body {body.name}".WriteLine(ConsoleColor.Blue);
                var shape = new FoShape3D(body.name, "Red")
                {
                    Body = body
                };

                shape.Render(Viewer3D!, scene, 0, 0);
            });

            platform.labels?.ForEach(label =>
            {
                $"RenderToScene Label {label.name}".WriteLine(ConsoleColor.Blue);
                var shape = new FoText3D(label.name, "Yellow")
                {
                    Label = label
                };

                shape.Render(Viewer3D!, scene, 0, 0);
            });
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