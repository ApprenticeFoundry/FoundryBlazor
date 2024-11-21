// this is a tool to load/unload knowledge modules that define projects





using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Models;


namespace FoundryBlazor.Shape;

// a World is just of bag o things to draw,  then need to be handed to a arena to do the rendering
// on to a stage or scene

public interface IWorld3D: ITreeNode
{
    //public List<FoShape2D>? Shapes();
    public List<FoGroup3D>? ShapeGroups();
    public List<FoDatum3D>? Datums();
    public List<FoShape3D>? ShapeBodies();
    public List<FoMenu3D>? Menus();
    public List<FoPanel3D>? Panels();
    public List<FoText3D>? Labels();
    public List<FoRelationship3D>? Relationships();

    public T AddGlyph3D<T>(T glyph) where T : FoGlyph3D;
    public T RemoveGlyph3D<T>(T glyph) where T : FoGlyph3D;

    public string GetName();

    public IWorld3D RemoveDuplicates();
    public IWorld3D ClearAll();
    void AddAction(string name, string color, Action action);
    bool PublishToStage(FoStage3D stage);
    bool PublishToArena(IArena arena);
}

public class FoWorld3D : FoGlyph3D, IWorld3D
{

    public FoWorld3D(string name) : base(name)
    {
        GetSlot<FoGroup3D>();
        GetSlot<FoShape3D>();
        GetSlot<FoText3D>();
        GetSlot<FoDatum3D>();
        GetSlot<FoMenu3D>();
        GetSlot<FoPanel3D>();
        GetSlot<FoPathway3D>();
    }





    public T AddGlyph3D<T>(T glyph) where T : FoGlyph3D
    {
        if (glyph is FoGroup3D group)
            Slot<FoGroup3D>()?.Add(group);
        else if (glyph is FoShape3D shape)
            Slot<FoShape3D>()?.Add(shape);
        else if (glyph is FoText3D text)
            Slot<FoText3D>()?.Add(text);
        else if (glyph is FoDatum3D datum)
            Slot<FoDatum3D>()?.Add(datum);
        else if (glyph is FoMenu3D menu)
            Slot<FoMenu3D>()?.Add(menu);
        else if (glyph is FoPanel3D panel)
            Slot<FoPanel3D>()?.Add(panel);
        else if (glyph is FoPathway3D pathway)
            Slot<FoPathway3D>()?.Add(pathway);
        else
            throw new Exception("Unknown Glyph Type");
        
        return glyph;
    }

    public T RemoveGlyph3D<T>(T glyph) where T : FoGlyph3D
    {
        if (glyph is FoGroup3D group)
            GetSlot<FoGroup3D>()?.Remove(group);
        if (glyph is FoShape3D shape)
            GetSlot<FoShape3D>()?.Remove(shape);
        if (glyph is FoText3D text)
            GetSlot<FoText3D>()?.Remove(text);
        if (glyph is FoDatum3D datum)
            GetSlot<FoDatum3D>()?.Remove(datum);
        if (glyph is FoMenu3D menu)
            GetSlot<FoMenu3D>()?.Remove(menu);
        if (glyph is FoPanel3D panel)
            GetSlot<FoPanel3D>()?.Remove(panel);
        if (glyph is FoPathway3D pathway)
            GetSlot<FoPathway3D>()?.Remove(pathway);
        return glyph;
    }

    public IWorld3D ClearAll()
    {
        GetSlot<FoGroup3D>()?.Clear();
        GetSlot<FoShape3D>()?.Clear();
        GetSlot<FoText3D>()?.Clear();
        GetSlot<FoDatum3D>()?.Clear();
        GetSlot<FoMenu3D>()?.Clear();
        GetSlot<FoPanel3D>()?.Clear();
        GetSlot<FoPathway3D>()?.Clear();
        return this;
    }

    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        AddFolderIfNotEmpty<FoGroup3D>(list);
        AddFolderIfNotEmpty<FoShape3D>(list);
        AddFolderIfNotEmpty<FoText3D>(list);
        AddFolderIfNotEmpty<FoDatum3D>(list);

        AddFolderIfNotEmpty<FoMenu3D>(list);

        AddFolderIfNotEmpty<FoPanel3D>(list);
        AddFolderIfNotEmpty<FoPathway3D>(list);
        return list;
    }

    public bool PublishToStage(FoStage3D stage)
    {
        GetMembers<FoShape3D>()?.ForEach(shape => stage.AddShape<FoShape3D>(shape));
        GetMembers<FoGroup3D>()?.ForEach(group => stage.AddShape<FoGroup3D>(group));
        GetMembers<FoText3D>()?.ForEach(label => stage.AddShape<FoText3D>(label));
        GetMembers<FoDatum3D>()?.ForEach(datum => stage.AddShape<FoDatum3D>(datum));
        GetMembers<FoMenu3D>()?.ForEach(menu => stage.AddShape<FoMenu3D>(menu));
        GetMembers<FoPanel3D>()?.ForEach(panel => stage.AddShape<FoPanel3D>(panel));
        GetMembers<FoPathway3D>()?.ForEach(pathway => stage.AddShape<FoPathway3D>(pathway));

        return true;
    }

    public bool PublishToArena(IArena arena)
    {
        GetMembers<FoShape3D>()?.ForEach(shape => arena.AddShape<FoShape3D>(shape));
        GetMembers<FoGroup3D>()?.ForEach(group => arena.AddShape<FoGroup3D>(group));
        GetMembers<FoText3D>()?.ForEach(label => arena.AddShape<FoText3D>(label));
        GetMembers<FoDatum3D>()?.ForEach(datum => arena.AddShape<FoDatum3D>(datum));
        GetMembers<FoMenu3D>()?.ForEach(menu => arena.AddShape<FoMenu3D>(menu));
        GetMembers<FoPanel3D>()?.ForEach(panel => arena.AddShape<FoPanel3D>(panel));
        GetMembers<FoPathway3D>()?.ForEach(pathway => arena.AddShape<FoPathway3D>(pathway));

        return true;
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




    public IWorld3D RemoveDuplicates()
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
