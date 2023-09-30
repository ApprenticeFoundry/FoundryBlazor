using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;
using QRCoder;
using System.Drawing;


namespace FoundryBlazor.Shape;

public interface IQRCodeService
{
    FoImage2D? AttachQRCode(string url, FoGlyph2D parent, IDrawing drawing, double scale=0.1);
}

public class QRCodeService : IQRCodeService
{
    public FoImage2D? AttachQRCode(string url, FoGlyph2D parent, IDrawing drawing, double scale=0.1)
    {
        if (string.IsNullOrEmpty(url)) 
            return null;
            
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);



        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        var base64 = Convert.ToBase64String(qrCodeImage);
        var dataURL = $"data:image/png;base64,{base64}";

        var qrShape = new FoImage2D(80, 80, "White")
        {
            ImageUrl = dataURL,
            ScaleX = scale,
            ScaleY = scale,
        };

        // qrShape.BeforeShapeRefresh((obj,tick) => {
        //     obj.PinX = parent.PinX + 30;
        //     obj.PinY = parent.PinY + parent.Height / 2 - 10;
        // });
        drawing.AddShape<FoImage2D>(qrShape);
        return qrShape;
    }


}