// this is a tool to load/unload knowledge modules that define projects





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
        GetSlot<FoMenu3D>();
        GetSlot<FoPanel3D>();
        GetSlot<FoPathway3D>();
        GetSlot<FoRelationship3D>();
    }

    public FoWorld3D(UDTO_World source) : this()
    {
        FillFromUDTOWorld(source);
    }

    public FoWorld3D FillFromUDTOWorld(UDTO_World world)
    {
        world.platforms.ForEach(item =>
        {
            var group = new FoGroup3D()
            {
                PlatformName = item.platformName,
                GlyphId = item.uniqueGuid,
                Name = item.name,
            };
            Slot<FoGroup3D>().Add(group);
        });

        world.bodies.ForEach(item =>
        {
            var pos = item.position;
            var box = item.boundingBox;
            var shape3D = new FoShape3D()
            {
                PlatformName = item.platformName,
                GlyphId = item.uniqueGuid,
                Name = item.name,
                Address = item.address,
                Symbol = item.symbol,
                Type = item.type,
                Color = string.IsNullOrEmpty(item.material) ? "Green" : item.material,
                Position = pos?.LocAsVector3(),
                Rotation = pos?.AngAsVector3(),
                BoundingBox = box?.BoxAsVector3(),
                Scale = box?.ScaleAsVector3(),
                Pivot = box?.PinAsVector3(),
            };
            Slot<FoShape3D>().Add(shape3D);
            //$"FoShape3D from world {shape3D.Symbol} X = {shape3D.Position?.X}".WriteSuccess();
            if (item.subSystem != null)
            {
                shape3D.Targets = item.subSystem.Targets();
            }

        });

        world.labels.ForEach(item =>
        {
            var pos = item.position;
            var text3D = new FoText3D()
            {
                PlatformName = item.platformName,
                GlyphId = item.uniqueGuid,
                Name = item.name,
                Address = item.address,
                Position = pos?.LocAsVector3(),
                Text = item.text,
                Details = item.details
            };
            Slot<FoText3D>().Add(text3D);
        });

        return this;
    }


    public List<FoGroup3D>? ShapeGroups()
    {
        return GetMembers<FoGroup3D>();
    }
    public List<FoDatum3D>? Datums()
    {
        return GetMembers<FoDatum3D>();
    }

    public List<FoShape3D>? ShapeBodies()
    {
        return GetMembers<FoShape3D>();
    }

    public List<FoMenu3D>? Menus()
    {
        return GetMembers<FoMenu3D>();
    }
    public List<FoPanel3D>? Panels()
    {
        return GetMembers<FoPanel3D>();
    }

    public List<FoText3D>? Labels()
    {
        return GetMembers<FoText3D>();
    }
    public List<FoRelationship3D>? Relationships()
    {
        return GetMembers<FoRelationship3D>();
    }


    //public static string GetColor(DT_Target model)
    //{
    //    var Color = model.domain switch
    //    {
    //        "PIN" => "Pink",
    //        "PROC" => "Wisteria",
    //        "DOC" => "Gray",
    //        "ASST" => "Aqua",
    //        "CAD" => "Orange",
    //        "WRLD" => "Green",
    //        _ => "Yellow",
    //    };
    //    return Color;
    //}



    public FoWorld3D RemoveDuplicates()
    {

        var platforms = ShapeGroups()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
        if (platforms != null)
            GetSlot<FoGroup3D>()?.Flush().AddRange(platforms);

        var bodies = ShapeBodies()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
        if (bodies != null)
            GetSlot<FoShape3D>()?.Flush().AddRange(bodies);

        var labels = Labels()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
        if (labels != null)
            GetSlot<FoText3D>()?.Flush().AddRange(labels);

        var datums = Datums()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
        if (datums != null)
            GetSlot<FoDatum3D>()?.Flush().AddRange(datums);

        var relationships = Relationships()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
        if (relationships != null)
            GetSlot<FoRelationship3D>()?.Flush().AddRange(relationships);

        return this;
    }
}
