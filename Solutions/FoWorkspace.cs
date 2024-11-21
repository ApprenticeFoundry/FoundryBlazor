using Blazor.Extensions.Canvas.Canvas2D;
using BlazorComponentBus;
 

using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;


using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Microsoft.JSInterop;
using Radzen;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;


namespace FoundryBlazor.Solutions;

public enum ViewStyle { None, View2D, View3D }
public enum InputStyle { None, Drawing, FileDrop }



public interface IWorkspace : IWorkbook
{
    string GetBaseUrl();
    string SetBaseUrl(string url);

    Task InitializedAsync(string defaultHubURI);
    IDrawing GetDrawing();
    IArena GetArena();
    ISelectionService GetSelectionService();
    IFoundryService GetFoundryService();

    string GetUserID();
    ViewStyle GetViewStyle();
    void SetViewStyle(ViewStyle style);
    bool IsViewStyle2D();
    bool IsViewStyle3D();

    U EstablishCommand<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoCommand2D;
    U EstablishMenu2D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoMenu2D;
    U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D;


    void ClearAllWorkbook();
    List<FoWorkbook> AllWorkbooks();
    List<FoWorkbook> AddWorkbook(FoWorkbook book);
    T EstablishWorkbook<T>(string key) where T : FoWorkbook;

    FoWorkbook? FindWorkbook(string name);
    FoWorkbook CurrentWorkbook();
    FoWorkbook SetCurrentWorkbook(FoWorkbook book);


    Task LocalFileSave(string filename, byte[] data);
    Task<string> FileLoad(string filename);
    Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args);

    ComponentBus GetPubSub();
    void OnDispose();
}

public class FoWorkspace : FoComponent, IWorkspace
{
    public static bool RefreshCommands { get; set; } = true;
    public static bool RefreshMenus { get; set; } = true;

    protected string UserID { get; set; } = "";
    protected string CurrentUrl { get; set; } = "";

    public InputStyle InputStyle { get; set; } = InputStyle.Drawing;

    protected ViewStyle viewStyle = ViewStyle.View2D;

    private FoWorkbook ActiveWorkbook { get; set; }
    protected IDrawing ActiveDrawing { get; init; }
    protected IArena ActiveArena { get; init; }
    public ICommand Command { get; set; }
    public IPanZoomService PanZoom { get; set; }


    public IFoundryService Foundry { get; set; }
    protected IToast Toast { get; set; }
    protected ComponentBus PubSub { get; set; }
    protected IPopupDialog PopupDialog { get; set; }
    protected IJSRuntime JsRuntime { get; set; }
    protected ISelectionService SelectionService { get; set; }


    public Func<IBrowserFile, CanvasMouseArgs, Task> OnFileDrop { get; set; } = async (IBrowserFile file, CanvasMouseArgs args) => { await Task.CompletedTask; };


    protected Action SetDrawingStyle { get; set; }
    protected Action SetFileDropStyle { get; set; }

    public FoWorkspace(IFoundryService foundry)
    {
        Foundry = foundry;
        Toast = foundry.Toast();
        Command = foundry.Command();
        SelectionService = foundry.Selection();
        ActiveDrawing = foundry.Drawing();
        ActiveArena = foundry.Arena();
        PubSub = foundry.PubSub();
        PanZoom = foundry.PanZoom();
        PopupDialog = foundry.PopupDialog();
        JsRuntime = foundry.JS();

        ActiveWorkbook = EstablishWorkbook<CommonWorkbook>("Shared");

        SetDrawingStyle = async () =>
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("CanvasFileInput.HideFileInput");
                InputStyle = InputStyle.Drawing;
                await PubSub!.Publish<InputStyle>(InputStyle);
                "SetDrawingStyle".WriteWarning();
            }
            catch { }
        };

        SetFileDropStyle = async () =>
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("CanvasFileInput.ShowFileInput");
                InputStyle = InputStyle.FileDrop;
                await PubSub!.Publish<InputStyle>(InputStyle);
                "SetFileDropStyle".WriteWarning();
            }
            catch { }
        };
    }


    //https://stackoverflow.com/questions/52683706/how-can-one-generate-and-save-a-file-client-side-using-blazor
    public async Task LocalFileSave(string filename, byte[] data)
    {
        if (JsRuntime != null)
            await JsRuntime.InvokeAsync<object>("window.saveAsFile", filename, Convert.ToBase64String(data));
    }
    public async Task<string> FileLoad(string filename)
    {
        await Task.CompletedTask;
        return "";
    }

    public IFoundryService GetFoundryService()
    {
        return Foundry;
    }

    public virtual void PreRender(int tick)
    {
        AllWorkbooks()?.ForEach(item =>
        {
            if (item.IsActive)
                item.PreRender(tick);
        });
    }

    public virtual void PostRender(int tick)
    {
        AllWorkbooks()?.ForEach(item =>
        {
            if (item.IsActive)
                item.PostRender(tick);
        });
    }


    public FoWorkbook CurrentWorkbook()
    {
        if (ActiveWorkbook == null)
        {
            var found = AllWorkbooks().Where(book => book.IsActive).FirstOrDefault();
            if (found == null)
            {
                found = new FoWorkbook(this, Foundry)
                {
                    Key = "Book-1",
                    Name = "Book-1",
                };
                AddWorkbook(found);
            }
            ActiveWorkbook = found;
            ActiveWorkbook.IsActive = true;
        }

        return ActiveWorkbook;
    }

    public FoWorkbook SetCurrentWorkbook(FoWorkbook book)
    {
        //$"called SetCurrentWorkbook: {book.Key}".WriteSuccess();
        if (ActiveWorkbook == book) 
            return ActiveWorkbook;

        RefreshMenus = true;
        ActiveWorkbook = book;
        AllWorkbooks().ForEach(item => item.IsActive = false);
        ActiveWorkbook.IsActive = true;
        //$"SetCurrentWorkbook: {ActiveWorkbook.Key}".WriteSuccess();

        var drawing = GetDrawing();
        drawing.SetCurrentPage(ActiveWorkbook.CurrentPage());
        return ActiveWorkbook;
    }

    public FoPage2D EstablishCurrentPage<T>(string pagename, string color = "Ivory") where T : FoPage2D
    {
        return CurrentWorkbook().EstablishCurrentPage<T>(pagename, color);
    }

    public FoPage2D CurrentPage()
    {
        return CurrentWorkbook().CurrentPage();
    }

    public FoStage3D EstablishCurrentStage<T>(string pagename, string color = "Ivory") where T : FoStage3D
    {
        return CurrentWorkbook().EstablishCurrentStage<T>(pagename, color);
    }

    public FoStage3D CurrentStage()
    {
        return CurrentWorkbook().CurrentStage();
    }

    public virtual async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        AllWorkbooks()?.ForEach(async item =>
        {
            if (item.IsActive)
                await item.RenderWatermark(ctx, tick);
        });
        await Task.CompletedTask;
    }

    public virtual async Task DropFileCreateShape(IBrowserFile file, CanvasMouseArgs args)
    {
        await OnFileDrop.Invoke(file, args);
    }

    public async Task InitializedAsync(string defaultHubURI)
    {
        PubSub!.SubscribeTo<ViewStyle>(OnWorkspaceViewStyleChanged);
        // if (!Command.HasHub())
        // {
        //     EstablishDrawingSyncHub(defaultHubURI);
        // }

        await PubSub!.Publish<InputStyle>(InputStyle);
    }

    public void StartHub()
    {
        // Command.StartHub();
        var defaultHubURI = Command.GetServerUri()?.ToString() ?? "GetServerUri Error";
        var note = $"Starting SignalR Hub:{defaultHubURI}".WriteNote();
        Command.SendToast(ToastType.Info, note);
    }

    public void StopHub()
    {
        // Command.StopHub();
        var defaultHubURI = Command.GetServerUri()?.ToString() ?? "GetServerUri Error";
        var note = $"Starting SignalR Hub:{defaultHubURI}".WriteNote();
        Command.SendToast(ToastType.Info, note);
    }
    public void OnDispose()
    {
        // if (Command.HasHub())
        // {
        //     DisconnectDrawingSyncHub();
        // }
    }

    private void OnWorkspaceViewStyleChanged(ViewStyle e)
    {
        viewStyle = e;
    }

    public string GetUserID()
    {
        if (string.IsNullOrEmpty(UserID))
        {
            var data = new MockDataGenerator();
            UserID = data.GenerateName();
        }
        return UserID;
    }


    public string GetBaseUrl()
    {
        return CurrentUrl;
    }

    public string SetBaseUrl(string url)
    {
        CurrentUrl = url;
        $"CurrentUrl: {CurrentUrl}".WriteSuccess();
        return CurrentUrl;
    }

    public IDrawing GetDrawing()
    {
        return ActiveDrawing;
    }

    public IArena GetArena()
    {
        return ActiveArena;
    }

    public ISelectionService GetSelectionService()
    {
        return SelectionService;
    }

    public void ClearAllWorkbook()
    {
        GetSlot<FoWorkbook>()?.Clear();
        GetSlot<FoMenu2D>()?.Clear();
        GetSlot<FoMenu3D>()?.Clear();
        FoWorkspace.RefreshCommands = true;
        FoWorkspace.RefreshMenus = true;
        //"ClearAllWorkbook".WriteWarning();
    }

    public List<FoWorkbook> AddWorkbook(FoWorkbook book)
    {
        Add<FoWorkbook>(book);
        FoWorkspace.RefreshCommands = true;
        FoWorkspace.RefreshMenus = true;
        return Members<FoWorkbook>();
    }

    public virtual T EstablishWorkbook<T>(string key) where T : FoWorkbook
    {
        var found = AllWorkbooks().Where(item => item.GetType() == typeof(T)).FirstOrDefault() as T;
        if (found == null)
        {
            found = Activator.CreateInstance(typeof(T), this, Foundry) as T;
            found!.Key = key;
            found!.Name = key;
            AddWorkbook(found);
        }
        return found!;
    }

    public FoWorkbook? FindWorkbook(string name)
    {
        var found = AllWorkbooks().Where(item => item.Key.Matches(name)).FirstOrDefault();
        return found;
    }

    public List<FoWorkbook> AllWorkbooks()
    {
        return Members<FoWorkbook>();
    }





    public U EstablishMenu2D<U>(string name, bool clear) where U : FoMenu2D
    {
        RefreshMenus = true;
        var menu = Find<U>(name);
        if (menu == null)
        {
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public U EstablishMenu2D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoMenu2D
    {
        var menu = EstablishMenu2D<U>(name, clear);

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                menu.Add<T>(shape);
        }

        return menu;
    }

    public U EstablishMenu3D<U>(string name, bool clear) where U : FoMenu3D
    {
        var menu = Find<U>(name);
        if (menu == null)
        {
            RefreshMenus = true;
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public U EstablishMenu3D<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton3D where U : FoMenu3D
    {
        var menu = EstablishMenu3D<U>(name, clear);

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                menu.Add<T>(shape);
        }

        //menu.LayoutHorizontal();

        return menu;
    }

    public virtual void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        //$"ENTER FoWorkspace CreateMenus".WriteWarning();
        GetSlot<FoMenu2D>()?.Clear();
        GetSlot<FoMenu3D>()?.Clear();

        var OpenNew = async () =>
        {
            var target = nav!.ToAbsoluteUri("/");
            try
            {
                await js.InvokeAsync<object>("open", target); //, "_blank", "height=600,width=1200");
            }
            catch { }
        };

        //only the active one
        //AllWorkbooks().ForEach(item => item.CreateMenus(space, js, nav));
        
        ActiveWorkbook?.CreateMenus(space, js, nav);
        //GetDrawing()?.CreateMenus(space, js, nav);
        // GetArena()?.CreateMenus(space, js, nav);
    }
    public virtual Dictionary<string, Action> DefaultMenu()
    {
        return new Dictionary<string, Action>()
        {
            //{ "New Window", () => OpenNew()},
            { "View 2D", () => PubSub.Publish<ViewStyle>(ViewStyle.View2D)},
            { "View 3D", () => PubSub.Publish<ViewStyle>(ViewStyle.View3D)},
            { "Pan Zoom", () => GetDrawing()?.TogglePanZoomWindow()},
            { "Save Drawing", () => Command.Save()},
            { "Restore Drawing", () => Command.Restore()},
        };
    }


    public U EstablishCommand<U>(string name, bool clear) where U : FoCommand2D
    {
        var menu = Find<U>(name);
        if (menu == null)
        {
            RefreshCommands = true;
            menu = Activator.CreateInstance(typeof(U), name) as U;
            Add<U>(menu!);
        }
        if (clear)
            menu?.Clear();

        return menu!;
    }

    public U EstablishCommand<U, T>(string name, Dictionary<string, Action> actions, bool clear) where T : FoButton2D where U : FoCommand2D
    {
        var commandBar = EstablishCommand<U>(name, clear);

        foreach (KeyValuePair<string, Action> item in actions)
        {
            if (Activator.CreateInstance(typeof(T), item.Key, item.Value) is T shape)
                commandBar?.Add<T>(shape);
        }

        return commandBar!;
    }

    public virtual void CreateCommands(IWorkspace space, IJSRuntime JsRuntime, NavigationManager nav, string serverUrl)
    {
        GetSlot<FoCommand2D>()?.Clear();

        var OpenDTAR = async () =>
        {
            var target = nav!.ToAbsoluteUri(serverUrl);
            try
            {
                await JsRuntime!.InvokeAsync<object>("open", target);
            }
            catch { }
        };


        space.EstablishCommand<FoCommand2D, FoButton2D>("CMD", new Dictionary<string, Action>()
        {
            { "Ping", () => DoPing()},
            { "Clear", () => DoClear()},
            { "FileDrop", () => SetFileDropStyle()},
            { "Draw", () => SetDrawingStyle()},
            { "Save", () => DoSave()},
            // { "1:1", () => PanZoom.Reset()},
            // { "Zoom 2.0", () => PanZoom.SetZoom(2.0)},
            // { "Zoom 0.5", () => PanZoom.SetZoom(0.5)},
            // { "Down", () => ActiveDrawing?.MovePanBy(0,50)},
            // { "Right", () => ActiveDrawing?.MovePanBy(50,0)},
            // { "Up", () => ActiveDrawing?.MovePanBy(0,-50)},
            // { "Left", () => ActiveDrawing?.MovePanBy(-50,0)},
            { "Window", () => ActiveDrawing?.TogglePanZoomWindow()},
            { "Hit", () => ActiveDrawing?.ToggleHitTestDisplay()},
        }, true);

        ActiveWorkbook?.CreateCommands(space, JsRuntime, nav, serverUrl);

        FoWorkspace.RefreshCommands = true;
    }

    private async void DoSave()
    {
        var text = "Hello, Saved world!";
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);

        await LocalFileSave("HelloWorld.txt", bytes);
    }

    public void DoPing()
    {
        Command.SendToast(ToastType.Info, "Ping");
    }

    public void DoClear()
    {
        GetDrawing()?.ClearAll();
    }

    public ViewStyle GetViewStyle()
    {
        return viewStyle;
    }
    public void SetViewStyle(ViewStyle view)
    {
        viewStyle = view;
    }
    public bool IsViewStyle2D()
    {
        return GetViewStyle() == ViewStyle.View2D;
    }
    public bool IsViewStyle3D()
    {
        return GetViewStyle() == ViewStyle.View3D;
    }

    // public HubConnection EstablishDrawingSyncHub(string defaultHubURI)
    // {
    //     if (Command.HasHub())
    //         return Command.GetSignalRHub()!;

    //     //var secureHub = defaultHubURI.Replace("http://", "https://");
    //     var secureHubURI = new Uri(defaultHubURI);


    //     var hub = new HubConnectionBuilder()
    //         .WithUrl(secureHubURI)
    //         .Build();

    //     Command.SetSignalRHub(hub, secureHubURI, GetUserID(), Toast);
    //     SetSignalRHub(hub, GetUserID());

    //     //Toast?.Success($"HubConnection {secureHubURI} ");

    //     return hub;
    // }

    // public void DisconnectDrawingSyncHub()
    // {
    //     if (Command.HasHub())
    //         Command.StopHub();

    //     // Command.SetSignalRHub(hub, GetUserID(), Toast);
    //     // SetSignalRHub(hub, GetUserID());

    // }

    // public bool SetSignalRHub(HubConnection hub, string panid)
    // {
    //     ActiveWorkbook.SetSignalRHub(hub, panid);

    //     hub.Closed += async (error) =>
    //    {
    //        var rand = new Random();
    //        await Task.Delay(rand.Next(0, 5) * 1000);
    //        await hub.StartAsync();
    //    };

    //     hub.Reconnecting += async (error) =>
    //     {
    //         var rand = new Random();
    //         await Task.Delay(rand.Next(0, 5) * 1000);
    //     };

    //     hub.Reconnected += async (error) =>
    //     {
    //         var rand = new Random();
    //         await Task.Delay(rand.Next(0, 5) * 1000);
    //     };
    //     return true;
    // }

    public List<IFoMenu> CollectMenus(List<IFoMenu> list)
    {
        GetMembers<FoMenu2D>()?.ForEach(item => list.Add(item));
        GetMembers<FoMenu3D>()?.ForEach(item => list.Add(item));
        ActiveWorkbook?.CollectMenus(list);
        return list;
    }
    public List<IFoCommand> CollectCommands(List<IFoCommand> list)
    {
        GetMembers<FoCommand2D>()?.ForEach(item => list.Add(item));
        ActiveWorkbook?.CollectCommands(list);
        return list;
    }

    public ComponentBus GetPubSub()
    {
        return PubSub;
    }

    public virtual async Task RefreshRender(int tick)
    {
        AllWorkbooks().ForEach(async wb => await wb.RefreshRender(tick));
        await Task.CompletedTask;
    }


}
