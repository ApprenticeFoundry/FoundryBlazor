using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Microsoft.AspNetCore.Components;
using QRCoder;
using System.Text;

namespace FoundryBlazor.Shared.SVG;

public class QRCodeBase : SVGBase<FoImage2D>
{
    [Inject] protected ISelectionService? SelectionService { get; set; }

    [Parameter] public FoImage2D Shape { get; set; } = new();
    protected int PaddingX { get; set; } = 12;
    protected int PaddingY { get; set; } = 12;

    protected string GetURL()
    {
        return Shape.ImageUrl;
    }

    protected string GetStyle()
    {
        var style = new StringBuilder("width:").Append(GetWidth() - 2 * PaddingX).Append("px;").Append("height:").Append(GetHeight() - 2 * PaddingY).Append("px").ToString();
        return style;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitSource(Shape);
    }

    // public void AttachQRCode(string url, FoGlyph2D parent, IDrawing drawing)
    // {
    //     if (string.IsNullOrEmpty(url)) 
    //         return;

    //     var qrGenerator = new QRCodeGenerator();
    //     var qrCodeData = qrGenerator.CreateQrCode("Any Text", QRCodeGenerator.ECCLevel.Q);

    //     var qrCode = new PngByteQRCode(qrCodeData);
    //     var qrCodeImage = qrCode.GetGraphic(20);
    //     var base64 = Convert.ToBase64String(qrCodeImage);
    //     // var dataURL = $"data:image/png;base64,{base64}";

    //     var qrShape = new FoImage2D(80, 80, "White")
    //     {
    //         ImageUrl = dataURL,
    //         ScaleX = 0.10,
    //         ScaleY = 0.10,
    //     };

    //     qrShape.BeforeShapeRefresh((obj,tick) => {
    //         obj.PinX = parent.PinX + 30;
    //         obj.PinY = parent.PinY + parent.Height / 2 - 10;
    //     });
    //     drawing.AddShape<FoImage2D>(qrShape);

    // }


}
