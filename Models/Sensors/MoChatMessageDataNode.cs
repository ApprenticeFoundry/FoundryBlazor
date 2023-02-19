using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace FoundryBlazor.Model
{
public class MoChatMessageDataNode : MoDataNode<SPEC_ChatMessage, UDTO_ChatMessage>
{
    public MoChatMessageDataNode() : base("Chat")
    {
        Spec = SPEC_ChatMessage.RandomSpec();
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
            // var change = tick % 60 == 0;
            // if (!change) return;
            // model.Spec = SPEC_ChatMessage.RandomSpec();
            // UpdateValues2DUsingSpec(shape, model.Spec);
        };
    }

}
}