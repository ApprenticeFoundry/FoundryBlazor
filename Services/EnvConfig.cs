//Source: https://dusted.codes/dotenv-in-dotnet

using FoundryRulesAndUnits.Extensions;
using System.Reflection;

namespace FoundryBlazor;

public interface IEnvConfig
{
	string SERVER_URL();
	void SET_SERVER_URL(Uri uri);

	public List<string> EstablishAllFolders();
	void EstablishDirectory(string folder);
	bool FileExist(string filePath);
	bool PathExist(string filePath);


	string RootStorageFolder();

	string CasheFolder();

	string TempFolder();
	string LoggingFolder();
}

public class EnvConfig : IEnvConfig
{
	private readonly string Storage = "storage";


	public string LOCAL_SERVER_URL { get; set; } = "";

	public string DEPLOYMENT { get; set; } = "";

	public EnvConfig(string filePath)
	{
		this.SetDefaultValues(filePath);
	}




	public void SET_SERVER_URL(Uri uri)
	{
		var url = uri.AbsoluteUri.ToString();
		if ("CLOUD".Equals(DEPLOYMENT))
		{
			url = url.Replace("http:", "https:");
		}
		LOCAL_SERVER_URL = url;
	}

	public string SERVER_URL()
	{
		return $"{LOCAL_SERVER_URL}";
	}



	public string DocumentCasheDownloadEndpoint(string filename)
	{
		if (filename.StartsWith("http")) return filename;

		var local = SERVER_URL();
		var folder = CasheFolder();
		return $"{local}{folder}/{filename}";
	}



	private static void Load(string filePath)
	{
		if (!File.Exists(filePath))
			return;

		foreach (var line in File.ReadAllLines(filePath))
		{
			var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 2)
				continue;

			Environment.SetEnvironmentVariable(parts[0], parts[1]);
		}
	}

	public void Extract(string Name)
	{
		var obj = Environment.GetEnvironmentVariable(Name);
		if (obj != null)
		{
			var prop = GetType().GetProperty(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
			if (prop != null)
			{
				if (prop.PropertyType == typeof(int))
				{
					var sec = int.Parse(obj.ToString());
					prop.SetValue(this, sec);
				}
				else
				{
					prop.SetValue(this, obj);
				}
			}
		}
	}

	public void SetDefaultValues(string filePath)
	{
		EnvConfig.Load(filePath);

		Extract("LOCAL_SERVER_URL");
		Extract("DEPLOYMENT");
		Extract("KEEP_ALIVE_SECONDS");
	}




	public string RootStorageFolder()
	{
		return Storage;
	}

	public string LoggingFolder()
	{
		return $"{RootStorageFolder()}/Logging";
	}


    // public void RefreshStaticFiles()
    // {

    //     var source = RootKnowledgeCenterAssets();
    //     var target = CasheFolder();
		             
    //     $"RefreshStaticFiles: source {source}".WriteNote();
    //     $"RefreshStaticFiles: cashe {target}".WriteNote();

    //     // CopyDirectory(source, target);

    //     foreach (var subdirectory in Directory.GetDirectories(source))
    //     {
    //         CopyFiles(subdirectory, target);     
    //     } 
    // }

	public string CasheFolder()
	{
		return $"{RootStorageFolder()}/StaticFiles";
	}



	public string TempFolder()
	{
		return $"{RootStorageFolder()}/TempData";
	}

	public void EstablishDirectory(string folder)
	{
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
	}

	public bool FileExist(string filePath)
	{
		return File.Exists(filePath);
	}

	public bool PathExist(string filePath)
	{
		return Directory.Exists(filePath);
	}

	public List<string> AllFolders()
	{
		return new List<string>() {
			CasheFolder(),
		};
	}

	public List<string> EstablishAllFolders()
	{
		EstablishDirectory(TempFolder());

		var all = AllFolders();
		all.ForEach(folder =>
		{
			EstablishDirectory(folder);
		});
		return all;
	}
}


