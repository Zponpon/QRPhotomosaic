using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Flann;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.GPU;

namespace QRPhotoMosaic.Method
{
    public static class FLANN
    {
        public static string avgtxt = "AvgColor.txt";
        public static Matrix<float> features = null;
        public static Matrix<float> features4x4 = null;
        public static Index kdtree = null;
        public static Matrix<int> Indices = null;
        public static Matrix<float> Query = null;
        public static Matrix<float> Query4x4 = null;
        public static Matrix<int> Indices4x4 = null;

        public static void Search(Bitmap src, int version, int k)
        {
            int blockSize = MainForm.singleton.BlockSize;
            int v = (version * 4 + 17) + 1;
            int dstSize = v * v;
            float blockTotal = blockSize * blockSize;
            float R = 0, G = 0, B = 0;

            Matrix<int> indices = new Matrix<int>(dstSize, k);
            Matrix<float> dist = new Matrix<float>(dstSize, k);
            Matrix<float> query = new Matrix<float>(dstSize, 3);
            Bitmap newSrc = ImageProc.ScaleImage(src, v * blockSize);
            int index = 0;
            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                for (int x = 0; x < newSrc.Height; x += blockSize)
                {
                    for (int i = y; i < y + blockSize; ++i)
                    {
                        for (int j = x; j < x + blockSize; ++j)
                        {
                            R += Convert.ToSingle(newSrc.GetPixel(j, i).R);
                            G += Convert.ToSingle(newSrc.GetPixel(j, i).G);
                            B += Convert.ToSingle(newSrc.GetPixel(j, i).B);
                        }
                    }
                    R /= blockTotal;
                    G /= blockTotal;
                    B /= blockTotal;
                    query.Data[index, 0] = R;
                    query.Data[index, 1] = G;
                    query.Data[index, 2] = B;
                    index++;
                    R = G = B = 0;
                }
            }
            kdtree.KnnSearch(query, indices, dist, k, 64);
            FLANN.Query = query;
            FLANN.Indices = indices;
            GC.Collect();
        }

        public static void Search4x4(Bitmap src, int version, int k)
        {
            int blockSize = MainForm.singleton.BlockSize;
            int v = (version * 4 + 17) + 1;
            int dstSize = v * v;
            float blockTotal = blockSize * blockSize;
            int R = 0, G = 0, B = 0;

            Matrix<int> indices = new Matrix<int>(dstSize, k);
            Matrix<float> dist = new Matrix<float>(dstSize, k);
            Matrix<float> query = new Matrix<float>(dstSize, 48);
            Bitmap newSrc = ImageProc.ScaleImage(src, v * blockSize);
            int quater = blockSize / 4;
            int dq = quater * quater;
            int index = 0;
            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                for (int x = 0; x < newSrc.Height; x += blockSize)
                {
                    int blockIdx = 0;
                    
                    for (int i = y; i < y + blockSize; i += quater)
                    {
                        for (int j = x; j < x + blockSize; j += quater)
                        {
                            for (int m = 0; m < quater; m++)
                            {
                                for (int n = 0; n < quater; n++)
                                {
                                    Color pixel = newSrc.GetPixel(j + n, i + m);
                                    R += Convert.ToInt32(pixel.R);
                                    G += Convert.ToInt32(pixel.G);
                                    B += Convert.ToInt32(pixel.B);
                                }
                            }

                            R /= dq;
                            G /= dq;
                            B /= dq;
                            query.Data[index, blockIdx++] = (float)R;
                            query.Data[index, blockIdx++] = (float)G;
                            query.Data[index, blockIdx++] = (float)B;
                            R = G = B = 0;
                        }
                    }
                    index++;
                }
            }
            kdtree.KnnSearch(query, indices, dist, k, 64);
            FLANN.Query4x4 = query;
            FLANN.Indices4x4 = indices;
            GC.Collect();
        }

        public static void Search4x4Lab(Bitmap src, int version, int k)
        {
            int blockSize = MainForm.singleton.BlockSize;
            int v = (version * 4 + 17) + 1;
            int dstSize = v * v;
            float blockTotal = blockSize * blockSize;
            int R = 0, G = 0, B = 0;
            ColorSpace cs = new ColorSpace();

            Matrix<int> indices = new Matrix<int>(dstSize, k);
            Matrix<float> dist = new Matrix<float>(dstSize, k);
            Matrix<float> query = new Matrix<float>(dstSize, 48);
            Bitmap newSrc = ImageProc.ScaleImage(src, v * blockSize);
            int quater = blockSize / 4;
            int dq = quater * quater;
            int index = 0;
            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                for (int x = 0; x < newSrc.Height; x += blockSize)
                {
                    int blockIdx = 0;

                    for (int i = y; i < y + blockSize; i += quater)
                    {
                        for (int j = x; j < x + blockSize; j += quater)
                        {
                            for (int m = 0; m < quater; m++)
                            {
                                for (int n = 0; n < quater; n++)
                                {
                                    Color pixel = newSrc.GetPixel(j + n, i + m);
                                    R += Convert.ToInt32(pixel.R);
                                    G += Convert.ToInt32(pixel.G);
                                    B += Convert.ToInt32(pixel.B);
                                }
                            }

                            R /= dq;
                            G /= dq;
                            B /= dq;
                            ColorSpace.Lab lab = cs.RGB2Lab(R, G, B);
                            query.Data[index, blockIdx++] = (float)lab.L;
                            query.Data[index, blockIdx++] = (float)lab.a;
                            query.Data[index, blockIdx++] = (float)lab.b;
                            R = G = B = 0;
                        }
                    }
                    index++;
                }
            }
            kdtree.KnnSearch(query, indices, dist, k, 64);
            FLANN.Query4x4 = query;
            FLANN.Indices4x4 = indices;
            GC.Collect();
        }
    }
}
