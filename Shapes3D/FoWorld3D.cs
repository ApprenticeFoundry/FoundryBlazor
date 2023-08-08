// this is a tool to load/unload knowledge modules that define projects

using BlazorThreeJS.Maths;
using FoundryBlazor.Extensions;
using IoBTMessage.Models;

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
        GetSlot<FoPathway3D>();
        GetSlot<FoRelationship3D>();
    }

    public FoWorld3D(UDTO_World source) : this()
    {
        FillFromUDTOWorld(source);
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

    public List<FoMenu3D>? Menus()
    {
        return GetMembers<FoMenu3D>();
    }

    public List<FoText3D>? Labels()
    {
        return GetMembers<FoText3D>();
    }
    public List<FoRelationship3D>? Relationships()
    {
        return GetMembers<FoRelationship3D>();
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
                Pivot = box?.PinAsVector3(),
            };
            Slot<FoShape3D>().Add(shape3D);
            $"FoShape3D from world {shape3D.Symbol} X = {shape3D.Position?.X}".WriteSuccess();

            //add the nav menu
            if (item.subSystem != null)
            {
                LayoutSystemInSwinlanes(item.subSystem, 0, 0, 14);
                shape3D.NavMenu = new FoMenu3D("NavMenu")
                {
                    Position = pos?.LocAsVector3().Add(1, 2, 0)
                };

                shape3D.TextPanel = new FoPanel3D("TextPanel")
                {
                    Width = 2,
                    Height = 1.5,
                    Color = "purple",
                    Position = pos?.LocAsVector3().Add(1, 2, -3)
                };

                var textLines = item.subSystem.Targets().Select((item) => $"Address: {item.address}").ToList();
                shape3D.TextPanel.TextLines = textLines;

                item.subSystem.Targets().ForEach((target) =>
                {
                    var button = new FoButton3D(target.address, () => $"Clicked {target.address}".WriteSuccess());
                    shape3D.NavMenu.Add(button);

                    var subPanel = shape3D.TextPanel.Establish<FoPanel3D>(target.address);
                    subPanel.TextLines.Add(target.address);
                    subPanel.Color = "green";
                    subPanel.Position = new Vector3(target.x, target.y, target.z);
                });
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


    public static void LayoutSystemInSwinlanes(DT_System system, int dx = 0, int dy = 0, int dz = 0)
    {
        if (system == null) return;


        var targets = system.Targets().OrderBy(item => item.linkCount).ToList();
        var dict = targets.GroupBy(item => item.domain).ToDictionary(item => item.Key, item => item.ToList());

        var order = new List<string>() { "WRLD", "PIN", "DOC", "PROC", "CAD", "ASST" };

        //set the target locations here 
        var x = 200;
        var y = 200;
        var z = 0;
        foreach (var item in order)
        {
            if (dict.ContainsKey(item))
            {
                y = 200;
                foreach (var target in dict[item])
                {
                    target.x = x + dx;
                    target.y = y + dy;
                    target.z = z + dz;
                    y += 200;
                }
                x += 250;
            }
        }
    }

    public FoWorld3D RemoveDuplicates()
    {

        var platforms = Platforms()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
        if (platforms != null)
            GetSlot<FoGroup3D>()?.Flush().AddRange(platforms);

        var bodies = Bodies()?.GroupBy(i => i.GlyphId).Select(g => g.First()).ToList();
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
