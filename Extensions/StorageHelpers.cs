using System.Reflection;
using System.Text;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;



namespace FoundryBlazor.Extensions;

public static class StorageHelpers
{
    private static readonly Dictionary<string, Type> typeLookup = new();
    private static FileExtensionContentTypeProvider? _provider;
    public static FileExtensionContentTypeProvider MIMETypeProvider()
    {
        if (_provider == null)
        {
            StorageHelpers._provider = new FileExtensionContentTypeProvider();
            _provider.Mappings[".babylon"] = "application/babylon";
            _provider.Mappings[".pdf"] = "application/pdf";
            _provider.Mappings[".obj"] = "model/obj";
            _provider.Mappings[".mtl"] = "model/mtl";
            _provider.Mappings[".fbx"] = "model/fbx";
            _provider.Mappings[".gltf"] = "model/gltf";
            _provider.Mappings[".glb"] = "model/glb";
            _provider.Mappings[".txt"] = "text/plain";
            _provider.Mappings[".mp4"] = "video/mp4";
            _provider.Mappings[".mov"] = "video/mov";
            _provider.Mappings[".mp3"] = "video/mp3";
            _provider.Mappings[".pdf"] = "application/pdf";
            _provider.Mappings[".doc"] = "application/vnd.ms-word";
            _provider.Mappings[".docx"] = "application/vnd.ms-word";
            _provider.Mappings[".xls"] = "application/vnd.ms-excel";
            _provider.Mappings[".xlsx"] = "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet";
            _provider.Mappings[".png"] = "image/png";
            _provider.Mappings[".jpg"] = "image/jpeg";
            _provider.Mappings[".jpeg"] = "image/jpeg";
            _provider.Mappings[".gif"] = "image/gif";
            _provider.Mappings[".csv"] = "text/csv";
        }

        return _provider;
    }

    public static string GetMIMEType(this string fileName)
    {
        var provider = StorageHelpers.MIMETypeProvider();
        if (!provider.TryGetContentType(fileName, out string contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    public static async Task<string> ReadAsStringAsync(this IFormFile file)
    {
        var result = new StringBuilder();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
                result.AppendLine(await reader.ReadLineAsync());
        }
        return result.ToString();
    }




    public static void RegisterLookupType<T>() where T: class
    {
        var type = typeof(T);
        var name = type.Name;
        if ( typeLookup.ContainsKey(name) == false ) {
            typeLookup.Add(name, type);
        }
    }

    public static Type? LookupType(string payloadType, Assembly? assembly=null)
    {
        
        if ( typeLookup.TryGetValue(payloadType, out Type? type) == false ) {
            var source = assembly ?? typeof(StorageHelpers).Assembly;
            type = source.DefinedTypes.FirstOrDefault(item => item.Name == payloadType);
            if ( type != null)
                typeLookup.Add(payloadType, type);
        }
        return type;
    }

  

    public static Stream GenerateStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

	public static void EstablishDirectory(string folder)
	{
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
	}
    public static bool PathExist(string filePath)
	{
		return Directory.Exists(filePath);
	}

	public static bool FileExist(string filePath)
	{
		return File.Exists(filePath);
	}

    public static string LocalPath(string directory, string filename)
    {
        string filePath = Path.Combine(directory, filename);
        return filePath;
    }

    public static string FullPath(string directory, string filename)
    {
        string path = Directory.GetCurrentDirectory();
        string filePath = Path.Combine(path, directory, filename);
   
        return filePath;
    }

    public static string WriteData(string folder, string filename, string data)
    {
        try
        {
            $"WriteData local {folder.ToUpper()}: {filename}".WriteTrace();
            EstablishDirectory(folder);
            string filePath = FullPath(folder, filename);
            File.WriteAllText(filePath, data);
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error WriteData {filename}| {ex.Message}");
            throw;
        }
    }
    public static string ReadData(string folder, string filename)
    {
        try
        {
            $"ReadData local {folder.ToUpper()}: {filename}".WriteTrace();
            EstablishDirectory(folder);
            string filePath = FullPath(folder, filename);
            string data = File.ReadAllText(filePath);
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ReadData {filename}| {ex.Message}");
            throw;
        }
    }

    public static List<T> ReadListFromFile<T>(string filename, string directory) where T : class
    {
        try
        {
            string filePath = FullPath(directory, filename);

            string text = File.ReadAllText(filePath);
            var result = CodingExtensions.HydrateList<T>(text, true);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ReadListFromFile {filename}| {ex.Message}");
            throw;
        }
    }

    public static List<T> WriteListToFile<T>(List<T> data, string filename, string directory) where T : class
    {
        try
        {
            string filePath = FullPath(directory, filename);

            var result = CodingExtensions.DehydrateList<T>(data, true);
            File.WriteAllText(filePath, result);

            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error WriteListToFile | {ex.Message}");
            throw;
        }
    }

    public static T ReadObjectFromFile<T>(string filename, string directory) where T : class
    {
        try
        {
            string filePath = FullPath(directory, filename);

            string text = File.ReadAllText(filePath);
            var result = CodingExtensions.Hydrate<T>(text, true);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ReadObjectFromFile | {ex.Message}");
            throw;
        }
    }

    public static void WriteSetting<T>(T value) where T : class
    {
        var key = typeof(T).Name;
        var filename = $"{key}.json";

        try
        {
            string filePath = FullPath("config", filename);

            var result = CodingExtensions.Dehydrate<T>(value, false);
            File.WriteAllText(filePath, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error WriteSetting | {ex.Message}");
            throw;
        }
    }
}
