using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding.QrCode;
using ZxingForQRcodeHiding.QrCode.Internal;

namespace QRPhotoMosaic.Method
{
    class QRCodeEncoding
    {
        public ErrorCorrectionLevel GetECLevel(string level)
        {
            if (level == "L")
                return ErrorCorrectionLevel.L;
            else if (level == "M")
                return ErrorCorrectionLevel.M;
            else if (level == "Q")
                return ErrorCorrectionLevel.Q;
            else
                return ErrorCorrectionLevel.H;
        }

        public BitMatrix CreateQR(string inputString, ErrorCorrectionLevel level)
        {
            IDictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            hints[EncodeHintType.ERROR_CORRECTION] = level;
            hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
            
            BitMatrix QRbitMatrix = null;
            int QRWeight = 200;
            int QRHeight = 200;

            try
            {
                QRbitMatrix = new QRCodeWriter().encode(inputString, BarcodeFormat.QR_CODE, QRWeight, QRHeight, hints);
            }
            catch (WriterException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return QRbitMatrix;
        }

        public Bitmap ToBitmap(BitMatrix matrix)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                }
            }
            return bitmap;
        }
    }
}
