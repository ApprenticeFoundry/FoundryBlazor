using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;

public class MoBiometricDataNode : MoDataNode<SPEC_Biometric, UDTO_Biometric>
{
    private FoCompound2D? Shape;
    public MoBiometricDataNode() : base("Bio")
    {
        Spec = SPEC_Biometric.RandomSpec();
    }
    public override void Start() 
    {
        if ( Shape != null)
            Shape.ApplyLayout = true;  // move to start and stop
    }
    public override void OnNext(int tick) 
    {
        Spec = SPEC_Biometric.RandomSpec();
        Forward(Spec);

        if ( Shape != null )
        {
            UpdateValues2DUsingSpec(Shape, Spec);
        }

        base.OnNext(tick);
    }
    public override void ApplyExternalMethods(FoCompound2D shape)
    {
        var model = this;
        Shape = shape;
        shape.GlyphId = model.ModelId;

        GenerateValues2D(shape);

        shape.ContextLink = (obj, tick) =>
        {
            model.Tick = tick;
            SetDrawModelText(shape);
            // var change = tick % 30 == 0;
            // if (!change) return;
            // model.Spec = SPEC_Biometric.RandomSpec();
            // UpdateValues2DUsingSpec(shape, model.Spec);
        };
    }
}



