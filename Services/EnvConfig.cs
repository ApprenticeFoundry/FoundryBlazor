//Source: https://dusted.codes/dotenv-in-dotnet

using System.Reflection;

namespace FoundryBlazor;

public interface IEnvConfig
{
    string IOBT_TVA_URL();
    string IOBT_TVAHUB_URL();
	string MQTT_URL();
	string IoTHub_Connection();
	string KAFKA_URL();
	string BLOB_URL();
	string SERVER_URL();
	string ES_URL();
	void SET_SERVER_URL(Uri uri);

	UserData Author();
	UserData SetUserData(UserData author);


	public List<string> EstablishAllFolders();
	void EstablishDirectory(string folder);
	bool FileExist(string filePath);
	bool PathExist(string filePath);

	string ModuleDownloadEndpoint(string filename);
	string DocumentCasheDownloadEndpoint(string filename);

	string BlobDocumentContainer();
	string BlobAppStoreContainer();
	string SetBlobAppStoreContainer(string root);

	string RootStorageFolder();
	string ModuleFolder();
	string WorkbookFolder();
	string AppStoreFolder();
	string CasheFolder();
	string VersionQueueFolder();
	string ComponentFolder();
	string TempFolder();
	string LoggingFolder();
}

public class EnvConfig : IEnvConfig
{
	private readonly string Storage = "storage";
	private UserData? UserData { get; set; }
	public string BLOB_ROOT_CONTAINER { get; set; } = "dtar-appstore";
	public string LOCAL_SERVER_URL { get; set; } = "";
	public string MQTT_BROKER_URL { get; set; } = "";
	public string KAFKA_BROKER_URL { get; set; } = "";
	public string IoTHUB_BROKER_URL { get; set; } = "";
	public string IOBT_BASE_URL { get; set; } = "";
	public string ELASTIC_SEARCH_URL { get; set; } = "";
	public string BLOB_STORAGE_URL { get; set; } = "";
	public string DEPLOYMENT { get; set; } = "";

	public EnvConfig(string filePath)
	{
		this.SetDefaultValues(filePath);
	}

	public string MQTT_URL()
	{
		return $"{MQTT_BROKER_URL}";
	}

    public string IOBT_TVA_URL()
    {
        return $"{IOBT_BASE_URL}";
    }
    public string IOBT_TVAHUB_URL()
    {
        return $"{IOBT_BASE_URL}/ClientHub";
    }
	public string ES_URL()
	{
		return $"{ELASTIC_SEARCH_URL}";
	}

	public string IoTHub_Connection()
	{
		// return $"{IOTHUB_BROKER_URL}";
		//return "HostName=fsrib-tva.azure-devices.net;DeviceId=tva-1;SharedAccessKey=KOBsunpKq9fjTLBQ5sB7ud/5aiAHAcpSqswtvkFbUTA=";
		var xx = "HostName=iothub-fsrib-dev-1.azure-devices.net;DeviceId=IoBTSquire1;SharedAccessKey=nytN3hjr0+DOBa8H7KGsksp4RYgJyneJ1LCKJhx41ug=";

		return xx;
	}
	public string KAFKA_URL()
	{
		return $"{KAFKA_BROKER_URL}";
	}

	public string BLOB_URL()
	{
		return $"{BLOB_STORAGE_URL}/{BlobDocumentContainer()}";
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

	public string ModuleDownloadEndpoint(string filename)
	{
		var local = SERVER_URL();
		return $"{local}api/Module/Download/{filename}";
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

		Extract("IOBT_BASE_URL");
		Extract("ELASTIC_SEARCH_URL");
		Extract("BLOB_STORAGE_URL");
		Extract("LOCAL_SERVER_URL");
		Extract("MQTT_BROKER_URL");
		Extract("IOTHUB_BROKER_URL");
		Extract("KAFKA_BROKER_URL");
		Extract("DEPLOYMENT");
		Extract("KEEP_ALIVE_SECONDS");
	}

	public UserData SetUserData(UserData author)
	{
		UserData = author;
		return Author();
	}
	public UserData Author()
	{
		if (UserData == null)
		{
			UserData = new UserData()
			{
				Email = "IOBT@SAIC.COM",
				Username = "IOBT_TEAM"
			};
		}
		return UserData;
	}

	public string BlobDocumentContainer()
	{
		return "dtar-container";
	}
	public string SetBlobAppStoreContainer(string root)
	{
		this.BLOB_ROOT_CONTAINER = root;
		return BlobAppStoreContainer();
	}
	public string BlobAppStoreContainer()
	{
		return BLOB_ROOT_CONTAINER;
	}

	public string RootStorageFolder()
	{
		return Storage;
	}

	public string LoggingFolder()
	{
		return $"{RootStorageFolder()}/Logging";
	}
	public string ModuleFolder()
	{
		return $"{RootStorageFolder()}/Modules";
	}
	public string ComponentFolder()
	{
		return $"{RootStorageFolder()}/Components";
	}


	public string AppStoreFolder()
	{
		return $"{RootStorageFolder()}/AppStore";
	}


	public string CasheFolder()
	{
		return $"{RootStorageFolder()}/StaticFiles";
	}

	public string VersionQueueFolder()
	{
		return $"{RootStorageFolder()}/StaticVersionQueue";
	}

	public string WorkbookFolder()
	{
		return $"{RootStorageFolder()}/Workbooks";
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
			AppStoreFolder(),
			CasheFolder(),
			WorkbookFolder(),
			ModuleFolder(),
			ComponentFolder(),
			VersionQueueFolder()
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


