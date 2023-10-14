
using FoundryBlazor.Shape;

public class FoDynamicRender 
{
    private Type _type { get; set; }
    private Dictionary<string,object> _source { get; set; }

    public FoDynamicRender(Type target, FoGlyph2D item) 
    {
         _type = target;
        _source = new Dictionary<string,object>()
        {
            {"Shape",  item }
        };
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