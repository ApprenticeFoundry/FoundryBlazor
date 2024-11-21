
using FoundryBlazor.Shape;
using FoundryBlazor.Shared.SVG;

public class FoDynamicRender 
{
    private Type _type { get; set; }
    private Dictionary<string,object> _source { get; set; }

    public static FoDynamicRender Video2D(Dictionary<string,object> parameters) 
    {
        return new FoDynamicRender(typeof(FoVideo2D), parameters);
    }
    
    public static FoDynamicRender Image2D(Dictionary<string,object> parameters) 
    {
        return new FoDynamicRender(typeof(FoImage2D), parameters);
    }

    public FoDynamicRender(Type target, FoGlyph2D item) 
    {
         _type = target;
        _source = new Dictionary<string,object>()
        {
            {"Shape",  item }
        };
    }

    public FoDynamicRender(Type target, Dictionary<string,object> parameters) 
    {
         _type = target;
        _source = parameters;
    }


    public Type DynamicType()
    {
        return _type;
    }

    public Dictionary<string,object> DynamicParameters()
    {
        return _source;
    }
}