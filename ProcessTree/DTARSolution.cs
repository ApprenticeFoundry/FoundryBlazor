using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;

using BlazorComponentBus;
using FoundryBlazor.Canvas;
using FoundryBlazor.Services;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;
using FoundryBlazor.Extensions;
using IoBTModules.Extensions;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;
public interface IDTARSolution
{
    Task<List<UDTO_File>> UploadFile(IBrowserFile file);
    Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args);

    string GetServerUrl();
    Dictionary<string, Action> DocumentMenuItems(string name, IJSRuntime js);
    Dictionary<string, Action> ProcessMenuItems(string name, IJSRuntime js);
    Dictionary<string, Action> WorldMenuItems(string name, IJSRuntime js);
}

public class DTARSolution : DT_Manager, IDTARSolution
{
    private IToast? Toast { get; set; }
    private IDrawing? Drawing { get; set; }
    private IArena? Arena { get; set; }
    private ComponentBus PubSub { get; set; }
    private IRestAPIServiceDTAR DTARRestService { get; set; }
    private FoLayoutTree<FoHero2D>? CurrentLayout { get; set; }


    private readonly Dictionary<string, DT_Title> _Models = new();

    public DTARSolution( 
        ComponentBus pubsub,
        IToast toast,
        IRestAPIServiceDTAR dtar, 
        IDrawing drawing, 
        IArena arena) : base("DTARSolution")
    {
        Toast = toast;
        DTARRestService = dtar;
        PubSub = pubsub;
        Drawing = drawing;
        Arena = arena;

        PubSub.SubscribeTo<AttachAssetFileEvent>(obj =>
        {
            if ( FindModel(obj.AssetFile.Name) is DT_AssetFile asset) {
                if ( FindModel(obj.Target.Name) is DT_Hero target) {
                    AddAssetReference(target, asset);
                    if ( CurrentLayout != null)
                    {
                        var node = CurrentLayout.FindNodeWithName(obj.Target.Name);
                        var child = new FoLayoutTree<FoHero2D>(obj.AssetFile);
                        node?.AddChildNode(child);  
                        LayoutTree(CurrentLayout);
                    }
                }
            };

            //you need to send this to the cloud Right?
        });
    }

    private DT_AssetFile CreateAssetFile(UDTO_File source)
    {
        var asset = new DT_AssetFile
        {
            name = source.filename.CleanToFilename(),
            filename = source.filename,
            docType = source.mimeType,
            url = source.url
        };
        AddModel(asset);
        return asset;
    }

    public async Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args)
    {

        $"DropFileCreateShape {file.Name} {args.OffsetX} {args.OffsetY}".WriteLine(ConsoleColor.Green);
        var shape = new FoHero2D();
        shape.MoveTo(args.OffsetX, args.OffsetY);
        shape.ResizeTo(200, 100);

        shape.Color = "Orange";
        shape.Title = "Uploading";
        Drawing?.AddShape<FoHero2D>(shape);

        try {
            var result = await DTARRestService.UploadFile(file);
            var fileInfo = result.FirstOrDefault();
            if ( fileInfo != null)
            {
                var asset = CreateAssetFile(fileInfo);
                asset.title = fileInfo.filename;
                shape.TagWithModel(asset,"Pink");

                Toast?.Success($"Uploaded {asset.name} {asset.url}");
                //$"DropFileCreateShape {asset.name} {asset.url}".WriteLine(ConsoleColor.Green);
            }
        } 
        catch ( Exception ex)
        {
            shape.Tag = ex.Message;
            Toast?.Error(ex.Message);
        }
    }

    public async Task<List<UDTO_File>> UploadFile(IBrowserFile file)
    {
        var result =  await DTARRestService.UploadFile(file);
        var fileInfo = result.FirstOrDefault();
        if (fileInfo != null) 
            $"File Upload Complete {fileInfo.filename} {fileInfo.url} {fileInfo.mimeType}".WriteLine(ConsoleColor.Green);
        else
            $"File Upload Failed ".WriteLine(ConsoleColor.Red);

        return result;
    }
    
    public string GetServerUrl()
    {
        return DTARRestService.GetServerUrl();
    }

    private DT_Title? FindModel(string key)
    {
        if ( _Models.TryGetValue(key, out DT_Title? model)) return model;
        return null;
    }
    private DT_Title AddModel(DT_Title model)
    {
        if ( !_Models.ContainsKey(model.guid) )  {
            _Models.Add(model.guid, model);
        }

        return model;
    }


    public Dictionary<string, Action> ProcessMenuItems(string name, IJSRuntime js)
    {

        var menu = new Dictionary<string, Action>()
        {
            { "Test Process", () => SetDoCreateProcess(MakeProcess()) },
        };

        //watch me extend this menu after a service call
        Task.Run(async () =>
        {
            var plans = await DTARRestService.GetAllProcessPlans();
            plans.ForEach(item =>
            {
                menu.Add(item.title, () => SetDoCreateProcess(item));
            });

            Drawing?.EstablishMenu<FoMenu2D>(name, menu,true);
            FoPage2D.RefreshMenus = true;
        });

        return menu;
    }


    public Dictionary<string, Action> DocumentMenuItems(string name, IJSRuntime js)
    {
        var menu = new Dictionary<string, Action>()
        {
            { "Test Document", () => SetDoCreateDocuments(MakeDocument()) },
        };

        //watch me extend this menu after a service call
        Task.Run(async () =>
        {
            var docs = await DTARRestService.GetRootDocument();
            docs?.ForEach(item =>
            {
                if (item == null) return;
                menu.Add(item.title, () => SetDoCreateDocuments(item));
            });

            Drawing?.EstablishMenu<FoMenu2D>(name, menu,true);
            FoPage2D.RefreshMenus = true;
        });

        return menu;
    }




    public static DT_MILDocument MakeDocument()
    {
        var doc = new DT_MILDocument() { title = "Root"};

        var step1 = new DT_MILDocument(){ title = "Child1"};
        doc.AddChild(step1);
        var step2 = new DT_MILDocument(){ title = "Child2"};
        doc.AddChild(step2);

        AddDocument(step1, "File1");
        AddDocument(step2, "File2");
        return doc;
    }

    public Dictionary<string, Action> WorldMenuItems(string name, IJSRuntime js)
    {

        var menu = new Dictionary<string, Action>()
        {
           // { "Create World", () => EstablishWorld() },
            { "Test World", () => {
                var world = Arena?.MakeWorld();
                $"we created a world".WriteLine();
                Arena?.RenderWorld(world);
             } },
        };

        //watch me extend this menu after a service call
        Task.Run(async () =>
        {
            var docs = await DTARRestService.GetWorlds();
            docs.ForEach(item =>
            {
                menu.Add(item.title, () => Arena?.RenderWorld(item));
            });

            Arena?.EstablishMenu<FoMenu3D>(name, menu,true);
            FoPage2D.RefreshMenus = true;
        });

        return menu;
    }

    private void BuildPlatform(UDTO_Platform platform, FoLayoutTree<FoHero2D> source)
    {
        var shape = source.GetShape();
        var model = FindModel(shape.Name);
        if ( model == null)
            return;

        var pixelsPerMeter = 200; 
        var label = platform.FindOrCreate<UDTO_Label>(model.guid, true);
        var details = FoHero2D.CreateTextList( model.description, 25);
        var title = $"{source.ComputeName()} {model.title}";


        label.CreateLabelAt(title, details, shape.PinX / pixelsPerMeter, shape.PinY / pixelsPerMeter, 10 * source.level);

        var children = source.GetChildren();
        children?.ForEach(item => BuildPlatform(platform,item));
    }

    public  void EstablishWorld()
    {
        if ( CurrentLayout == null ) return;

        var shape = CurrentLayout.GetShape();
        var model = FindModel(shape.Name);
        if ( model == null)
            return;

        var world = Find<DT_World3D>(shape.Name);
        if ( world == null ) 
        {
            var platform = new UDTO_Platform()
            {
                uniqueGuid =  shape.Name,   
                platformName = shape.Title
            };
            CurrentLayout.GetChildren()?.ForEach(item => BuildPlatform(platform,item));
            
            world = Establish<DT_World3D>(shape.Name);
            world.title = model.title;
            world.description = model.description;
            world.FillWorldFromPlatform(platform);
        }
        DTARRestService.WorldAddOrUpdate(world);
    }

    public static DT_ProcessPlan MakeProcess()
    {
        var process = new DT_ProcessPlan();

        var step1 = new DT_ProcessStep();
        process.AddProcessStep(step1);

        var item1_1 = new DT_StepItem();
        var item1_2 = new DT_StepItem();
        var item1_3 = new DT_StepItem();
        var item1_4 = new DT_StepItem();
        var item1_5 = new DT_StepItem();
        step1.AddStepDetail<DT_StepItem>(item1_1);
        step1.AddStepDetail<DT_StepItem>(item1_2);
        step1.AddStepDetail<DT_StepItem>(item1_3);
        step1.AddStepDetail<DT_StepItem>(item1_4);
        step1.AddStepDetail<DT_StepItem>(item1_5);

        var step2 = new DT_ProcessStep();
        process.AddProcessStep(step2);

        var item2_1 = new DT_StepItem();
        var item2_2 = new DT_StepItem();
        var item2_3 = new DT_StepItem();
        var item2_4 = new DT_StepItem();
        AddDocument(item2_4, "File1 item2_4");
        AddDocument(item2_4, "File2 item2_4");

        step2.AddStepDetail<DT_StepItem>(item2_1);
        step2.AddStepDetail<DT_StepItem>(item2_2);
        step2.AddStepDetail<DT_StepItem>(item2_3);
        step2.AddStepDetail<DT_StepItem>(item2_4);


        var step3 = new DT_ProcessStep();
        process.AddProcessStep(step3);
        AddDocument(step3, "File1 step3");
        AddDocument(step3, "File2 step3");

        var item3_1 = new DT_StepItem();
        var item3_2 = new DT_StepItem();
        var item3_3 = new DT_StepItem();
        var item3_4 = new DT_StepItem();
        var item3_5 = new DT_StepItem();
        step3.AddStepDetail<DT_StepItem>(item3_1);
        step3.AddStepDetail<DT_StepItem>(item3_2);
        step3.AddStepDetail<DT_StepItem>(item3_3);
        step3.AddStepDetail<DT_StepItem>(item3_4);
        step3.AddStepDetail<DT_StepItem>(item3_5);

        var step4 = new DT_ProcessStep();
        process.AddProcessStep(step4);

        var item4_1 = new DT_StepItem();
        var item4_2 = new DT_StepItem();

        step4.AddStepDetail<DT_StepItem>(item4_1);
        step4.AddStepDetail<DT_StepItem>(item4_2);


        return process;
    }

    private static DT_AssetFile AddDocument(DT_Hero hero, string filename)
    {
        var asset = new DT_AssetFile() { filename = filename };
        return AddAssetReference(hero, asset);
    }

    private static DT_AssetFile AddAssetReference(DT_Hero hero, DT_AssetFile asset)
    {
        var docRef = new DT_AssetReference()
        {
            asset = asset,
            assetGuid = asset.guid,
            heroGuid = hero.guid,
        };
        hero.AddAssetReference<DT_AssetReference>(docRef);
        return asset;
    }

    public void AttachItem<V>(FoLayoutTree<V> node, DT_AssetFile item) where V : FoHero2D
    {
        AddModel(item);
        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(item,"Pink");
        Drawing?.AddShape<V>(shape);
                        
        //$"CreateAssetFile {shape.Tag} {shape.Name}".WriteLine(ConsoleColor.Yellow);
        var child = new FoLayoutTree<V>(shape);
        node.AddChildNode(child);       
    }

    public FoLayoutTree<V> CreateAssetFileShapeTree<V>(FoLayoutTree<V> node, DT_Hero model) where V : FoHero2D
    {

        var list = model.CollectAssetFiles(new List<DT_AssetFile>(), false);
        var assets = list.Where(item => item != null).ToList();
        assets.ForEach(item => AttachItem(node, item));
        return node;
    }

    public FoLayoutTree<V> CreateItemShapeTree<V>(DT_Hero model) where V : FoHero2D
    {
        AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model,"Blue");
        Drawing?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        CreateAssetFileShapeTree(node, model);

        return node;
    }

    public FoLayoutTree<V> CreateStepShapeTree<V>(DT_Hero model) where V : FoHero2D
    {
        AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model,"Black").ResizeTo(250, 90);
        Drawing?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        model.Children()?.ForEach(item =>
        {
            var subNode = CreateItemShapeTree<V>(item);
            node.AddChildNode(subNode);
        });
        CreateAssetFileShapeTree(node, model);

        return node;
    }

    public FoLayoutTree<V> CreatePlanShapeTree<V>(DT_Hero model) where V : FoHero2D
    {
        AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model,"Red").ResizeTo(250, 80);
        Drawing?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        model.Children()?.ForEach(step =>
        {
            var subNode = CreateStepShapeTree<V>(step);
            node.AddChildNode(subNode);
        });

        CreateAssetFileShapeTree(node, model);

        return node;
    }

    public FoLayoutTree<V> CreateDocumentShapeTree<V>(DT_MILDocument model) where V : FoHero2D
    {
        AddModel(model);

        var shape = Activator.CreateInstance<V>();
        shape.TagWithModel(model, "Orange");
        Drawing?.AddShape<V>(shape);

        var node = new FoLayoutTree<V>(shape);
        model.children?.ForEach(child =>
        {
            var subnode = CreateDocumentShapeTree<V>(child);
            node.AddChildNode(subnode);
        });
        CreateAssetFileShapeTree(node, model);

        return node;
    }

    private void SetDoCreateProcess(DT_ProcessPlan model)
    {
        AddModel(model);

        Add<DT_ProcessPlan>(model);
        Drawing?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var margin = new Point(20, 50); 
            var page = Drawing.CurrentPage();
            var pt = Drawing.InchesToPixelsInset(page.PageWidth / 2, 5.0);

            CurrentLayout = CreatePlanShapeTree<FoHero2D>(model);
            CurrentLayout.HorizontalLayout(pt.X, pt.Y, margin, LayoutRules.ProcessLayout);

            CurrentLayout.HorizontalLayoutConnections<FoConnector1D>(Drawing.Pages());

            var shape = CurrentLayout.GetShape();
            //shape.Tag = $"Node: {_layout.ComputeName()}";
        });
    }

    private void SetDoCreateWorld(DT_World3D model)
    {
        AddModel(model);

        Add<DT_World3D>(model);
        Arena?.SetDoCreate((CanvasMouseArgs args) =>
        {
            var margin = new Point(20, 50);;
            // var page = Arena.CurrentPage();
            // var pt = Drawing.InchesToPixelsInset(page.PageWidth/2, 5.0);

            // Layout = CreateDocumentShapeTree<FoHero2D>(model, 0, 0);
            // Layout.HorizontalLayout(pt.X, pt.Y, margin, LayoutRules.ProcessLayout);

            // Layout.HorizontalLayoutConnections<FoConnector1D>(Drawing.Pages());
            
            // var shape = Layout.GetShape();

        });
    }

    private void SetDoCreateDocuments(DT_MILDocument model)
    {
        AddModel(model);

        Add<DT_MILDocument>(model);
        Drawing?.SetDoCreate((CanvasMouseArgs args) =>
        {
            CurrentLayout = CreateDocumentShapeTree<FoHero2D>(model);

            LayoutTree(CurrentLayout);
            //var shape = CurrentLayout.GetShape();
            //shape.Tag = $"Node: {_layout.ComputeName()}";
        });
    }

    private void LayoutTree(FoLayoutTree<FoHero2D> layout)
    {
        var margin = new Point(20, 50);
        var page = Drawing!.CurrentPage();
        var pt = Drawing!.InchesToPixelsInset(page.PageWidth/3, 5.0);

        layout.HorizontalLayout(pt.X, pt.Y, margin, LayoutRules.ProcessLayout);

        layout.HorizontalLayoutConnections<FoConnector1D>(Drawing.Pages());
    }

    public async Task RenderTree(Canvas2DContext ctx)
    {
        if (CurrentLayout != null)
            await CurrentLayout.RenderTree(ctx);
    }




}
