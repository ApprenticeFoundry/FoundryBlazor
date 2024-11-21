using BlazorThreeJS.Core;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Viewers;
using FoundryRulesAndUnits.Models;

namespace FoundryBlazor.Shape;

public class FoGroup3D : FoGlyph3D, IShape3D
{



    public Vector3? Offset { get; set; }


    public FoGroup3D() : base()
    {
        GetSlot<FoShape3D>();
        GetSlot<FoText3D>();
        GetSlot<FoDatum3D>();
        GetSlot<FoRelationship3D>();
        GetSlot<FoGroup3D>();
    }

    public FoGroup3D(string name) : base(name)
    {
        GetSlot<FoShape3D>();
        GetSlot<FoText3D>();
        GetSlot<FoDatum3D>();
        GetSlot<FoRelationship3D>();
        GetSlot<FoGroup3D>();
    }

    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        AddFolderIfNotEmpty<FoGroup3D>(list);
        AddFolderIfNotEmpty<FoShape3D>(list);
        AddFolderIfNotEmpty<FoText3D>(list);
        AddFolderIfNotEmpty<FoDatum3D>(list);

        return list;
    }

    public T AddShape<T>(T value) where T : FoGlyph3D
    {
        var collection = Slot<T>();
        if (string.IsNullOrEmpty(value.Key))
            value.Key = collection.NextItemName();


        collection.AddObject(value.Key, value);
        return value;
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


    public FoGroup3D EstablishBox(string name, double width = 1.0, double height = 1.0, double depth = 1.0)
    {
        this.Key = name;
        BoundingBox = new Vector3(width, height, depth);
        Position = new Vector3();
        Offset = new Vector3();
        return this;
    }






    public FoGroup3D Flush()
    {
        GetSlot<FoGroup3D>()?.Flush();
        GetSlot<FoShape3D>()?.Flush();
        GetSlot<FoText3D>()?.Flush();
        GetSlot<FoDatum3D>()?.Flush();
        GetSlot<FoRelationship3D>()?.Flush();
        return this;
    }



    public U? RelateMembers<U>(FoGlyph3D source, string name, FoGlyph3D target) where U : FoRelationship3D
    {
        var tag = $"{source.GlyphId}:{name}";
        var relationship = Find<U>(tag);
        if (relationship == null)
        {
            relationship = FindOrCreate<U>(tag, true);
            relationship?.Build(source.GlyphId, name, target.GlyphId);
        }
        else
        {
            relationship.Relate(target.GlyphId);
        }

        return relationship;
    }

    public U? UnrelateMembers<U>(FoGlyph3D source, string name, FoGlyph3D target) where U : FoRelationship3D
    {
        var tag = $"{source.GlyphId}:{name}";
        var relationship = Find<U>(tag);
        relationship?.Unrelate(target.GlyphId);

        return relationship;
    }




    private T CreateItem<T>(string name) where T : FoGlyph3D
    {
        var found = (Activator.CreateInstance(typeof(T), name) as T)!;
        //found.GlyphId = Guid.NewGuid().ToString();
        return found;
    }



    public T CreateUsing<T>(string name, string guid = "") where T : FoGlyph3D
    {
        var found = FindOrCreate<T>(name, true);
        if (!string.IsNullOrEmpty(guid))
            found!.GlyphId = guid;

        return found!;
    }

    public T? FindOrCreate<T>(string name, bool create = false) where T : FoGlyph3D
    {
        var found = Find<T>(name);
        if (found == null && create)
        {
            found = CreateItem<T>(name);
            Slot<T>().Add(found);
        }
        return found;
    }


    public override async Task<bool> PreRender(FoArena3D arena, bool deep = true)
    {
        var tasks = new List<Task>();

        tasks.AddRange(Members<FoShape3D>().Select(shape => shape.PreRender(arena, deep)));
        tasks.AddRange(Members<FoText3D>().Select(shape => shape.PreRender(arena, deep)));
        tasks.AddRange(Members<FoGroup3D>().Select(shape => shape.PreRender(arena, deep)));
        tasks.AddRange(Members<FoDatum3D>().Select(shape => shape.PreRender(arena, deep)));


        await Task.WhenAll(tasks);
        return true;
    }

    public override bool Render(Scene scene, int tick, double fps, bool deep = true)
    {
        Members<FoShape3D>().ForEach(shape => shape.Render(scene, tick, fps, deep));
        Members<FoText3D>().ForEach(shape => shape.Render(scene, tick, fps, deep));
        Members<FoGroup3D>().ForEach(shape => shape.Render(scene, tick, fps, deep));
        Members<FoDatum3D>().ForEach(shape => shape.Render(scene, tick, fps, deep));
        return true;
    }



}

