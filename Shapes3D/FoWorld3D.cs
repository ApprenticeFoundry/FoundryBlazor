// this is a tool to load/unload knowledge modules that define projects
using System.Collections.Generic;
using System.Linq;
using FoundryBlazor.Extensions;



namespace FoundryBlazor.Shape;


public class FoWorld3D : FoGlyph3D
{


    public FoWorld3D() : base()
    {
        GetSlot<FoGroup3D>();
        GetSlot<FoShape3D>();
        GetSlot<FoText3D>();
        GetSlot<FoDatum3D>();
        GetSlot<FoRelationship3D>();
    }

    public List<FoGroup3D>? Platforms()
    {
        return GetMembers<FoGroup3D>();
    }
    public List<FoDatum3D>? Datums()
    {
        return GetMembers<FoDatum3D>();
    }

    public List<FoShape3D>? Bodies()
    {
        return GetMembers<FoShape3D>();
    }

    public List<FoText3D>? Labels()
    {
        return GetMembers<FoText3D>();
    }
    public List<FoRelationship3D>? Relationships()
    {
        return GetMembers<FoRelationship3D>();
    }

    // public T Find<T>(string name) where T : FoGlyph3D
    // {
    // 	if (typeof(T).Name.Matches(nameof(FoShape3D)))
    // 		return bodies?.FirstOrDefault(item => item.name.Matches(name)) as T;

    // 	if (typeof(T).Name.Matches(nameof(FoText3D)))
    // 		return labels?.FirstOrDefault(item => item.name.Matches(name)) as T;

    // 	if (typeof(T).Name.Matches(nameof(FoGroup3D)))
    // 		return platforms?.FirstOrDefault(item => item.name.Matches(name)) as T;

    // 	return null;
    // }

    // public T FindReferenceDesignation<T>(string name) where T : FoGlyph3D
    // {
    // 	if (typeof(T).Name.Matches(nameof(FoShape3D)))
    // 		return bodies?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

    // 	if (typeof(T).Name.Matches(nameof(FoText3D)))
    // 		return labels?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

    // 	if (typeof(T).Name.Matches(nameof(FoGroup3D)))
    // 		return platforms?.FirstOrDefault(item => item.referenceDesignation.Matches(name)) as T;

    // 	return null;
    // }

    public FoWorld3D FlushPlatforms()
    {
        Platforms()?.ForEach(platform => platform.Flush());
        return this;
    }

    public List<FoGroup3D>? FillPlatforms()
    {
        Platforms()?.ForEach(platform =>
        {
            platform.Flush();

            //TODO: Why are TextLabels being added to Bodies?  If we can prevent that then we don't need to check obj.Type != null
            Bodies()?.Where(obj => obj.IsSamePlatform(platform) && obj.Type != null)
                    .Select(obj => platform.Add<FoShape3D>(obj)).ToList();

            Labels()?.Where(obj => obj.IsSamePlatform(platform))
                    .Select(obj => platform.Add<FoText3D>(obj)).ToList();

            Datums()?.Where(obj => obj.IsSamePlatform(platform))
                    .Select(obj => platform.Add<FoDatum3D>(obj)).ToList();
        });
        return Platforms();
    }

    //public FoWorld3D FillWorldFromPlatform(FoGroup3D platform)
    //{
    //    platforms.Add(platform);
    //    bodies.AddRange(platform.bodies);
    //    labels.AddRange(platform.labels);
    //    relationships.AddRange(platform.relationships);
    //    return RemoveDuplicates();
    //}

    //public FoWorld3D FillWorldFromWorld(FoWorld3D world)
    //{
    //    platforms.AddRange(world.platforms);
    //    bodies.AddRange(world.bodies);
    //    labels.AddRange(world.labels);
    //    relationships.AddRange(world.relationships);
    //    return RemoveDuplicates();
    //}

    public FoWorld3D RemoveDuplicates()
    {
        //platforms = platforms.DistinctBy(i => i.uniqueGuid).ToList();
        //bodies = bodies.DistinctBy(i => i.uniqueGuid).ToList();
        //labels = labels.DistinctBy(i => i.uniqueGuid).ToList();
        //relationships = relationships.DistinctBy(i => i.uniqueGuid).ToList();

        var platforms = Platforms()?.GroupBy(i => i.UniqueGuid).Select(g => g.First()).ToList();
        if (platforms != null)
            GetSlot<FoGroup3D>()?.Flush().AddRange(platforms);

        var bodies = Bodies()?.GroupBy(i => i.UniqueGuid).Select(g => g.First()).ToList();
        if (bodies != null)
            GetSlot<FoShape3D>()?.Flush().AddRange(bodies);

        var labels = Labels()?.GroupBy(i => i.UniqueGuid).Select(g => g.First()).ToList();
        if (labels != null)
            GetSlot<FoText3D>()?.Flush().AddRange(labels);

        var datums = Datums()?.GroupBy(i => i.UniqueGuid).Select(g => g.First()).ToList();
        if (datums != null)
            GetSlot<FoDatum3D>()?.Flush().AddRange(datums);

        var relationships = Relationships()?.GroupBy(i => i.UniqueGuid).Select(g => g.First()).ToList();
        if (relationships != null)
            GetSlot<FoRelationship3D>()?.Flush().AddRange(relationships);

        return this;
    }
}
