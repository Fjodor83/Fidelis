// Fidelity.Server/Services/CardGeneratorService.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Fidelity.Shared.Models;
using QRCoder;

namespace Fidelity.Server.Services
{
    public class CardGeneratorService : ICardGeneratorService
    {
        public async Task<byte[]> GeneraCardDigitaleAsync(Cliente cliente, PuntoVendita puntoVendita)
        {
            await Task.CompletedTask;

            // Dimensioni card: 800x500 px (proporzioni carta di credito)
            int width = 800;
            int height = 500;

            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);

            // Abilita anti-aliasing per qualità migliore
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // ===== SFONDO GRADIENTE SUNS =====
            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, width, height),
                Color.FromArgb(16, 90, 18),    // #105a12ff
                Color.FromArgb(5, 62, 48),     // #053e30ff
                LinearGradientMode.Horizontal))
            {
                graphics.FillRectangle(brush, 0, 0, width, height);
            }

            // ===== PATTERN DECORATIVO =====
            using (var penPattern = new Pen(Color.FromArgb(50, 255, 255, 255), 2))
            {
                for (int i = 0; i < width; i += 40)
                {
                    graphics.DrawLine(penPattern, i, 0, i + 100, height);
                }
            }


            // ===== LOGO SUNS (IMMAGINE) =====
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "images", "SunsLogo.webp");
            if (File.Exists(logoPath))
            {
                using var logoStream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
                using var logoImage = Image.FromStream(logoStream);
                
                // Disegna il logo con dimensione proporzionata
                int logoWidth = 120;
                int logoHeight = 120;
                graphics.DrawImage(logoImage, new Rectangle(40, 30, logoWidth, logoHeight));
            }
            else
            {
                // Fallback al testo se l'immagine non esiste
                using (var fontLogo = new Font("Arial", 48, FontStyle.Bold))
                using (var brushWhite = new SolidBrush(Color.White))
                {
                    graphics.DrawString("☀️ SUNS", fontLogo, brushWhite, new PointF(40, 40));
                }
            }

            using (var fontSubtitle = new Font("Arial", 18, FontStyle.Regular))
            using (var brushWhite = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                graphics.DrawString("Zero & Company", fontSubtitle, brushWhite, new PointF(45, 155));
            }

            // ===== NOME CLIENTE =====
            string nomeCompleto = $"{cliente.Nome} {cliente.Cognome}".ToUpper();
            using (var fontNome = new Font("Arial", 24, FontStyle.Bold))
            using (var brushWhite = new SolidBrush(Color.White))
            {
                graphics.DrawString(nomeCompleto, fontNome, brushWhite, new PointF(40, 200));
            }

            // ===== CODICE FIDELITY =====
            using (var fontCodice = new Font("Courier New", 32, FontStyle.Bold))
            using (var brushGold = new SolidBrush(Color.FromArgb(255, 223, 128)))
            {
                graphics.DrawString(cliente.CodiceFidelity, fontCodice, brushGold, new PointF(40, 260));
            }

            // ===== PUNTI ATTUALI =====
            using (var fontPunti = new Font("Arial", 18, FontStyle.Regular))
            using (var brushWhite = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                graphics.DrawString($"Punti: {cliente.PuntiTotali}", fontPunti, brushWhite, new PointF(40, 320));
            }

            // ===== PUNTO VENDITA REGISTRAZIONE =====
            using (var fontPv = new Font("Arial", 14, FontStyle.Italic))
            using (var brushWhite = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            {
                graphics.DrawString($"Registrato presso: {puntoVendita.Nome}", fontPv, brushWhite, new PointF(40, 355));
            }

            // ===== DATA REGISTRAZIONE =====
            using (var fontData = new Font("Arial", 12, FontStyle.Regular))
            using (var brushWhite = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
            {
                string dataReg = cliente.DataRegistrazione.ToString("dd/MM/yyyy");
                graphics.DrawString($"Membro dal: {dataReg}", fontData, brushWhite, new PointF(40, 385));
            }

            // ===== QR CODE =====
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(cliente.CodiceFidelity, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(10, new byte[] {0,0,0}, new byte[] {255,255,255}); // Dark (black), Light (white)
            
            using var msQr = new MemoryStream(qrBytes);
            using var qrBitmap = new Bitmap(msQr);

            // Disegna QR code con bordo bianco
            int qrSize = 180;
            int qrX = width - qrSize - 50;
            int qrY = height - qrSize - 50;

            // Bordo bianco attorno al QR
            using (var brushWhiteBg = new SolidBrush(Color.White))
            {
                graphics.FillRectangle(brushWhiteBg, qrX - 10, qrY - 10, qrSize + 20, qrSize + 20);
            }

            graphics.DrawImage(qrBitmap, qrX, qrY, qrSize, qrSize);

            // Testo sotto QR
            using (var fontQr = new Font("Arial", 11, FontStyle.Regular))
            using (var brushWhite = new SolidBrush(Color.White))
            {
                var textSize = graphics.MeasureString("Scansiona per punti", fontQr);
                float textX = qrX + (qrSize - textSize.Width) / 2;
                graphics.DrawString("Scansiona per punti", fontQr, brushWhite, new PointF(textX, qrY + qrSize + 15));
            }

            // ===== FOOTER =====
            using (var fontFooter = new Font("Arial", 10, FontStyle.Regular))
            using (var brushWhite = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
            {
                graphics.DrawString("www.sunscompany.com • Fidelity Program", fontFooter, brushWhite, new PointF(40, height - 35));
            }

            // ===== CONVERTI IN BYTE ARRAY =====
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
