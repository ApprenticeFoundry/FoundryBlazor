using FoundryBlazor.Shape;

namespace FoundryBlazor.Persistence;

public class PagePersist
{
    public FoPage2D? Page { get; set; }

    public ShapeSetPersist<FoText2D>? FoText2D { get; set; }
    public ShapeSetPersist<FoShape2D>? FoShape2D { get; set; }
    public ShapeSetPersist<FoShape1D>? FoShape1D { get; set; }
    public ShapeSetPersist<FoGroup2D>? FoGroup2D { get; set; }

    public PagePersist()
    {
    }
    public PagePersist SavePage (FoPage2D source)
    {
        Page = source;
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

    public static ShapeSetPersist<T>? PersistShapes<T>(List<T> list) where T: FoGlyph2D 
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

    public void RestorePage(FoPage2D page)
    {
        FoShape2D?.ForEach(shape => page.Add<FoShape2D>(shape.RestoreShape()));
        FoShape1D?.ForEach(shape => page.Add<FoShape1D>(shape.RestoreShape()));
        FoText2D?.ForEach(shape => page.Add<FoText2D>(shape.RestoreShape()));
        FoGroup2D?.ForEach(shape => page.Add<FoGroup2D>(shape.RestoreShape()));
    }
}

public class PageSetPersist<T> : List<T> where T : PagePersist
{

}

