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
using ZxingForQRcodeHiding.Common;

namespace QRPhotoMosaic.Method
{
    public static class FLANN
    {
        public static string avgtxt = "AvgColor.txt";
        public static string ClassifyPath = "C:\\Users\\Zpon\\Documents\\GitHub\\QRPhotomosaic\\bin\\Classify\\";
        public static Matrix<float> features = null;
        public static Matrix<float> features4x4 = null;
        public static Index kdtree = null;
        public static Matrix<int> Indices = null;
        public static Matrix<float> Query = null;
        public static Matrix<float> Query4x4 = null;
        public static Matrix<int> Indices4x4 = null;

        public static Matrix<float>[] Newfeatures4x4 = new Matrix<float>[16];
        public static Index[] kdtrees = new Index[16];
        private static Dictionary<int, List<string>> usedDict = new Dictionary<int, List<string>>();

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
                                    R += pixel.R;
                                    G += pixel.G;
                                    B += pixel.B;
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

        public static void Search4x4MidLab(Bitmap src, int version, int k, BitMatrix QRMatrix)
        {
            int blockSize = MainForm.singleton.BlockSize;
            int v = (version * 4 + 17);
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
            int bitX = 0, bitY = 0;
            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                for (int x = 0; x < newSrc.Height; x += blockSize)
                {
                    int vecIdx = 0;
                    int subIdx = 0;
                    for (int i = y; i < y + blockSize; i += quater)
                    {
                        for (int j = x; j < x + blockSize; j += quater)
                        {
                            for (int m = 0; m < quater; m++)
                            {
                                for (int n = 0; n < quater; n++)
                                {
                                    if (subIdx != 5 && subIdx != 6 && subIdx != 9 && subIdx != 10)
                                    {
                                        Color pixel = newSrc.GetPixel(j + n, i + m);
                                        R += pixel.R;
                                        G += pixel.G;
                                        B += pixel.B;
                                    }
                                    else
                                    {
                                        Color pixel = newSrc.GetPixel(j + n, i + m);
                                        if (!QRMatrix[bitX, bitY])
                                        {
                                            R += Convert.ToInt32(255 * 0.6 + pixel.R * 0.4);
                                            G += Convert.ToInt32(255 * 0.6 + pixel.G * 0.4);
                                            B += Convert.ToInt32(255 * 0.6 + pixel.B * 0.4);
                                            //R += 255;
                                            //G += 255;
                                            //B += 255;
                                        }
                                        else
                                        {
                                            R += Convert.ToInt32(pixel.R * 0.4);
                                            G += Convert.ToInt32(pixel.G * 0.4);
                                            B += Convert.ToInt32(pixel.B * 0.4);
                                        }
                                    }
                                }
                            }
                            subIdx++;
                            R /= dq;
                            G /= dq;
                            B /= dq;
                            ColorSpace.Lab lab = cs.RGB2Lab(R, G, B);
                            query.Data[index, vecIdx++] = (float)lab.L;
                            query.Data[index, vecIdx++] = (float)lab.a;
                            query.Data[index, vecIdx++] = (float)lab.b;
                            R = G = B = 0;
                        }
                    }
                    index++;
                    bitX++;
                }
                bitX = 0;
                bitY++;
            }
            kdtree.KnnSearch(query, indices, dist, k, 64);
            FLANN.Query4x4 = query;
            FLANN.Indices4x4 = indices;
            GC.Collect();
        }

        public static void buildTrees()
        {
            for(int i = 0; i < 16; ++i)
            {
                kdtrees[i] = new Index(Newfeatures4x4[i], 5);
            }
        }

        public static string Search4x4LabOther(Bitmap img, int x, int y, int blockSize, string[] names, int kdIndex, int k)
        {
            int quater = blockSize / 4;
            int dq = quater * quater;
            int R = 0, G = 0, B = 0;
            Matrix<int> indices = new Matrix<int>(1, k);
            Matrix<float> dist = new Matrix<float>(1, k);
            Matrix<float> query = new Matrix<float>(1, 48);
            ColorSpace cs = new ColorSpace();
            int xb = x * blockSize;
            int yb = y * blockSize;
            int blockIdx = 0;
            for (int i = yb; i < yb + blockSize; i += quater)
            {
                for (int j = xb; j < xb + blockSize; j += quater)
                {
                    
                    for(int m = 0; m < quater; m++)
                    {
                        for (int n = 0; n < quater; n++)
                        {
                            Color pixel = img.GetPixel(j+n, i+m);
                            R += (int)pixel.R;
                            G += (int)pixel.G;
                            B += (int)pixel.B;
                        }
                    }
                    R /= dq;
                    G /= dq;
                    B /= dq;
                    ColorSpace.Lab lab = cs.RGB2Lab(R, G, B);
                    query.Data[0, blockIdx++] = (float)lab.L;
                    query.Data[0, blockIdx++] = (float)lab.a;
                    query.Data[0, blockIdx++] = (float)lab.b;
                    R = G = B = 0;
                }
            }
            kdtrees[kdIndex].KnnSearch(query, indices, dist, k, 64);//First time k = 1;
            
            for (int i = 0; i < indices.Cols; ++i)
            {
                int index = indices.Data[0, i];
                string name = names[index];
                if(!usedDict.ContainsKey(kdIndex))
                {
                    usedDict.Add(kdIndex, new List<string>());
                    usedDict[kdIndex].Add(name);
                    return name;
                }
                else if (!usedDict[kdIndex].Contains(name))
                {
                    usedDict[kdIndex].Add(names[index]);
                    return name;
                }
            }
            
            return names[indices.Data[0, 0]];
        }
    }
}
