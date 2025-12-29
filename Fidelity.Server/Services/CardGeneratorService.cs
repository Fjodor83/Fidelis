using SkiaSharp;
using QRCoder;
using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;

namespace Fidelity.Server.Services;

public class CardGeneratorService : ICardGeneratorService
{
    public async Task<byte[]> GeneraCardDigitaleAsync(Cliente cliente, PuntoVendita? puntoVendita)
    {
        return await Task.Run(() =>
        {
            var puntoVenditaNome = puntoVendita?.Nome ?? "Suns Fidelity Card";
            const int width = 800;
            const int height = 500;
            
            var info = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            
            using (var paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(width, 0),
                    new[] { SKColor.Parse("#105a12"), SKColor.Parse("#053e30") },
                    SKShaderTileMode.Clamp
                );
                canvas.DrawRect(0, 0, width, height, paint);
            }
            
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = 36;
                paint.IsAntialias = true;
                paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
                canvas.DrawText("SUNS FIDELITY CARD", 40, 60, paint);
            }
            
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = 24;
                paint.IsAntialias = true;
                canvas.DrawText(puntoVenditaNome, 40, 100, paint);
            }
            
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = 28;
                paint.IsAntialias = true;
                paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
                canvas.DrawText($"{cliente.Nome} {cliente.Cognome}", 40, 200, paint);
            }
            
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = 32;
                paint.IsAntialias = true;
                paint.Typeface = SKTypeface.FromFamilyName("Courier New", SKFontStyle.Bold);
                canvas.DrawText(cliente.CodiceFidelity, 40, 260, paint);
            }
            
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = 24;
                paint.IsAntialias = true;
                canvas.DrawText($"Punti: {cliente.PuntiTotali}", 40, 320, paint);
            }
            
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(cliente.CodiceFidelity, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(10);
            
            using (var qrImage = SKBitmap.Decode(qrBytes))
            {
                canvas.DrawBitmap(qrImage, new SKRect(550, 150, 750, 350));
            }
            
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        });
    }

    public async Task<byte[]> GeneraQRCodeAsync(string contenuto, int dimensione = 200)
    {
        return await Task.Run(() =>
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(contenuto, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(dimensione / 10); // Approximation
        });
    }
}
