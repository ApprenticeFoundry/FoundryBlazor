using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Extensions;

namespace FoundryBlazor.Persistence;

public class VersionPersistence // : FoBase
{
    public string? Name { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string? Revision { get; set; }
    public string? Change { get; set; }
    public string? Status { get; set; } // PUBLISHED, FOR_REVIEW, IN_DRAFT
    public string? Author { get; set; }
    public string? ApprovedBy { get; set; }
    public string Filename { get; set; }
    public string? Path { get; set; }
    public string? Url { get; set; }

    public string? DownloadUrl { get; set; }
    public string? Description { get; set; }


    public string? LastVersion { get; set; }
    public string? FutureVersion { get; set; }
    public string? TimeStamp { get; set; }

    public VersionPersistence()
    {
        Version = "";
        Filename = "";
        Title = "";
    }

    public VersionPersistence ShallowCopy()
    {
        var result = (VersionPersistence)this.MemberwiseClone();
        return result;
    }

    public VersionPersistence GenerateNewVersion()
    {
        var version = IncrementVersion(Version);

        var result = ShallowCopy();
        result.Version = version;
        result.LastVersion = Filename;

        result.Filename = ComputeNewFilename(Filename ?? "", version);
        FutureVersion = result.Filename;
        return result;
    }

    private static string ComputeNewFilename(string oldFilename, string version)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(oldFilename);
        var list = name.Split('_');
        list[^1] = version;
        var result = $"{string.Join('_', list)}.json";
        return result;
    }
    public static string IncrementVersion(string ver)
    {
        string answer = "0001";
        if (int.TryParse(ver, out int result))
        {
            result++;
            answer = result.ToString().PadLeft(4, '0');
        }
        return answer;
    }

    public static string ComputeSubfolder(string name)
    {
        var subfolder = name.RemoveExtension();
        subfolder = subfolder.RemoveVersion();
        return subfolder;
    }

    public static string CurrentTimeStamp()
    {
        var stamp = DateTime.UtcNow.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        return stamp;
    }

    public static string CleanToFilename(string source)
    {
        if ( string.IsNullOrEmpty(source) ) return "";  
        var filename = source.Trim();

        filename = filename.Replace('/', '-');
        filename = filename.Replace('"', '-');
        filename = filename.Replace(' ', '-');
        //filename = filename.Trim(Path.GetInvalidFileNameChars());
        //filename = filename.Trim(Path.GetInvalidPathChars());
        return filename;
    }

    public static VersionPersistence Generate(string previous, string name, string title, string author)
    {
        var version = IncrementVersion(!string.IsNullOrEmpty(previous) ? previous : "0000");
        var filename = CleanToFilename(name);
        var info = new VersionPersistence
        {
            Title = title,
            Name = name,
            Author = author,
            Version = version,
            Status = "DEVELOP",
            Filename = $"{filename}_{version}.json",
            TimeStamp = CurrentTimeStamp()
        };
        return info;
    }

    public static List<VersionPersistence> FilterByLatestVersion(List<VersionPersistence> source)
    {
        var dict = source
            .OrderByDescending(obj => obj.Filename!)
            .GroupBy(obj => obj.Name ?? obj.Title!)
            .ToDictionary(g => g.Key, g => g.ToList());

        var list = new List<VersionPersistence>();
        foreach (KeyValuePair<string, List<VersionPersistence>> entry in dict)
        {
            list.Add(entry.Value.First());
        }
        return list;
    }

}

