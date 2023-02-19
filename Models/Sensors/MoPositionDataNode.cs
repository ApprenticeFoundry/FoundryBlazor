using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;

public class MoPositionDataNode : MoDataNode<SPEC_Position, UDTO_Position>
{
    public MoPositionDataNode() : base("Pos")
    {
        Spec = SPEC_Position.RandomSpec();
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
            // var change = tick % 30 == 0;
            // if (!change) return;
            // model.Spec = SPEC_Position.RandomSpec();
            // UpdateValues2DUsingSpec(shape, model.Spec);
        };
    }
}
