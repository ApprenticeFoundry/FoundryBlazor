using BlazorThreeJS.Viewers;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using System.Xml.Linq;

namespace FoundryBlazor.Shape;

public interface IStage
{
    FoStage3D ClearStage();
    FoStage3D UpdateStage();
    V AddShape<V>(V shape) where V : FoGlyph3D;
    T RemoveShape<T>(T value) where T : FoGlyph3D;
    (bool success, T found) FindShape<T>(string path) where T : FoGlyph3D;
}

public class FoStage3D : FoGlyph3D, IStage
{
    public static bool RefreshMenus { get; set; } = true;
    public double StageMargin { get; set; } = .50;  //meters
    public double StageWidth { get; set; } = 30.0;  //meters
    public double StageHeight { get; set; } = 30.0;  //meters
    public double StageDepth { get; set; } = 30.0;  //meters



    public FoStage3D() : base()
    {
    }

    public FoStage3D(string name) : base(name)
    {
    }
    public FoStage3D(string name, string color) : base(name, color)
    {
    }


    public FoStage3D(string name, int width, int height, int depth, string color) : base(name, color)
    {
        //ResetLocalPin((obj) => 0, (obj) => 0);
        Width = width;
        Height = height;
        Depth = depth;
    }

    public (bool success, string path, T? found) FindUsingPath<U,T>(string path) where U: FoGlyph3D where T: FoGlyph3D
    {
        var parts = path.Split('.', 2); // Split into first part and the rest
        var firstPart = parts[0];
        var rest = parts.Length > 1 ? parts[1] : null;

        var current = Slot<U>().FindWhere(x => x.GetName().Matches(firstPart));
        if (rest == null) return (false, path, null);
        var parent = current?.FirstOrDefault() as T;
        if ( parent == null) return (false, path, null);

        var result = FindMemberByPath<T>(parent, rest);
        return (result.success, path, result.found);
    }

    public (bool success, string path, T? found) FindMemberByPath<T>(string path) where T: FoGlyph3D
    {
        var parts = path.Split('.', 2); // Split into first part and the rest
        var firstPart = parts[0];
        var rest = parts.Length > 1 ? parts[1] : null;

        var current = Slot<T>().FindWhere(x => x.GetName().Matches(firstPart));
        if (rest == null) return (false, path, null);
        var parent = current?.FirstOrDefault();
        if ( parent == null) return (false, path, null);

        var result = FindMemberByPath<T>(parent, rest);
        return (result.success, path, result.found);
    }
    

    public (bool success, string path, T? found)  FindMemberByPath<T>(T parent, string path) where T: FoGlyph3D
    {
        if ( path == null) return (false, "", null);
        if ( parent == null) return (false, path, null);

        var current = parent;
        var parts = path.Split('.');
        foreach (var part in parts)
        {
            var list = current.FindWhere<T>((x) => x.GetName().Matches(part));
            current = list?.FirstOrDefault();
            if (current == null) return (false, path, null);
        }
        return (true, path, current);
    }


    public override IEnumerable<TreeNodeAction> GetTreeNodeActions()
    {
        var result = base.GetTreeNodeActions().ToList();
        result.AddAction("Clear", "btn-danger", () =>
        {
            ClearStage();
         });

        result.AddAction("Render", "btn-success", () =>
        {
            UpdateStage();
        });
        return result;
    }



    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = base.GetTreeChildren().ToList();
        var slots = AllSlotsOfType<FoGlyph3D>();

        //$"Stage GetTreeChildren Slots {slots.Count()}".WriteInfo();

        foreach (var collection in slots)
        {
            AddFolderFor(list, collection);
            //$"GetTreeChildren Adding Folder For {collection.TypeSpec.Name}".WriteInfo();
        }

        return list;
    }
    
    public FoStage3D ClearStage()
    {
        //"ClearStage All Slots".WriteInfo();
        var lists = AllSlotsOfType<FoGlyph3D>();
        foreach (var collection in lists)
        {
            //$"ClearStage {collection.TypeSpec.Name}  {collection.Count()}".WriteInfo();
            var items = collection.ValuesOfType<FoGlyph3D>().ToList();
            foreach (var item in items)
            {
                item.SetDeleteValue3D();
                item.OnDelete?.Invoke(item);
            }
        }   
        foreach (var slot in AllSlots())
            slot.Clear();

        return this;
    }

    public FoStage3D UpdateStage()
    {
       return this;
    }

    public (bool success, T found) FindMember<T>(string name) where T : FoGlyph3D
    {
        var found = Members<T>().Where(x => x.GetName().Matches(name)).FirstOrDefault();
        return (found != null, found!);
    }

    public (bool success, T found) FindShape<T>(string path) where T : FoGlyph3D
    {
        var result = FindMemberByPath<T>(path);
        return (result.success, result.found!);
    }

    public T AddShape<T>(T value) where T : FoGlyph3D
    {
        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Key))
            value.Key = collection.NextItemName();
        

        value.SetDirty(true);
        collection.AddObject(value.Key, value);

        //$"Stage AddShape Total {collection.Count()} {collection.TypeSpec.Name} {value.GetName()}".WriteInfo();
        return value;
    }

    public T RemoveShape<T>(T value) where T : FoGlyph3D
    {
        var collection = DynamicSlot(value.GetType());
        if (string.IsNullOrEmpty(value.Key))
        {
            value.Key = collection.NextItemName();
        }

        value.SetDirty(true);
        collection.RemoveObject(value.Key);

        return value;
    }


    public override bool RefreshToScene(Scene3D scene, bool deep = true)
    {

        foreach (var collection in AllSlotsOfType<FoGlyph3D>())
        {
            foreach (var item in collection.ValuesOfType<FoGlyph3D>())
            {
                item.RefreshToScene(scene, deep);
            }
        }


        return true;
    }
}