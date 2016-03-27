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

        public static Bitmap GrayImage(Bitmap I, string colorSpace)
        {
            Bitmap GrayImage = new Bitmap(I.Width, I.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double luminance = 0;
            //將亮度正規化至0~255
            for (int y = 0; y < I.Height; y++)
            {
                for (int x = 0; x < I.Width; x++)
                {
                    if (colorSpace == "HSL")
                    {
                        ACS.HSL = CSC.RGB2HSL(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.HSL.L * 255;
                    }
                    else if (colorSpace == "HSV")
                    {
                        ACS.HSV = CSC.RGB2HSV(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.HSV.V * 255;
                    }
                    else if (colorSpace == "Lab")
                    {
                        ACS.Lab = CSC.RGB2Lab(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.Lab.L * (255.0 / 100.0);
                    }
                    else
                    {
                        ACS.YUV = CSC.RGB2YUV(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.YUV.Y;
                    }

                    GrayImage.SetPixel(x, y, Color.FromArgb(Convert.ToInt32(luminance), Convert.ToInt32(luminance), Convert.ToInt32(luminance)));
                }
            }
            //Histogram = histogramdata(GrayImage);
            return GrayImage;
        }

        public static Bitmap PixelBasedBinarization(Bitmap I, string colorSpace)
        {
            Bitmap ThresholdImage = new Bitmap(I.Width, I.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ColorSpace CSC = new ColorSpace();
            ColorSpace.AllColorSpace ACS = new ColorSpace.AllColorSpace();
            double luminance = 0;
            for (int y = 0; y < I.Height; ++y)
            {
                for (int x = 0; x < I.Width; ++x)
                {
                    if (colorSpace == "HSL")
                    {
                        ACS.HSL = CSC.RGB2HSL(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.HSL.L * 255;
                    }
                    else if (colorSpace == "HSV")
                    {
                        ACS.HSV = CSC.RGB2HSV(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.HSV.V * 255;
                    }
                    else if (colorSpace == "Lab")
                    {
                        ACS.Lab = CSC.RGB2Lab(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.Lab.L * (255.0 / 100.0);
                        //if (luminance > 128.0f) luminance = 255.0f;
                        //else luminance = 0.0f;
                    }
                    else
                    {
                        ACS.YUV = CSC.RGB2YUV(I.GetPixel(x, y).R, I.GetPixel(x, y).G, I.GetPixel(x, y).B);
                        luminance = ACS.YUV.Y;
                    }

                    ThresholdImage.SetPixel(x, y, Color.FromArgb(Convert.ToInt32(luminance), Convert.ToInt32(luminance), Convert.ToInt32(luminance)));
                }
            }
            return ThresholdImage;
        }
    }
}
