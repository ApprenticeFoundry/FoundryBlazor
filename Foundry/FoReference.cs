namespace FoundryBlazor;
public class FoReference: FoBase
{
    private FoBase? _reference;
    public FoReference(FoBase source) : base(source.Key)
    {
        _reference = source;
    }

    public override string GetTreeNodeTitle()
    {
        return $"> {_reference?.GetTreeNodeTitle()} *";
    }

    public bool IsReferenceTo(FoBase source)
    {
        return _reference == source;
    }

}