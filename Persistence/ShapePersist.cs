using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoundryBlazor.Shape;

namespace FoundryBlazor.Persistence;

public class ShapePersist<S> where S :  FoGlyph2D
{
    public S? Shape { get; set; }

    public ShapeSetPersist<FoText2D>? FoText2D { get; set; }
    public ShapeSetPersist<FoShape2D>? FoShape2D { get; set; }
    public ShapeSetPersist<FoShape1D>? FoShape1D { get; set; }
    public ShapeSetPersist<FoGroup2D>? FoGroup2D { get; set; }

    public ShapePersist()
    {
    }

    public ShapePersist<S> SaveShape(S source) 
    {
        Shape = source;
                
        if (source.HasSlot<FoShape2D>()) {
            FoShape2D = PersistShapes<FoShape2D>(source.Members<FoShape2D>());
        }
        if (source.HasSlot<FoShape1D>()) {
            FoShape1D = PersistShapes<FoShape1D>(source.Members<FoShape1D>());
        }
        if (source.HasSlot<FoText2D>()) {
            FoText2D = PersistShapes<FoText2D>(source.Members<FoText2D>());
        }
        if (source.HasSlot<FoGroup2D>()) {
            FoGroup2D = PersistShapes<FoGroup2D>(source.Members<FoGroup2D>());
        }
        return this;
    }

    public ShapeSetPersist<T>? PersistShapes<T>(List<T> list) where T : FoGlyph2D
    {
        if ( list.Count == 0) return null;

        var shapes = new ShapeSetPersist<T>();
        list.ForEach(shape =>
        {
            var Shape = new ShapePersist<T>();
            shapes.Add(Shape.SaveShape(shape));
        });
        return shapes;
    }

    public S RestoreShape() 
    {
        return (Shape as S)!;
    }
}

public class ShapeSetPersist<S> : List<ShapePersist<S>> where S : FoGlyph2D
{

}
