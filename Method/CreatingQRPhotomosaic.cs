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
        private double Alpha_Magnification = 0.5f;
        private int version;
        private string colorSpace;
        private int centerSize;
        private double robustVal;

        public void RegisterColorCB()
        {
             
        }

        public BitMatrix ScaleQRCode(QRCodeInfo info, int tileSize)
        {
            int multiTileSize = tileSize * 2;
            int w = info.QRmatrix.Width * multiTileSize;
            int h = info.QRmatrix.Height * multiTileSize;
            Bitmap ScaleQR = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int i = 0, j = 0;
            BitMatrix QRMat = new BitMatrix(w, h);
            for (int y = 0; y < w; ++y )
            {
                for(int x = 0; x < h; ++x)
                {
                    i = x / multiTileSize;
                    j = y / multiTileSize;
                    if(info.QRmatrix[i, j])
                    {
                        QRMat[x, y] = true;
                        ScaleQR.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    }
                    else
                    {
                        QRMat[x, y] = false;
                        ScaleQR.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            return QRMat;
        }

        private Bitmap GlobalThresholdMask(Bitmap grayImage)
        {
            return ImageProc.PixelBasedGlobalThresholdMask(grayImage);
        }

        private Bitmap LocalThresholdMask(Bitmap grayImage, int windowSize, int moduleSize)
        {
            return ImageProc.PixelBasedLocalThresholdMask(grayImage, windowSize, moduleSize);
        }

        #region Embedding Work Region
        private void waterMarkDark(Bitmap result, Bitmap pmBitmap, int x, int y, int moduleLength)
        {
            Color SourceImageColor;
            //int halfmoduleLength = moduleLength / 2;

            for (int i = 0; i < moduleLength; i++)
            {
                for (int j = 0; j < moduleLength; j++)
                {
                    SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                    double weight = 0.2;
                    int R = Convert.ToInt32(SourceImageColor.R * weight);
                    int G = Convert.ToInt32(SourceImageColor.G * weight);
                    int B = Convert.ToInt32(SourceImageColor.B * weight);

                    result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(R, G, B));
                }
            }
        }

        private void waterMarkWhite(Bitmap result, Bitmap pmBitmap, int x, int y, int moduleLength)
        {
            Color SourceImageColor;
            //int halfmoduleLength = moduleLength / 2;
            for (int i = 0; i < moduleLength; i++)
            {
                for (int j = 0; j < moduleLength; j++)
                {
                    SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                    double weight = 0.2;
                    int R = Convert.ToInt32(255 * (1.0 - weight) + SourceImageColor.R * weight);
                    int G = Convert.ToInt32(255 * (1.0 - weight) + SourceImageColor.G * weight);
                    int B = Convert.ToInt32(255 * (1.0 - weight) + SourceImageColor.B * weight);

                    result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(R, G, B));
                }
            }
        }

        private Bitmap BlackCase(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int CenterSize, int moduleLength)  //黑
        {
            int Around = (moduleLength - CenterSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, CenterSize, moduleLength, Around);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        if (i >= Around && i < (Around + CenterSize) && j >= Around && j < (Around + CenterSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(x * moduleLength + i, y * moduleLength + j);
                            double luminance = 0;

                            if (colorSpace == "HSL")
                            {
                                ACS.HSL = CSC.RGB2HSL(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.HSL.L;
                            }
                            else if (colorSpace == "HSV")
                            {
                                ACS.HSV = CSC.RGB2HSV(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.HSV.V;
                            }
                            else if (colorSpace == "Lab")
                            {
                                ACS.Lab = CSC.RGB2Lab(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.Lab.L / 100;
                            }
                            else
                            {
                                ACS.YUV = CSC.RGB2YUV(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.YUV.Y / 255.0;
                            }

                            double T = LocalThresHoldImageColor.R / 255.0;

                            for (int count = 0; count < CenterSize / 2; count++)
                            {
                                if (luminance > (T - UserSetRobustnesspercent) - (Alpha_Magnification * count * UserSetRobustnesspercent))
                                {
                                    if (i >= Around + count && i < (Around + CenterSize) - count && j == Around + count) //上
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i >= Around + count && i < (Around + CenterSize) - count && j == (Around + CenterSize) - count - 1) //下
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i == Around + count && j > Around + count && j < (Around + CenterSize) - count) //左 
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i == (Around + CenterSize) - count - 1 && j > Around + count && j < (Around + CenterSize) - count) //右 
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                }
                                else
                                {
                                    result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
                                }
                            }
                        }//if
                        else
                        {
                            result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
                        }
                    }//for
                }//for
            }
            else
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
                    }
                }
            }
            return result;
        }

        private Bitmap WhiteCase(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int CenterSize, int moduleLength) //白
        {
            int Around = (moduleLength - CenterSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = Convert.ToDouble(robustVal) / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, CenterSize, moduleLength, Around);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            
            if (Lmean - UserSetRobustnesspercent < Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        if (i > Around - 1 && i < (Around + CenterSize) && j > Around - 1 && j < (Around + CenterSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(x * moduleLength + i, y * moduleLength + j);
                            double luminance = 0;

                            if (colorSpace == "HSL")
                            {
                                ACS.HSL = CSC.RGB2HSL(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.HSL.L;
                            }
                            else if (colorSpace == "HSV")
                            {
                                ACS.HSV = CSC.RGB2HSV(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.HSV.V;
                            }
                            else if (colorSpace == "Lab")
                            {
                                ACS.Lab = CSC.RGB2Lab(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.Lab.L / 100;
                            }
                            else
                            {
                                ACS.YUV = CSC.RGB2YUV(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                                luminance = ACS.YUV.Y / 255.0;
                            }

                            double T = LocalThresHoldImageColor.R / 255.0;

                            for (int count = 0; count < CenterSize / 2; count++)
                            {
                                if (luminance < (T + UserSetRobustnesspercent) + (Alpha_Magnification * count * UserSetRobustnesspercent))
                                {
                                    if (i >= Around + count && i < (Around + CenterSize) - count && j == Around + count) //上
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i >= Around + count && i < (Around + CenterSize) - count && j == (Around + CenterSize) - count - 1) //下
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i == Around + count && j > Around + count && j < (Around + CenterSize) - count) //左
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i == (Around + CenterSize) - count - 1 && j > Around + count && j < (Around + CenterSize) - count) //右
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                }
                                else
                                    result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
                            }//for
                        }
                        else
                        {
                            result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
                        }
                    }//for
                }//for
            }
            else
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
                    }
                }
            }
            return result;
        }

        private double calculateScore(Bitmap pmBitmap, int x, int y, int CenterSize, int moduleLength, int Around)
        {
            Color SourceImageColor;
            ColorSpace CSC = new ColorSpace();
            double Sum_Luminance = 0, Lmean;
            int CenterRegion = CenterSize;
            if (CenterRegion < moduleLength / 3)
                CenterRegion = moduleLength / 3;
            //將亮度正規化至0~1
            int halfmoduleLength = 0;

            for (int i = Around; i < Around + CenterRegion; i++)   //計算CENTER方塊分數
            {
                for (int j = Around; j < Around + CenterRegion; j++)
                {
                    SourceImageColor = pmBitmap.GetPixel(x * moduleLength + halfmoduleLength + i, y * moduleLength + halfmoduleLength + j);
                    double Luminance = 0;

                    if (colorSpace == "HSL")
                    {
                        ColorSpace.HSL HSL;
                        HSL = CSC.RGB2HSL(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                        Luminance = HSL.L;
                    }
                    else if (colorSpace == "HSV")
                    {
                        ColorSpace.HSV HSV;
                        HSV = CSC.RGB2HSV(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                        Luminance = HSV.V;
                    }
                    else if (colorSpace == "Lab")
                    {
                        ColorSpace.Lab Lab;
                        Lab = CSC.RGB2Lab(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                        Luminance = Lab.L / 100;
                    }
                    else
                    {
                        ColorSpace.YUV YUV;
                        YUV = CSC.RGB2YUV(SourceImageColor.R, SourceImageColor.G, SourceImageColor.B);
                        Luminance = YUV.Y / 255.0;
                    }
                    Sum_Luminance += Luminance;
                }
            }

            Lmean = (Sum_Luminance / (CenterRegion * CenterRegion));

            return Lmean;
        }

        private ColorSpace.AllColorSpace luminance_adjustment(ColorSpace.AllColorSpace ACS, int CenterSize, double luminance2, int Color) //color 1為白 0為黑
        {
            ColorSpace CSC = new ColorSpace();

            if (CenterSize == 2 & Color == 0)
                luminance2 = 0;
            if (CenterSize == 2 & Color == 1)
                luminance2 = 1;

            if (luminance2 < 0)
                luminance2 = 0;
            if (luminance2 > 1)
                luminance2 = 1;

            if (colorSpace == "HSL")
            {
                ACS.RGB = CSC.HSL2RGB(ACS.HSL.H, ACS.HSL.S, luminance2);
            }
            else if (colorSpace == "HSV")
            {
                ACS.RGB = CSC.HSV2RGB(ACS.HSV.H, ACS.HSV.S, luminance2);
            }
            else if (colorSpace == "Lab")
            {
                ACS.RGB = CSC.Lab2RGB(luminance2 * 100, ACS.Lab.a, ACS.Lab.b);
            }

            else
            {
                ACS.RGB = CSC.YUV2RGB(luminance2 * 255.0, ACS.YUV.U, ACS.YUV.V);
            }

            return ACS;
        }
        #endregion

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

        private Bitmap DoProcess(BackgroundWorker worker, BitMatrix QRMat, Bitmap pmBitmap, Bitmap mask, int tileSize, int centerSize, int robustVal)
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
                            waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    #region UpRight
                    else if (pmY < tileSize * 8 && pmX < pmBitmap.Width && pmX >= (pmBitmap.Width - tileSize * 8))
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    #region DownLeft
                    else if (pmX < tileSize * 8 && pmY < pmBitmap.Height && pmY >= (pmBitmap.Height - tileSize * 8))
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    #region AlignmentPattern
                    else if (version > 1 && pmX >= AlignmentPatternLocation_X && pmX < AlignmentPatternLocation_X + tileSize * 5
                    && pmY >= AlignmentPatternLocation_Y && pmY < AlignmentPatternLocation_X + tileSize * 5)
                    {

                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            waterMarkDark(result, pmBitmap, matX, matY, tileSize);
                        }
                        else
                        {
                            waterMarkWhite(result, pmBitmap, matX, matY, tileSize);
                        }
                    }
                    #endregion
                    else
                    {
                        if (QRMat[matX, matY]) //黑為1 白為0
                        {
                            result = BlackCase(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                        }
                        else
                        {
                            result = WhiteCase(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                        }
                        
                    }
                }
            }

       
            return result;
        }
        
        public Bitmap Generate(BackgroundWorker worker, QRCodeInfo info, Bitmap QRBitmap, Bitmap pmBitmap, int? tileSize, int? centerSize, int? robustVal, string colorSpace)
        {
            if (!tileSize.HasValue || !centerSize.HasValue || !robustVal.HasValue) return null;
            this.centerSize = centerSize.Value;
            this.robustVal = robustVal.Value;
            this.colorSpace = colorSpace;
            AlignmentPatternLocation_X = info.AlignmentPatternLocation_X * tileSize.Value;
            AlignmentPatternLocation_Y = info.AlignmentPatternLocation_Y * tileSize.Value;
            version = info.QRVersion;
            
            //int size = pmBitmap.Width - tileSize.Value * 2;
            if (!tileSize.HasValue) return null;
            int size = pmBitmap.Width - tileSize.Value; //ex: pm->22, qr->21

            Bitmap overlapping = ImageProc.OverlappingArea(pmBitmap, size, size, tileSize.Value);
            worker.ReportProgress(10, "Calculate the overlapping area");

            Bitmap grayImage = ImageProc.GrayImage(overlapping, colorSpace);
            worker.ReportProgress(20, "Generate a gray image of overlapping area");

            Bitmap thresholdMask = LocalThresholdMask(grayImage, 5, tileSize.Value);
            worker.ReportProgress(30, "Generate pixel based threshold mask");

            Bitmap prevWork = DoProcess(worker, info.QRmatrix, overlapping, thresholdMask, tileSize.Value, centerSize.Value, robustVal.Value);

            Bitmap resultQR = addQuietZone(prevWork, tileSize.Value);

            grayImage = null;
            overlapping = null;
            prevWork = null;
            
            return resultQR;
        }
    }
}
