using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;
using QRCoder;
using SkiaSharp;
using System.Drawing;


namespace FoundryBlazor.Shape;

public interface IQRCodeService
{
    FoImage2D? CreateQRCodeImage(string url, IDrawing drawing, int size=100, double scale=0.1);
    SKData CreateQRCodePNG(string url, int width, int height);
}

public class QRCodeService : IQRCodeService
{
    public FoImage2D? CreateQRCodeImage(string url, IDrawing drawing, int size,  double scale=0.1)
    {
        if (string.IsNullOrEmpty(url)) 
            return null;
            
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);


        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        var base64 = Convert.ToBase64String(qrCodeImage);
        var dataURL = $"data:image/png;base64,{base64}";

        var qrShape = new FoImage2D(size, size, "White")
        {
            ImageUrl = dataURL,
            ScaleX = scale,
            ScaleY = scale,
        };

        drawing.AddShape<FoImage2D>(qrShape);
        return qrShape;
    }

    public  SKData CreateQRCodePNG(string url, int width, int height)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);

        var bitmap = SKBitmap.Decode(qrCodeImage);
        var resized = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
        var png = resized.Encode(SKEncodedImageFormat.Png, 2);

        // using var memoryStream = new MemoryStream();
        // png.AsStream().CopyTo(memoryStream);
        // var base64 = Convert.ToBase64String(memoryStream.ToArray());
        // var dataURL = $"data:image/png;base64,{base64}";

        return png;
    }

}