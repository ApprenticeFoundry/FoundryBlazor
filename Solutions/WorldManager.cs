using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BlazorComponentBus;
using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;

using FoundryBlazor.Solutions;
using FoundryBlazor;

namespace FoundryBlazor.Solutions;

public interface IWorldManager
{
    // remember a World3D is just a bag of shapes to render, it is not a scene
    // it is the IArena that is the scene

    IWorld3D? FindWorld<T>(string title) where T : FoWorld3D;
    T CreateWorld<T>(string title, bool clear=true) where T : FoWorld3D;
    IWorld3D EstablishWorld(string title);
    IWorld3D AddWorld(FoWorld3D world, bool clear = true);
    List<FoWorld3D> AllWorlds();
}

public class WorldManager : FoComponent, IWorldManager
{

    public WorldManager()
    {
    }
    public List<FoWorld3D> AllWorlds()
    {
        return Members<FoWorld3D>();
    }

    public IWorld3D? FindWorld<T>(string title) where T : FoWorld3D
    {
        var found = GetMembers<FoWorld3D>()?.Where(obj => title.Matches(obj.Key)).FirstOrDefault(); 
        return found;
    }


    public IWorld3D EstablishWorld(string title) 
    {
        var found = GetMembers<FoWorld3D>()?.Where(obj => title.Matches(obj.Key)).FirstOrDefault(); 
        if ( found != null) 
            return found;

        return CreateWorld<FoWorld3D>(title);
    }

    public IWorld3D AddWorld(FoWorld3D world, bool clear=true)
    {
        if ( clear )
            GetMembers<FoWorld3D>()?.Clear();

        Add<FoWorld3D>(world);
        return world;
    }

    public T CreateWorld<T>(string title, bool clear=false) where T : FoWorld3D
    {
        if ( clear )
            GetMembers<FoWorld3D>()?.Clear();

        var world = (Activator.CreateInstance(typeof(T), title) as T)!;
        AddWorld(world);

        return world;
    }

 

}
