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
        private int? moduleSize;
        private int AlignmentPatternLocation_X;
        private int AlignmentPatternLocation_Y;
        private double Alpha_Magnification = 0.5f;
        private int version;
        private string colorSpace;
        private int centerSize;
        private double robustVal;


        public void Reset()
        {
            /*photomosaicImg = null;
            infoOfQRCode = null;
            QRCode = null;
            ColorSpace = null;
            tileSize = null;
            centerSize = null;
            robustVal = null;*/
        }

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

        public Bitmap BinaryImgThresholdMask(Bitmap I)
        {
            return ImageProc.PixelBasedBinarization(I, "Lab"); ;
        }

        private void waterMarkDark(Bitmap result, Bitmap pmBitmap, int x, int y, int moduleLength)
        {
            Color SourceImageColor;

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

        private Bitmap case1(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int CenterSize, int tileSize)  //黑
        {
            int Around = (tileSize - CenterSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, CenterSize, tileSize, Around);
            double Tmean = mask.GetPixel(x * tileSize + Around, y * tileSize + Around).R / 255.0;

            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < tileSize; i++)
                {
                    for (int j = 0; j < tileSize; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * tileSize + i, y * tileSize + j);
                        if (i >= Around && i < (Around + CenterSize) && j >= Around && j < (Around + CenterSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(x * tileSize + i, y * tileSize + j);
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
                            //double numOfCount = 0;

                            //if ((CenterSize / 2) - 1 > 0)
                            //    numOfCount = (T - UserSetRobustnesspercent) / ((CenterSize / 2.0) - 1);
                            //else
                            //    numOfCount = T - UserSetRobustnesspercent;

                            for (int count = 0; count < CenterSize / 2; count++)
                            {
                                if (luminance > (T - UserSetRobustnesspercent) - (Alpha_Magnification * count * UserSetRobustnesspercent))
                                {
                                    if (i >= Around + count && i < (Around + CenterSize) - count && j == Around + count) //上
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i >= Around + count && i < (Around + CenterSize) - count && j == (Around + CenterSize) - count - 1) //下
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i == Around + count && j > Around + count && j < (Around + CenterSize) - count) //左 
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                    else if (i == (Around + CenterSize) - count - 1 && j > Around + count && j < (Around + CenterSize) - count) //右 
                                    {
                                        double luminance2 = T - UserSetRobustnesspercent - (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 0);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        break;
                                    }
                                }
                                else
                                {
                                    result.SetPixel(x * tileSize + i, y * tileSize + j, SourceImageColor);
                                }
                            }
                        }//if
                        else
                        {
                            result.SetPixel(x * tileSize + i, y * tileSize + j, SourceImageColor);
                        }
                    }//for
                }//for
            }
            else
            {
                for (int i = 0; i < tileSize; i++)
                {
                    for (int j = 0; j < tileSize; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * tileSize + i, y * tileSize + j);
                        result.SetPixel(x * tileSize + i, y * tileSize + j, SourceImageColor);
                    }
                }

            }
            return result;
        }

        private Bitmap case2(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int CenterSize, int tileSize) //白
        {
            int Around = (tileSize - CenterSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = Convert.ToDouble(robustVal) / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, CenterSize, tileSize, Around);
            double Tmean = mask.GetPixel(x * tileSize + Around, y * tileSize + Around).R / 255.0;

            if (Lmean - UserSetRobustnesspercent < Tmean)
            {
                for (int i = 0; i < tileSize; i++)
                {
                    for (int j = 0; j < tileSize; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * tileSize + i, y * tileSize + j);
                        if (i > Around - 1 && i < (Around + CenterSize) && j > Around - 1 && j < (Around + CenterSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(x * tileSize + i, y * tileSize + j);
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
                            //double numOfCount = 0;

                            //if ((CenterSize / 2) - 1 > 0)
                            //    numOfCount = (1 - (T + UserSetRobustnesspercent)) / ((CenterSize / 2.0) - 1);
                            //else
                            //    numOfCount = 1 - (T + UserSetRobustnesspercent);

                            for (int count = 0; count < CenterSize / 2; count++)
                            {
                                if (luminance < (T + UserSetRobustnesspercent) + (Alpha_Magnification * count * UserSetRobustnesspercent))
                                {
                                    if (i >= Around + count && i < (Around + CenterSize) - count && j == Around + count) //上
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        //VisualQRcodeBmap.SetPixel(x * ScaleModuleSize + i, y * ScaleModuleSize + j, Color.Red);
                                        break;
                                    }
                                    else if (i >= Around + count && i < (Around + CenterSize) - count && j == (Around + CenterSize) - count - 1) //下
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        //VisualQRcodeBmap.SetPixel(x * ScaleModuleSize + i, y * ScaleModuleSize + j, Color.Red);
                                        break;
                                    }
                                    else if (i == Around + count && j > Around + count && j < (Around + CenterSize) - count) //左
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        //VisualQRcodeBmap.SetPixel(x * ScaleModuleSize + i, y * ScaleModuleSize + j, Color.Red);
                                        break;
                                    }
                                    else if (i == (Around + CenterSize) - count - 1 && j > Around + count && j < (Around + CenterSize) - count) //右
                                    {
                                        double luminance2 = T + UserSetRobustnesspercent + (Alpha_Magnification * count * UserSetRobustnesspercent);
                                        ACS = luminance_adjustment(ACS, CenterSize, luminance2, 1);
                                        result.SetPixel(x * tileSize + i, y * tileSize + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                                        //VisualQRcodeBmap.SetPixel(x * ScaleModuleSize + i, y * ScaleModuleSize + j, Color.Red);
                                        break;
                                    }
                                }
                                else
                                    result.SetPixel(x * tileSize + i, y * tileSize + j, SourceImageColor);
                            }//for
                        }
                        else
                        {
                            result.SetPixel(x * tileSize + i, y * tileSize + j, SourceImageColor);
                        }
                    }//for
                }//for
            }
            else
            {
                for (int i = 0; i < tileSize; i++)
                {
                    for (int j = 0; j < tileSize; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * tileSize + i, y * tileSize + j);
                        result.SetPixel(x * tileSize + i, y * tileSize + j, SourceImageColor);
                    }
                }
            }
            return result;
        }

        private double calculateScore(Bitmap OImage, int x, int y, int CenterSize, int ScaleModuleSize, int Around)
        {
            Color SourceImageColor;
            ColorSpace CSC = new ColorSpace();
            double Sum_Luminance = 0, Lmean;
            int CenterRegion = CenterSize;
            if (CenterRegion < ScaleModuleSize / 3)
                CenterRegion = ScaleModuleSize / 3;
            //將亮度正規化至0~1

            for (int i = Around; i < Around + CenterRegion; i++)   //計算CENTER方塊分數
            {
                for (int j = Around; j < Around + CenterRegion; j++)
                {
                    SourceImageColor = OImage.GetPixel(x * ScaleModuleSize + i, y * ScaleModuleSize + j);
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

        private Bitmap DoProcess(BitMatrix QRMat, Bitmap pmBitmap, Bitmap mask, int tileSize, int centerSize, int robustVal)
        {
            Bitmap result = new Bitmap(pmBitmap.Width, pmBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //tileSize==moduleSize
            int numModule = version * 4 + 17;
            int qrStartX = tileSize;
            int qrStartY = tileSize;
            int qrEndX = pmBitmap.Width - tileSize - 1;
            int qrEndY = pmBitmap.Height - tileSize - 1;
            for (int pmY = 0; pmY < pmBitmap.Height; pmY += tileSize)
            {
                for (int pmX = 0; pmX < pmBitmap.Width; pmX += tileSize)
                {
                    if(pmY<qrStartY || pmY>qrEndY || pmX<qrStartX || pmX>qrEndX)
                    {
                        for(int y = pmY; y < pmY+tileSize; ++y)
                        {
                            for(int x = pmX; x < pmX+tileSize;++x)
                            {
                                result.SetPixel(x, y, pmBitmap.GetPixel(x, y));
                            }
                        }
                    }
                    else
                    {
                        int qrX = pmX - qrStartX;
                        int qrY = pmY - qrStartY;
                        int matX = (qrX / 64) % numModule;
                        int matY = (qrY / 64) % numModule;
                        #region UpLeft
                        if(qrX < tileSize * 8 && qrY < tileSize*8)
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
                        else if(qrY < tileSize * 8 && qrX <= qrEndX && qrX >= (qrEndX - tileSize * 8))
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
                        else if (qrX < tileSize * 8 && qrY < qrEndY && qrY >= (qrEndY - tileSize * 8))
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
                        else if (version > 1 && qrX >= AlignmentPatternLocation_X && qrX < AlignmentPatternLocation_X + tileSize * 5
                     && qrY >= AlignmentPatternLocation_Y && qrY < AlignmentPatternLocation_X + tileSize * 5)
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
                                //drawModuleBlack(ScaleQRcodeBitmap, x, y, ModuleLength);
                                //drawModuleCenterDark(ScaleQRcodeBitmap, x, y, ModuleLength, CenterSize);
                                result = case1(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                //VisualQRcodeBmap = Black(VisualQRcodeBmap, OImage, x, y, ModuleLength, CenterSize, ScaleModuleSize);
                            }
                            else
                            {
                                //drawModuleWhite(ScaleQRcodeBitmap, x, y, ModuleLength);
                                //drawModuleCenterLight(ScaleQRcodeBitmap, x, y, ModuleLength, CenterSize);
                                result = case2(result, pmBitmap, mask, matX, matY, centerSize, tileSize);
                                //VisualQRcodeBmap = White(VisualQRcodeBmap, OImage, x, y, ModuleLength, CenterSize, ScaleModuleSize);
                            }
                        }
                    }
                }
            }
            return result;
        }
        
        public Bitmap Generate(QRCodeInfo info, Bitmap QRBitmap, Bitmap pmBitmap, int? tileSize, int? centerSize, int? robustVal, string colorSpace)
        {
            this.centerSize = centerSize.Value;
            this.robustVal = robustVal.Value;
            this.colorSpace = colorSpace;
            AlignmentPatternLocation_X = info.AlignmentPatternLocation_X * tileSize.Value;
            AlignmentPatternLocation_Y = info.AlignmentPatternLocation_Y * tileSize.Value;
            version = info.QRVersion;
            Bitmap mask = BinaryImgThresholdMask(pmBitmap);
            return DoProcess(info.QRmatrix, pmBitmap, mask, tileSize.Value, centerSize.Value, robustVal.Value);
        }
    }
}
