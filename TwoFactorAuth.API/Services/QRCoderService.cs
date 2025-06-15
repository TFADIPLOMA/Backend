using QRCoder;

namespace TwoFactorAuth.API.Services
{
    public class QRCoderService
    {
        public byte[] Generate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception("BAD_REQUEST");
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);

        } 
    }
}
