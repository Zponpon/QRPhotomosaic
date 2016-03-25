using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Client.Result;
using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding.QrCode.Internal;
using ZxingForQRcodeHiding.QrCode;

namespace QRPhotoMosaic.Method
{
    public class CreatingQRPhotomosaic
    {
        //public Bitmap masterImg { set; get; }//public String QRcodeContent { set; get; }
        public Bitmap photomosaicImg { set; get; }
        public QRCodeInfo infoOfQRCode { set; get; }
        public Bitmap QRCode { set; get; }
        public String ColorSpace { set; get; }
        public int? tileSize { set; get; }
        public int? centerSize { set; get; }
        public int? robustVal { set; get; }

        public void Reset()
        {
            photomosaicImg = null;
            infoOfQRCode = null;
            QRCode = null;
            ColorSpace = null;
            tileSize = null;
            centerSize = null;
            robustVal = null;
        }



        public Bitmap GenerateThresholdMask()
        {
            return null;
        }

        public Bitmap GenerateOurResult()
        {
            return null;
        }
    }
}
