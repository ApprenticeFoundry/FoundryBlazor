using FoundryBlazor.Shape;
using IoBTMessage.Models;

namespace FoundryBlazor.Model;

public class MoImageDataNode : MoDataNode<SPEC_Image, UDTO_Image>
{
    public MoImageDataNode() : base("Image")
    {
        Spec = SPEC_Image.RandomSpec();
    }

    public override void ApplyExternalMethods(FoCompound2D shape)
    {
        var model = this;
        shape.GlyphId = model.ModelId;

        GenerateValues2D(shape);
        var source = this.Spec;
        var image = new FoImage2D(source!.width, source.height, "Black")
        {
            IsVisible = false,
            ImageUrl = source.url
        };  
        shape.Add<FoImage2D>(image);
        image.ResetLocalPin((obj) => 0, (obj) => 0);


        shape.ContextLink = (obj, tick) =>
        {
            model.Tick = tick;
            SetDrawModelText(shape);

            // var change = tick % 60 == 0;
            // if (!change) return;
            
            // var spec = SPEC_Image.RandomSpec();
            // image.ResizeTo(spec.width, spec.height);
            // image.ImageUrl = spec.url;
            // model.Spec = spec;
            // UpdateValues2DUsingSpec(shape, spec);
            // shape.ApplyLayout = true;
            // //shape.Refresh(false);
        };
    }
}
