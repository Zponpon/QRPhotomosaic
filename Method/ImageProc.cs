using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QRPhotoMosaic.Method
{
    class ImageProc
    {
        public static Bitmap ScaleImage(Bitmap image, int size)
        {
            Bitmap ScaleImage = new Bitmap(image, size, size);
            return ScaleImage;
        }

        public static Bitmap ScaleImage(Bitmap image, int width, int height)
        {
            Bitmap ScaleImage = new Bitmap(image, width, height);
            return ScaleImage;
        }

        public static int NormalizeRGB(int val)
        {
            if (val > 255)
                val = 255;
            if (val < 0)
                val = 0;

            return val;
        }

        public static Bitmap OverlappingArea(Bitmap pm, int w, int h, int tileSize)
        {
            Bitmap dst = new Bitmap(w, h);
            int half = tileSize / 2;
            int numTiles = pm.Width / tileSize;

            for (int pmY = 0; pmY < h; pmY += tileSize)
            {
                for (int pmX = 0; pmX < w; pmX += tileSize)
                {
                    for (int y = pmY; y < pmY + tileSize; ++y)
                    {
                        for (int x = pmX; x < pmX + tileSize; ++x)
                        {
                            Color color = pm.GetPixel(x + half, y + half);
                            dst.SetPixel(x, y, color);
                        }
                    }
                }
            }
                return dst;
        }

        public static Bitmap GrayImage(Bitmap src, string colorSpace)
        {
            Bitmap grayImage = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double luminance = 0;
            //將亮度正規化至0~255
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    if (colorSpace == "HSL")
                    {
                        ACS.HSL = CSC.RGB2HSL(src.GetPixel(x, y).R, src.GetPixel(x, y).G, src.GetPixel(x, y).B);
                        luminance = ACS.HSL.L * 255;
                    }
                    else if (colorSpace == "HSV")
                    {
                        ACS.HSV = CSC.RGB2HSV(src.GetPixel(x, y).R, src.GetPixel(x, y).G, src.GetPixel(x, y).B);
                        luminance = ACS.HSV.V * 255;
                    }
                    else if (colorSpace == "Lab")
                    {
                        ACS.Lab = CSC.RGB2Lab(src.GetPixel(x, y).R, src.GetPixel(x, y).G, src.GetPixel(x, y).B);
                        luminance = ACS.Lab.L * (255.0 / 100.0);
                    }
                    else
                    {
                        ACS.YUV = CSC.RGB2YUV(src.GetPixel(x, y).R, src.GetPixel(x, y).G, src.GetPixel(x, y).B);
                        luminance = ACS.YUV.Y;
                    }
                    int L = Convert.ToInt32(luminance);
                    grayImage.SetPixel(x, y, Color.FromArgb(L, L, L));
                }
            }
            return grayImage;
        }

        #region GlobalRegion
        public static int GlobalBinaryThresholdValue(Bitmap GrayImage)
        {
            int threshold = 0;
            Bitmap ThresholdImage = new Bitmap(GrayImage.Width, GrayImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < GrayImage.Height; y++)
            {
                for (int x = 0; x < GrayImage.Width; x++)
                {
                    threshold += Convert.ToInt32(GrayImage.GetPixel(x, y).R);
                }
            }
            threshold = Convert.ToInt32(threshold / (GrayImage.Height * GrayImage.Width));
            return threshold;
        }
        public static Bitmap PixelBasedGlobalThresholdMask(Bitmap grayImage)
        {
            int threshold = GlobalBinaryThresholdValue(grayImage);
            Bitmap ThresholdImage = new Bitmap(grayImage.Width, grayImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    ThresholdImage.SetPixel(x, y, Color.FromArgb(threshold, threshold, threshold));
                }
            }
            return ThresholdImage;
        }
        public static Bitmap PixelBasedGlobalBinarization(Bitmap GrayImage, int threshold)
        {
            Bitmap BinaryImage = new Bitmap(GrayImage.Width, GrayImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < GrayImage.Height; ++y)
            {
                for (int x = 0; x < GrayImage.Width; ++x)
                {
                    int L = Convert.ToInt32(GrayImage.GetPixel(x, y).R);
                    if (L < threshold)
                        BinaryImage.SetPixel(x, y, Color.Black);
                    else
                        BinaryImage.SetPixel(x, y, Color.White);
                }
            }
            return BinaryImage;
        }
        #endregion

        #region LocalRegion
        public static int Module(Bitmap GrayImage, int x, int y, int moduleSize)
        {
            int luminance = 0;
            Color c;
            int count = 0;
            for (int i = 0; i < moduleSize; i++)
            {
                for (int j = 0; j < moduleSize; j++)
                {
                    if (x + j >= GrayImage.Width || y + i >= GrayImage.Height)
                    {
                        count++;
                    }
                    else
                    {
                        c = GrayImage.GetPixel((x + j) % GrayImage.Width, (y + i) % GrayImage.Height);  //避免超出圖片範圍
                        luminance += c.R;
                    }
                }
            }
            return luminance / ((moduleSize * moduleSize) - count);
        }

        private static int Windows(Bitmap grayImage, int x, int y, int windowsSize, int moduleSize)
        {
            int count = 0;
            int L = 0;

            for (int i = -(windowsSize / 2); i < windowsSize - (windowsSize / 2); i++)
            {
                for (int j = -(windowsSize / 2); j < windowsSize - (windowsSize / 2); j++)
                {
                    if (x + (j * moduleSize) >= grayImage.Width || y + (i * moduleSize) >= grayImage.Height || x + (j * moduleSize) < 0 || y + (i * moduleSize) < 0)
                    {
                        count++;
                    }
                    else
                    {
                        L += Module(grayImage, x + (j * moduleSize), y + (i * moduleSize), moduleSize);
                    }
                }
            }
            return L / ((windowsSize * windowsSize) - count);
        }

        private static Bitmap SetThresholdImage(Bitmap thresholdImage, int x, int y, int moduleSize, int threshold)
        {
            for (int i = y; i < y + moduleSize && i < thresholdImage.Height; i++)
            {
                for (int j = x; j < x + moduleSize && j < thresholdImage.Width; j++)
                {
                    thresholdImage.SetPixel(j, i, Color.FromArgb(threshold, threshold, threshold));
                }
            }
            return thresholdImage;
        }

        public static Bitmap PixelBasedLocakThresholdMask(Bitmap grayImage, int windowSize, int moduleSize)
        {
            Bitmap thresholdImage = new Bitmap(grayImage.Width, grayImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < grayImage.Height; y += moduleSize )
            {
                for (int x = 0; x < grayImage.Width; x += moduleSize)
                {
                    int threshold = Windows(grayImage, x, y, windowSize, moduleSize);
                    thresholdImage = SetThresholdImage(thresholdImage, x, y, moduleSize, threshold);
                }
            }
            return thresholdImage;
        }
        #endregion
    }
}
