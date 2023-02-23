using System;
using System.Collections.Generic;
using System.Linq;

namespace FoundryBlazor.Shape;

public class FoGroup3D : FoGlyph3D
{

    public FoVector3D? position { get; set; }
    public FoVector3D? boundingBox { get; set; }
    public FoVector3D? offset { get; set; }


    private readonly Dictionary<string, object> _lookup = new();

    public FoGroup3D() : base()
    {
        GetSlot<FoShape3D>();
        GetSlot<FoText3D>();
        GetSlot<FoDatum3D>();
        GetSlot<FoRelationship3D>();
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


    public FoGroup3D EstablishBox(string name, double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
    {
        this.Name = name;
        boundingBox = new FoVector3D()
        {
            units = units,
            X = width,
            Y = height,
            Z = depth,
        };
        position = new FoVector3D();
        offset = new FoVector3D();
        return this;
    }



    public T CreateUsingDTBASE<T>(FoGlyph3D obj) where T : FoGlyph3D
    {
        return CreateUsing<T>(obj.Name, obj.uniqueGuid);
    }

    public FoShape3D CreateCylinder(FoGlyph3D obj, double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
    {
        var result = CreateUsingDTBASE<FoShape3D>(obj);
        return result.CreateCylinder(obj.Name, width, height, depth, units);
    }

    public FoShape3D CreateBlock(FoGlyph3D obj, double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
    {
        var result = CreateUsingDTBASE<FoShape3D>(obj);
        return result.CreateBox(obj.Name, width, height, depth, units);
    }

    public FoShape3D CreateSphere(FoGlyph3D obj, double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
    {
        var result = CreateUsingDTBASE<FoShape3D>(obj);
        return result.CreateSphere(obj.Name, width, height, depth, units);
    }

    public FoShape3D CreateGlb(FoGlyph3D obj, string url, double width = 1.0, double height = 1.0, double depth = 1.0, string units = "m")
    {
        var result = CreateUsingDTBASE<FoShape3D>(obj);
        return result.CreateGlb(url, width, height, depth, units);
    }

    public FoText3D CreateLabel(FoGlyph3D obj, string text, double xLoc = 0.0, double yLoc = 0.0, double zLoc = 0.0, string units = "m")
    {
        var result = CreateUsingDTBASE<FoText3D>(obj);
        return result.CreateTextAt(text, xLoc, yLoc, zLoc, units);
    }


    public FoGroup3D SetPositionTo(FoVector3D loc)
    {
        position = loc;
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



    public U RelateMembers<U>(FoGlyph3D source, string name, FoGlyph3D target) where U : FoRelationship3D
    {
        var tag = $"{source.uniqueGuid}:{name}";
        var relationship = Find<U>(tag);
        if (relationship == null)
        {
            relationship = FindOrCreate<U>(tag, true);
            relationship.Build(source.uniqueGuid, name, target.uniqueGuid);
        }
        else
        {
            relationship.Relate(target.uniqueGuid);
        }

        return relationship;
    }

    public U? UnrelateMembers<U>(FoGlyph3D source, string name, FoGlyph3D target) where U : FoRelationship3D
    {
        var tag = $"{source.uniqueGuid}:{name}";
        var relationship = Find<U>(tag);
        relationship?.Unrelate(target.uniqueGuid);

        return relationship;
    }


 

    private T CreateItem<T>(string name) where T : FoGlyph3D
    {
        var found = Activator.CreateInstance<T>() as T;
        found.Name = name;
        found.platformName = platformName;
        found.uniqueGuid = Guid.NewGuid().ToString();
        return found;
    }



    public T CreateUsing<T>(string name, string guid = "") where T : FoGlyph3D
    {
        var found = FindOrCreate<T>(name, true);
        if (!string.IsNullOrEmpty(guid) )
            found!.uniqueGuid = guid;

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



}

