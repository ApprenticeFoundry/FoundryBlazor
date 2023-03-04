using FoundryBlazor.Extensions;

namespace FoundryBlazor.Shape;


public class FoGlue2D : FoBase
{
    private IGlueOwner? Source = null;
    private FoGlyph2D? Target = null;

    public FoGlue2D(string name) : base(name)
    {
    }

    public bool HasTarget(FoGlyph2D target)
    {
        return target == Target;
    }
    public bool TargetMoved(FoGlyph2D target)
    {
        if ( !HasTarget(target) || Target == null) return false;

        $"{Name} TargetMoved {target.Name}".WriteLine(ConsoleColor.DarkBlue);

        if ( Name.StartsWith("Finish") && Source != null )
        {
            Source.ComputeFinishFor(Target);
            //$"{Source} SetFinishTo {Target.Name}".WriteInfo(4);
            return true;
        }
        else if ( Name.StartsWith("Start")  && Source != null)
        {
            Source.ComputeStartFor(Target);
            //$"{Source} SetStartTo {Target.Name}".WriteInfo(4);
            return true;
        }
        return false;
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
        Source?.RemoveGlue(this);
        Target?.RemoveGlue(this);

        this.Source = null;
        this.Target = null;
        

        return this;
    }
}
