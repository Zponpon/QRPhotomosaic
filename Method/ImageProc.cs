﻿using System;
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
    }
}
