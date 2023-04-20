using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;


public class FoGlue2D : FoBase
{
    private IGlueOwner? Source = null;
    private FoGlyph2D? Target = null;

    public FoGlue2D(string name) : base(name)
    {
    }

    public void Deconstruct(out IGlueOwner source, out FoGlyph2D target)
    {
        source = Source!;
        target = Target!;
    }

    public bool HasTarget(FoGlyph2D target)
    {
        return target == Target;
    }


    
    public bool TargetMoved(FoGlyph2D target)
    {
        if ( !HasTarget(target) || Target == null) return false;
        return Source?.Smash(false) ?? false;
    }

    public bool MarkSelected(bool value)
    {
        return (Source?.MarkSelected(value)) != null;

    }

    public FoGlue2D GlueTo(IGlueOwner source, FoGlyph2D target) 
    {
        this.Source = source;
        this.Target = target;

        Source.AddGlue(this);
        Target.AddGlue(this);

        TargetMoved(Target);
        return this;
    }

    public FoGlue2D UnGlue() 
    {
        Source?.Smash(false);

        Source?.RemoveGlue(this);
        Target?.RemoveGlue(this);

        this.Source = null;
        this.Target = null;
        

        return this;
    }
}
