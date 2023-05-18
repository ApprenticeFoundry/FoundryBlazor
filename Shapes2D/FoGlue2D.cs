using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;


public class FoGlue2D : FoBase
{
    private IGlueOwner? Source = null;
    private FoGlyph2D? Target = null;
    private FoGlyph2D? Body = null;
    public FoGlue2D(string name) : base(name)
    {
    }

    public void Deconstruct(out IGlueOwner source, out FoGlyph2D target, out FoGlyph2D body)
    {
        source = Source!;
        target = Target!;
        body = Body!;
    }

    public bool HasTarget(FoGlyph2D target)
    {
        return target == Target;
    }

    public bool HasBody(FoGlyph2D body)
    {
        return body == Body;
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

    public FoGlue2D GlueTo(IGlueOwner source, FoGlyph2D target, FoGlyph2D body) 
    {
        this.Source = source;
        this.Target = target;  //if glued to center, target == body, otherwise the target is the subshape
        this.Body = body;  //if glued to center, the body is the target, otherwise the target is the subshape

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
        this.Body = null;       

        return this;
    }
}
