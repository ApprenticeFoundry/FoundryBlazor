using FoundryBlazor.Shape;

namespace FoundryBlazor.Persistence;

public class PagePersist
{
    public FoPage2D? Page { get; set; }

    public ShapeSetPersist<FoGlyph2D>? Other { get; set; }
    public ShapeSetPersist<FoGlyph2D>? Shapes1D { get; set; }
    public ShapeSetPersist<FoGlyph2D>? Shapes2D { get; set; }


    public PagePersist()
    {
    }
    public PagePersist SavePage (FoPage2D source)
    {
        Page = source;
        if (source.GetShapes1D().Count > 0) {
            Shapes1D = PersistShapes<FoGlyph2D>(source.GetShapes1D());
        }
        if (source.GetShapes2D().Count > 0) {
            Shapes2D = PersistShapes<FoGlyph2D>(source.GetShapes2D());
        }
        if (source.GetHiddenShapes().Count > 0) {
            Other = PersistShapes<FoGlyph2D>(source.GetHiddenShapes());
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
        Shapes2D?.ForEach(shape => page.Add<FoGlyph2D>(shape.RestoreShape()));
        Shapes1D?.ForEach(shape => page.Add<FoGlyph2D>(shape.RestoreShape()));
        Other?.ForEach(shape => page.Add<FoGlyph2D>(shape.RestoreShape()));
    }
}

public class PageSetPersist<T> : List<T> where T : PagePersist
{

}

