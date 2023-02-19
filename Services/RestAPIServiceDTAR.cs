using System.Net.Http.Headers;
using System.Text;
using FoundryBlazor.Extensions;
using IoBTMessage.Models;
using IoBTModules.Models;
using Microsoft.AspNetCore.Components.Forms;
namespace FoundryBlazor.Services;

public interface IRestAPIServiceDTAR
{
    Task<List<string>> GetVersion();

    Task<List<DT_World3D>> GetWorlds();
    Task<List<DT_MILDocument>> GetRootDocument();
    Task<List<DT_ProcessPlan>> GetAllProcessPlans();
    Task<DT_ProcessPlan?> GetProcessPlan(string guid);
    Task<DT_ComponentReference> GetComponentForPlan(string guid);
    Task<List<VersionInfo>> ModuleWorkbook(string guid);
    Task<List<UserMode>> SetApplicationMode(UserMode mode);
    Task<List<AppStoreDefinition>> GetWorkbooks();
    Task<List<DT_AssetFile>> GetAssetFiles();
    Task<List<AppStoreDefinition>> AppStoreRunOffline(string guid, bool toggle);
    Task<string> HardReset();
    Task<string> GetElasticSearchURI();
    void SetServerUrl(string url);
    string GetServerUrl();
    string GetAPIPath();
    Task<string> GetAPIVersion();
    Task<List<UDTO_File>> UploadFile(IBrowserFile file);

    Task<List<DT_World3D>> WorldAddOrUpdate(DT_World3D world);
}

public class RestAPIServiceDTAR : IRestAPIServiceDTAR
{
    private string ServerURL { get; set; } = "https://rondtar.azurewebsites.net/";  //"https://rondtar.azurewebsites.net/"; "https://localhost:5001";  // 
    public string APIPath { get; set; }
    private string? note { get; set; }
    private List<string>? ServerInfo { get; set; }
    public RestAPIServiceDTAR()
    {
        APIPath = $"{ServerURL}/api";
    }

    public void SetServerUrl(string url)
    {
        ServerURL = url;
        APIPath = $"{ServerURL}/api";
    }

    private async Task<List<string>> EstablishServerInfo()
    {
        ServerInfo ??= await GetVersion();
        return ServerInfo;
    }

    public string GetAPIPath()
    {
        return APIPath;
    }

    public string GetServerUrl()
    {
        return ServerURL;
    }

    public async Task<string> GetAPIVersion()
    {
        await EstablishServerInfo();
        if (ServerInfo!.Count > 0) return ServerInfo!.ElementAt(0);
        else return "NO API VERSION AVAILABLE";
    }

    public async Task<List<string>> GetVersion()
    {
        try
        {
            var path = new Uri($"{APIPath}/AssetFile/Version");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var wrapper = StorageHelpers.HydrateWrapper<string>(text, true);
            ServerInfo = wrapper.payload.ToList<string>();
            return ServerInfo;
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<string> GetElasticSearchURI()
    {
        try
        {
            var path = new Uri($"{APIPath}/ElasticSearch/Uri");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var wrapper = StorageHelpers.HydrateWrapper<string>(text, true);

            var defaultURI = "NO ELASTIC URI AVAILABLE";
            if (wrapper != null) return wrapper.payload.FirstOrDefault<string>() ?? defaultURI;
            else return defaultURI;
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<string> HardReset()
    {
        try
        {
            var path = new Uri($"{APIPath}/AppStore/HardReset");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            return text;
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<DT_MILDocument>> GetRootDocument()
    {
        try
        {
            var path = new Uri($"{APIPath}/Document/RootDocument");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_MILDocument>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<DT_World3D>> GetWorlds()
    {
        try
        {
            var path = new Uri($"{APIPath}/Platform/AllWorlds");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_World3D>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<DT_ProcessPlan>> GetAllProcessPlans()
    {
        try
        {
            var path = new Uri($"{APIPath}/ProcessPlan/AllPlans");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_ProcessPlan>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<DT_ProcessPlan?> GetProcessPlan(string guid)
    {
        try
        {
            var path = new Uri($"{APIPath}/ProcessPlan/ProcessPlan/{guid}");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_ProcessPlan>(text, true);

            if (result!.hasError)
            {
                return null;
            }
            else
            {
                var coll = result!.payload;
                return coll.First();
            }
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<DT_ComponentReference> GetComponentForPlan(string guid)
    {
        try
        {
            var path = new Uri($"{APIPath}/ProcessPlan/ComponentForPlan/{guid}");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_ComponentReference>(text, true);
            var coll = result!.payload;
            return coll.First();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<VersionInfo>> ModuleWorkbook(string guid)
    {
        try
        {
            var path = new Uri($"{APIPath}/Appstore/Modules/{guid}");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<VersionInfo>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<AppStoreDefinition>> AppStoreRunOffline(string guid, bool toggle)
    {
        try
        {
            var toggleStr = toggle.ToString().ToLower();
            var path = new Uri($"{APIPath}/AppStore/RunOffline/{guid}/{toggleStr}");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<AppStoreDefinition>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }


    public async Task<List<UserMode>> SetApplicationMode(UserMode mode)
    {
        try
        {
            var path = new Uri($"{APIPath}/Appstore/SetMode");
            var client = new HttpClient();

            var s = StorageHelpers.Dehydrate<UserMode>(mode, true);
            var c = new StringContent(s, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(path, c);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<UserMode>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<AppStoreDefinition>> GetWorkbooks()
    {
        try
        {
            var path = new Uri($"{APIPath}/AppStore/Applications");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<AppStoreDefinition>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<DT_AssetFile>> GetAssetFiles()
    {
        try
        {
            var path = new Uri($"{APIPath}/AssetFile/AssetFiles");
            var client = new HttpClient();

            var response = await client.GetAsync(path);
            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_AssetFile>(text, true);
            var coll = result!.payload;
            return coll.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<UDTO_File>> UploadFile(IBrowserFile file)
    {
        try
        {
            var path = new Uri($"{APIPath}/Manifest/UploadFile");
            var client = new HttpClient();

            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            if (!string.IsNullOrEmpty( file.ContentType))
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            content.Add(fileContent, "file", file.Name);

            var response = await client.PostAsync(path, content);
            var text = await response.Content.ReadAsStringAsync();
            var success = StorageHelpers.HydrateWrapper<UDTO_File>(text, true);
            
            var result = success.payload.ToList();
            return result;
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

    public async Task<List<DT_World3D>> WorldAddOrUpdate(DT_World3D world)
    {
        try
        {
            var path = new Uri($"{APIPath}/WorldEditor/AddOrUpdate");
            var client = new HttpClient();

            string json = StorageHelpers.Dehydrate<DT_World3D>(world, true);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(path, httpContent);

            var text = await response.Content.ReadAsStringAsync();
            var result = StorageHelpers.HydrateWrapper<DT_World3D>(text, true);
            return result.payload.ToList();
        }
        catch (Exception ex)
        {
            this.note = ex.Message;
            this.note.WriteLine(ConsoleColor.Yellow);
            throw;
        }
    }

}