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
        public static Index kdtree = null;
        public static Matrix<int> Indices = null;
        public static Matrix<float> Query = null;

        public static void Search(Bitmap src, int version)
        {
            int blockSize = MainForm.singleton.BlockSize;
            int v = (version * 4 + 17) + 1;
            int dstSize = v * v;
            float blockTotal = blockSize * blockSize;
            float R = 0, G = 0, B = 0;

            Matrix<int> indices = new Matrix<int>(dstSize, 1);
            Matrix<float> dist = new Matrix<float>(dstSize, 1);
            Matrix<float> query = new Matrix<float>(dstSize, 3);
            Bitmap newSrc = ImageProc.ScaleImage(src, v * blockSize);
            int index = 0;
            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                for (int x = 0; x < newSrc.Height; x += blockSize)
                {
                    int currX = 0, currY = 0;
                    Bitmap currBlock = new Bitmap(blockSize, blockSize);
                    for (int i = y; i < y + blockSize; ++i)
                    {
                        for (int j = x; j < x + blockSize; ++j)
                        {
                            R += Convert.ToSingle(newSrc.GetPixel(j, i).R);
                            G += Convert.ToSingle(newSrc.GetPixel(j, i).G);
                            B += Convert.ToSingle(newSrc.GetPixel(j, i).B);
                            currBlock.SetPixel(currX++, currY, newSrc.GetPixel(j, i));
                        }
                        currY++;
                        currX = 0;
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
            kdtree.KnnSearch(query, indices, dist, 1, 64);
            FLANN.Query = query;
            FLANN.Indices = indices;
        }
    }
}
