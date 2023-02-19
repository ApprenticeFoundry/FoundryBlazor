using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;

public class MoSystemDataNode : MoDataNode<SPEC_System, UDTO_System>
{
    public MoSystemDataNode() : base("System")
    {
        Spec = SPEC_System.RandomSpec();
    }

    public override void ApplyExternalMethods(FoCompound2D shape)
    {
        var model = this;
        shape.GlyphId = model.ModelId;

        GenerateValues2D(shape);

        shape.ContextLink = (obj, tick) =>
        {
            model.Tick = tick;
            SetDrawModelText(shape);
        };
    }
}
