using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
    static class Utility
    {
        public static void waterMarkDark(Bitmap result, Bitmap pmBitmap, int x, int y, int moduleLength)
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

        public static void waterMarkWhite(Bitmap result, Bitmap pmBitmap, int x, int y, int moduleLength)
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

        private static int DiamondDis(int pX, int pY, int cX, int cY)
        {
            int x = Math.Abs(pX - cX);
            int y = Math.Abs(pY - cY);
            return x + y;
        }

        public static void BlackDiamond(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int centerSize, int moduleLength, int robustVal, string colorSpace)  //黑
        {
            int Around = (moduleLength - centerSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, centerSize, moduleLength, Around, colorSpace);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            int halfCenterSize = centerSize / 2;
            int centerX = x * moduleLength + Around + (halfCenterSize);
            int centerY = y * moduleLength + Around + (halfCenterSize);
            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        int px = x * moduleLength + i;
                        int py = y * moduleLength + j;
                        SourceImageColor = pmBitmap.GetPixel(px, py);
                        
                        if (i >= Around && i < (Around + centerSize) && j >= Around && j < (Around + centerSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(px, py);
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
                            double d = DiamondDis(px, py, centerX, centerY);
                            

                            //if (d <= halfCenterSize && luminance > T - robustVal)
                            if (d <= halfCenterSize)
                            {
                                double layer = (Math.Abs(halfCenterSize - d)) / 5;
                                double luminance2 = T - UserSetRobustnesspercent - (0.5 * layer * UserSetRobustnesspercent);
                                luminance2 = 0;
                                ACS = luminance_adjustment(ACS, centerSize, luminance2, 0, colorSpace);
                                result.SetPixel(px, py, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                            }
                            else
                            {
                                result.SetPixel(px, py, SourceImageColor);
                            }

                        }//if
                        else
                        {
                            result.SetPixel(px, py, SourceImageColor);
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
                        int px = x * moduleLength + i;
                        int py = y * moduleLength + j;
                        SourceImageColor = pmBitmap.GetPixel(px, py);
                        result.SetPixel(px, py, SourceImageColor);
                    }
                }
            }
        }
                      
        public static void WhiteDiamond(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int centerSize, int moduleLength, int robustVal, string colorSpace)  //黑
        {
            int Around = (moduleLength - centerSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, centerSize, moduleLength, Around, colorSpace);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            int halfCenterSize = centerSize / 2;
            int centerX = x * moduleLength + Around + (halfCenterSize);
            int centerY = y * moduleLength + Around + (halfCenterSize);
            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        if (i >= Around && i < (Around + centerSize) && j >= Around && j < (Around + centerSize)) //center
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
                            double d = DiamondDis(x * moduleLength + i, y * moduleLength + j, centerX, centerY);
                            
                            //if (d <= halfCenterSize && luminance < T + UserSetRobustnesspercent)
                            if (d <= halfCenterSize)
                            {
                                double layer = (Math.Abs(halfCenterSize - d)) / 5;
                                double luminance2 = T + UserSetRobustnesspercent + (0.5 * layer * UserSetRobustnesspercent);
                                ACS = luminance_adjustment(ACS, centerSize, luminance2, 1, colorSpace);
                                result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                            }
                            else
                            {
                                result.SetPixel(x * moduleLength + i, y * moduleLength + j, SourceImageColor);
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
            //return result;
        }
        
        private static double Radius(int pX, int pY, int cX, int cY)
        {
            double x = Math.Pow(Convert.ToDouble((pX - cX)), 2.0);
            double y = Math.Pow(Convert.ToDouble((pY - cY)), 2.0);

            return Math.Sqrt(x + y);
        }     

        public static void BlackCircle(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int centerSize, int moduleLength, int robustVal, string colorSpace)  //黑
        {
            int Around = (moduleLength - centerSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, centerSize, moduleLength, Around, colorSpace);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            int radius = centerSize / 2;
            int centerX = x * moduleLength + Around + (radius);
            int centerY = y * moduleLength + Around + (radius);
            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        int px = x * moduleLength + i;
                        int py = y * moduleLength + j;
                        SourceImageColor = pmBitmap.GetPixel(px, py);
                        if (i >= Around && i <= (Around + centerSize) && j >= Around && j <= (Around + centerSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(px, py);
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
                            double d = Radius(px, py, centerX, centerY);
                            
                            //if (d <= radius && luminance > T-UserSetRobustnesspercent )
                            if (d <= radius)
                            {
                                double layer = (Math.Abs(radius - d)) / 5;
                                double luminance2 = T - UserSetRobustnesspercent - ( 0.5 * layer * UserSetRobustnesspercent);
                                ACS = luminance_adjustment(ACS, centerSize, luminance2, 0, colorSpace);
                                result.SetPixel(px, py, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                            }
                            else
                            {
                                result.SetPixel(px, py, SourceImageColor);
                            }

                        }//if
                        else
                        {
                            result.SetPixel(px, py, SourceImageColor);
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
                        int px = x * moduleLength + i;
                        int py = y * moduleLength + j;
                        SourceImageColor = pmBitmap.GetPixel(px, py);
                        result.SetPixel(px, py, SourceImageColor);
                    }
                }
            }
            //return result;
        }
                      
        public static void WhiteCircle(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int centerSize, int moduleLength, int robustVal, string colorSpace)  //黑
        {
            int Around = (moduleLength - centerSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, centerSize, moduleLength, Around, colorSpace);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            int radius = centerSize / 2;
            int centerX = x * moduleLength + Around + radius;
            int centerY = y * moduleLength + Around + radius;
            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        int px = x * moduleLength + i;
                        int py = y * moduleLength + j;
                        SourceImageColor = pmBitmap.GetPixel(px, py);
                        if (i >= Around && i < (Around + centerSize) && j >= Around && j < (Around + centerSize)) //center
                        {
                            LocalThresHoldImageColor = mask.GetPixel(px, py);
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
                            double d = Radius(px, py, centerX, centerY);
                            //if (d <= radius && luminance < T + UserSetRobustnesspercent)
                            if (d <= radius)
                            {
                                double layer = (Math.Abs(radius - d)) / 5;
                                double luminance2 = T + UserSetRobustnesspercent + (0.5 * layer * UserSetRobustnesspercent);
                                ACS = luminance_adjustment(ACS, centerSize, luminance2, 1, colorSpace);
                                result.SetPixel(px, py, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
                            }
                            else
                            {
                                result.SetPixel(px, py, SourceImageColor);
                            }

                        }//if
                        else
                        {
                            result.SetPixel(px, py, SourceImageColor);
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
                        int px = x * moduleLength + i;
                        int py = y * moduleLength + j;
                        SourceImageColor = pmBitmap.GetPixel(px, py);
                        result.SetPixel(px, py, SourceImageColor);
                    }
                }
            }
        }
                      
        public static void BlackSquare(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int centerSize, int moduleLength, int robustVal, string colorSpace)  //黑
        {
            int Around = (moduleLength - centerSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = robustVal / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, centerSize, moduleLength, Around, colorSpace);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;
            if (Lmean + UserSetRobustnesspercent > Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        if (i >= Around && i < (Around + centerSize) && j >= Around && j < (Around + centerSize)) //center
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

                            double luminance2 = 0;
                            ACS = luminance_adjustment(ACS, centerSize, luminance2, 0, colorSpace);
                            result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
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
            //return result;
        }
                      
        public static void WhiteSquare(Bitmap result, Bitmap pmBitmap, Bitmap mask, int x, int y, int centerSize, int moduleLength, int robustVal, string colorSpace) //白
        {
            int Around = (moduleLength - centerSize) / 2;
            Color SourceImageColor, LocalThresHoldImageColor;
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double UserSetRobustnesspercent = Convert.ToDouble(robustVal) / 255.0;
            double Lmean = calculateScore(pmBitmap, x, y, centerSize, moduleLength, Around, colorSpace);
            double Tmean = mask.GetPixel(x * moduleLength + Around, y * moduleLength + Around).R / 255.0;

            if (Lmean - UserSetRobustnesspercent < Tmean)
            {
                for (int i = 0; i < moduleLength; i++)
                {
                    for (int j = 0; j < moduleLength; j++)
                    {
                        SourceImageColor = pmBitmap.GetPixel(x * moduleLength + i, y * moduleLength + j);
                        if (i > Around - 1 && i < (Around + centerSize) && j > Around - 1 && j < (Around + centerSize)) //center
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
                            /*
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
                            }//for*/
                            double luminance2 = 1;
                            ACS = luminance_adjustment(ACS, centerSize, luminance2, 1, colorSpace);
                            result.SetPixel(x * moduleLength + i, y * moduleLength + j, Color.FromArgb(ACS.RGB.R, ACS.RGB.G, ACS.RGB.B));
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
            //return result;
        }

        private static double calculateScore(Bitmap pmBitmap, int x, int y, int centerSize, int moduleLength, int Around, string colorSpace)
        {
            Color SourceImageColor;
            ColorSpace CSC = new ColorSpace();
            double Sum_Luminance = 0, Lmean;
            int CenterRegion = centerSize;
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

        private static ColorSpace.AllColorSpace luminance_adjustment(ColorSpace.AllColorSpace ACS, int centerSize, double luminance2, int Color, string colorSpace) //color 1為白 0為黑
        {
            ColorSpace CSC = new ColorSpace();

            if (centerSize == 2 & Color == 0)
                luminance2 = 0;
            if (centerSize == 2 & Color == 1)
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
    }
}
