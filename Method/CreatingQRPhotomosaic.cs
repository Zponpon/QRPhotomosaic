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
        private int AlignmentPatternLocation_X;
        private int AlignmentPatternLocation_Y;
        private int version;
        private string colorSpace;
        private int centerSize;
        private double robustVal;

        private double a, minusb, plusb;//function params

        public void RegisterColorCB()
        {
             
        }

        private Bitmap GlobalThresholdMask(Bitmap grayImage)
        {
            return ImageProc.PixelBasedGlobalThresholdMask(grayImage);
        }

        private Bitmap LocalThresholdMask(Bitmap grayImage, int windowSize, int moduleSize)
        {
            return ImageProc.PixelBasedLocalThresholdMask(grayImage, windowSize, moduleSize);
        }

        private Bitmap addQuietZone(Bitmap prevWork, int tileSize)
        {
            Bitmap qr = new Bitmap(prevWork.Width + tileSize * 4, prevWork.Height + tileSize * 4);
            int srcX = tileSize * 2;
            int srcY = srcX;
            for (int y = 0; y < qr.Height; y++ )
            {
                for(int x = 0; x < qr.Width; x++)
                {
                    if (x >= srcX && (x < srcX + prevWork.Width) && y >= srcY && (y < srcY + prevWork.Height))
                    {
                        qr.SetPixel(x, y, prevWork.GetPixel(x - srcX, y - srcY));
                    }
                    else
                    {
                        qr.SetPixel(x, y, Color.White);
                    }
                }
            }
            return qr;
        }

        private Bitmap GeneratingProcess(BackgroundWorker worker, BitMatrix QRMat, Bitmap pmBitmap, Bitmap mask, int tileSize, int centerSize, int robustVal, string shape)
        {
            Bitmap result = new Bitmap(pmBitmap.Width, pmBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //  tileSize==moduleSize
            #region Variable
            int numModule = version * 4 + 17;
            int halfTileSize = tileSize / 2;
            int doubleTileSize = tileSize * 2;
            int TwothirdsTileSize = doubleTileSize - halfTileSize;
            //int qrStartX = halfTileSize;
            //int qrStartY = halfTileSize;
            int qrEndX = pmBitmap.Width - halfTileSize - 1;
            int qrEndY = pmBitmap.Height - halfTileSize - 1;
            #endregion
            //  pm->photomosaic
            
            for (int pmY = 0; pmY < pmBitmap.Height; pmY += tileSize)
            {
                worker.ReportProgress(30 + (pmY * 70 / pmBitmap.Height), "Generate a photomosaic with QR code...");
                for (int pmX = 0; pmX < pmBitmap.Width; pmX += tileSize)
                {
                    int matX = (pmX / tileSize) % numModule;
                    int matY = (pmY / tileSize) % numModule;
                    #region UpLeft
                    if (pmX < tileSize * 8 && pmY < tileSize * 8)
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            //waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            //waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    #region UpRight
                    else if (pmY < tileSize * 8 && pmX < pmBitmap.Width && pmX >= (pmBitmap.Width - tileSize * 8))
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            //waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            //waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    #region DownLeft
                    else if (pmX < tileSize * 8 && pmY < pmBitmap.Height && pmY >= (pmBitmap.Height - tileSize * 8))
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            //waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            //waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    #region AlignmentPattern
                    else if (version > 1 && pmX >= AlignmentPatternLocation_X && pmX < AlignmentPatternLocation_X + tileSize * 5
                    && pmY >= AlignmentPatternLocation_Y && pmY < AlignmentPatternLocation_X + tileSize * 5)
                    {

                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            //waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            //waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                            Utility.waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    else
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            switch (shape)
                            {
                                case "Square":
                                    //result = BlackSquare(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.BlackSquare(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, plusb);
                                    break;
                                case "Circle":
                                    //result = BlackCircle(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.BlackCircle(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, plusb);
                                    break;
                                case "Diamond":
                                    //result = BlackDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.BlackDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, plusb);
                                    break;
                                case "Corner":
                                    //result = BlackDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.BlackCorner(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, plusb);
                                    break;
                                default:
                                    break;
                            }
                            //result = BlackCase(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                            //result = BlackDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                            //result = BlackCircle(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                        }
                        else
                        {
                            switch (shape)
                            {
                                case "Square":
                                    //result = WhiteSquare(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.WhiteSquare(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, minusb);
                                    break;
                                case "Circle":
                                    //result = WhiteCircle(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.WhiteCircle(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, minusb);
                                    break;
                                case "Diamond":
                                    //result = WhiteDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                    Utility.WhiteDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, minusb);
                                    break;
                                case "Corner":
                                    Utility.WhiteCorner(result, pmBitmap, mask, matX, matY, centerSize, tileSize, robustVal, colorSpace, a, minusb);
                                    break;
                                default:
                                    break;
                            }
                            //result = WhiteSquare(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                            //result = WhiteCircle(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                            //result = WhiteDiamond(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                        }
                        
                    }
                }
            }

       
            return result;
        }
        
        public Bitmap Generate(BackgroundWorker worker, QRCodeInfo info, Bitmap QRBitmap, Bitmap pmBitmap, int? tileSize, int? centerSize, int? robustVal, string colorSpace, string shape, string check, double minLum, double maxLum)
        {
            if (!tileSize.HasValue || !centerSize.HasValue || !robustVal.HasValue) return null;
            this.centerSize = centerSize.Value;
            this.robustVal = robustVal.Value;
            //this.colorSpace = colorSpace;
            this.colorSpace = "YUV";
            AlignmentPatternLocation_X = info.AlignmentPatternLocation_X * tileSize.Value;
            AlignmentPatternLocation_Y = info.AlignmentPatternLocation_Y * tileSize.Value;
            version = info.QRVersion;
            
            if (!tileSize.HasValue) return null;
            int size = pmBitmap.Width - tileSize.Value; //ex: pm->22, qr->21
            Bitmap overlapping = pmBitmap;
            if (check == "N")
            {
                worker.ReportProgress(10, "Calculate the overlapping area");
                overlapping = ImageProc.OverlappingArea(pmBitmap, size, size, tileSize.Value);
            }

            Bitmap greyImage = null;
            string path = "..\\GreyImage\\" + MainForm.singleton.masterImgName;
            //Console.WriteLine(path);
            try
            {
                greyImage = Image.FromFile(path) as Bitmap;
            }
            //FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
            catch(FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                worker.ReportProgress(20, "Generate a gray image of overlapping area");
                greyImage = ImageProc.GrayImage(overlapping, colorSpace);
                FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
                greyImage.Save(file, System.Drawing.Imaging.ImageFormat.Bmp);
            }

            path = "..\\ThresholdMask\\" + MainForm.singleton.masterImgName;
            Bitmap thresholdMask;
            try
            {
                thresholdMask = Image.FromFile(path) as Bitmap;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                worker.ReportProgress(30, "Generate pixel based threshold mask");
                thresholdMask = LocalThresholdMask(greyImage, 3, tileSize.Value);
                FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
                thresholdMask.Save(file, System.Drawing.Imaging.ImageFormat.Bmp);
            }

            if(shape != "Corner")
                //Utility.ParamsAB(tileSize.Value * 9 / 16, tileSize.Value * 5 / 16, 255.0f, 0.0f, colorSpace, out a, out minusb, out plusb);
                Utility.ParamsAB(tileSize.Value * 9 / 16, tileSize.Value * 5 / 16, maxLum, minLum, colorSpace, out a, out minusb, out plusb);
                //Utility.ParamsAB(tileSize.Value, tileSize.Value * 5 / 16, maxLum, minLum, colorSpace, out a, out minusb, out plusb);
            else
                //Utility.ParamsAB(tileSize.Value * 3 / 4, tileSize.Value * 1 / 2, 255.0f, 0.0f, colorSpace, out a, out minusb, out plusb);
                Utility.ParamsAB(tileSize.Value * 3 / 4, tileSize.Value * 1 / 2, maxLum, minLum, colorSpace, out a, out minusb, out plusb);
                //Utility.ParamsAB(tileSize.Value , tileSize.Value * 1 / 2, maxLum, minLum, colorSpace, out a, out minusb, out plusb);

            Bitmap prevWork = GeneratingProcess(worker, info.QRmatrix, overlapping, thresholdMask, tileSize.Value, centerSize.Value, robustVal.Value, shape);

            Bitmap resultQR = addQuietZone(prevWork, tileSize.Value);

            greyImage = null;
            overlapping = null;
            prevWork = null;
            GC.Collect();
            return resultQR;
        }
    }
}
