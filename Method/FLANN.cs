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
        //public static int noramlK = 1;
        public static int hammingK = 1;
        public static int duplicate = 0;
        public static Matrix<float> features = null;
        public static Matrix<float> features4x4 = null;
        public static Index kdtree = null;
        public static Matrix<int> Indices = null;
        public static Matrix<float> Query = null;
        public static Matrix<float> Query4x4 = null;
        public static Matrix<int> Indices4x4 = null;
        public static int testIdx = 0;
        public static List<string> test = new List<string>();
        public static string hammingcheck = string.Empty;
        public static int hammingcount = 0;

        public static Matrix<float>[] Newfeatures4x4 = new Matrix<float>[16];
        public static Matrix<float> FunctionFeatures4x4 = null;
        public static Matrix<float>[] Distances = new Matrix<float>[4];
        public static Matrix<int>[] HammingIndices = new Matrix<int>[4];
        public static Index kdtree_Patterns = null;
        public static Index[] kdtrees = new Index[16];
        public static Dictionary<int, List<string>> usedDict = new Dictionary<int, List<string>>();
        public static Dictionary<int, List<int>> folderDict = new Dictionary<int, List<int>>();
        public static List<string> functionList = new List<string>();

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
            double L = 0, a = 0, b = 0;
            double ref_a = 500.0f * 25.0f / 29.0f;
            double ref_b = 200.0f * 25.0f / 29.0f;
            ColorSpace cs = new ColorSpace();

            Matrix<int> indices = new Matrix<int>(dstSize, k);
            Matrix<float> dist = new Matrix<float>(dstSize, k);
            Matrix<float> query = new Matrix<float>(dstSize, 48);
            Bitmap newSrc = ImageProc.ScaleImage(src, v * blockSize);
            ColorSpace.Lab lab = new ColorSpace.Lab();
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
                                    lab = cs.RGB2Lab(pixel.R, pixel.G, pixel.B);
                                    L += lab.L / 100.0f;
                                    a += (lab.a + ref_a) / (2 * ref_a);
                                    b += (lab.b + ref_b) / (2 * ref_b);
                                }
                            }
                            L /= dq;
                            a /= dq;
                            b /= dq;
                            query.Data[index, blockIdx++] = (float)L;
                            query.Data[index, blockIdx++] = (float)a;
                            query.Data[index, blockIdx++] = (float)b;
                            //R = G = B = 0;
                            L = a = b = 0;
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

        private static void OneRGBQuery(Bitmap block, Matrix<float> query, int tileSize, int startX = 0, int startY = 0)
        {
            int subIdx = 0;
            int quater = tileSize / 4;
            int dq = quater * quater;
            int endX = startX + tileSize;
            int endY = startY + tileSize;
            int R = 0, G = 0, B = 0;

            for (int y = startY; y < endY; y += quater)
            {
                for (int x = startX; x < endX; x += quater)
                {
                    for (int i = 0; i < quater; ++i)
                    {
                        for (int j = 0; j < quater; ++j)
                        {
                            Color pixel = block.GetPixel(x + j, y + i);
                            R += pixel.R;
                            G += pixel.G;
                            B += pixel.B;
                        }
                    }
                    R /= dq;
                    G /= dq;
                    B /= dq;
                    query.Data[0, subIdx++] = (float)R;
                    query.Data[0, subIdx++] = (float)G;
                    query.Data[0, subIdx++] = (float)B;
                    R = G = B = 0;
                }
            }
        }

        private static void OneLabQuery(Bitmap block, Matrix<float> query, int tileSize, int startX = 0, int startY = 0)
        {
            ColorSpace cs = new ColorSpace();
            ColorSpace.Lab lab;
            float ratio = Convert.ToSingle(MainForm.singleton.RatioNumber.Value);
            int subIdx = 0;
            int quater = tileSize / 4;
            int dq = quater * quater;
            int endX = startX + tileSize;
            int endY = startY + tileSize;
            double L = 0.0f, a = 0.0f, b = 0.0f;
            for (int y = startY; y < endY; y += quater)
            {
                for (int x = startX; x < endX; x += quater)
                {
                    for (int i = 0; i < quater; ++i)
                    {
                        for (int j = 0; j < quater; ++j)
                        {
                            Color pixel = block.GetPixel(x + j, y + i);
                            lab = cs.RGB2Lab(pixel.R, pixel.G, pixel.B);
                            L += (lab.L / 100.0f);
                            a += (lab.a + ColorSpace.ref_a) / (2 * ColorSpace.ref_a);
                            b += (lab.b + ColorSpace.ref_b) / (2 * ColorSpace.ref_b);
                        }
                    }
                    L /= dq;
                    a /= dq;
                    b /= dq;
                    query.Data[0, subIdx++] = (float)L * ratio;
                    query.Data[0, subIdx++] = (float)a;
                    query.Data[0, subIdx++] = (float)b;
                    L = a = b = 0;
                }
            }
        }

        public static string SearchFunctionPatternsRGB(Bitmap block, int tileSize, int k)
        {
            Matrix<int> indices = new Matrix<int>(1, k);
            Matrix<float> dist = new Matrix<float>(1, k);
            Matrix<float> query = new Matrix<float>(1, 48);
            OneRGBQuery(block, query, tileSize);
            kdtree_Patterns.KnnSearch(query, indices, dist, k, 64);
            string[] names = System.IO.Directory.GetFiles(PathConfig.FunctionDarkPath);
            for (int i = 0; i < k; ++i)
            {
                int index = indices.Data[0, i];
                if (!functionList.Contains(names[index]))
                {
                    functionList.Add(names[index]);
                    return names[index];
                }
            }
            return names[indices.Data[0, 0]];
        }

        public static string SearchFunctionPatternsLab(Bitmap block, int tileSize, int k)
        {
            Matrix<int> indices = new Matrix<int>(1, k);
            Matrix<float> dist = new Matrix<float>(1, k);
            Matrix<float> query = new Matrix<float>(1, 48);
            OneLabQuery(block, query, tileSize);
            kdtree_Patterns.KnnSearch(query, indices, dist, k, 64);
            string[] names = System.IO.Directory.GetFiles(PathConfig.FunctionDarkPath);
            for (int i = 0; i < k; ++i)
            {
                int index = indices.Data[0, i];
                if (!functionList.Contains(names[index]))
                {
                    functionList.Add(names[index]);
                    return names[index];
                }
            }
            return names[indices.Data[0, 0]];
        }

        public static string Search4x4LabOther(Bitmap img, int x, int y, int blockSize, string[] names, int kdIndex, int k, bool isFunction, int[] folderbits, bool isHamming = false, float distance = -1.0f, int hammingBit = -1)
        {
            int quater = blockSize / 4;
            int dq = quater * quater;
            double l = 0, a = 0, b = 0;
            if (isHamming && !isFunction)
                k = hammingK;
            Matrix<int> indices = new Matrix<int>(1, k);
            Matrix<float> dist = new Matrix<float>(1, k);
            Matrix<float> query = new Matrix<float>(1, 48);
            ColorSpace cs = new ColorSpace();
            ColorSpace.Lab lab;
            int xb = x * blockSize;
            int yb = y * blockSize;
            int blockIdx = 0;
            float ratio = Convert.ToSingle(MainForm.singleton.RatioNumber.Value);
            //double ref_a = 500.0f * 25.0f / 29.0f;
            //double ref_b = 200.0f * 25.0f / 29.0f;
            OneLabQuery(img, query, blockSize, xb, yb);
            #region Old Version
            /*
            for (int i = yb; i < yb + blockSize; i += quater)
            {
                for (int j = xb; j < xb + blockSize; j += quater)
                {
                    for(int m = 0; m < quater; m++)
                    {
                        for (int n = 0; n < quater; n++)
                        {
                            Color pixel = img.GetPixel(j+n, i+m);
                            lab = cs.RGB2Lab(pixel.R, pixel.G, pixel.B);
                            l += (lab.L / 100.0f);
                            a += (lab.a + ColorSpace.ref_a) / (2 * ColorSpace.ref_a);
                            b += (lab.b + ColorSpace.ref_b) / (2 * ColorSpace.ref_b);
                        }
                    }
                    l /= dq;
                    a /= dq;
                    b /= dq;
                    query.Data[0, blockIdx++] = (float)l * ratio;
                    query.Data[0, blockIdx++] = (float)a;
                    query.Data[0, blockIdx++] = (float)b;
                    l = a = b = 0.0f;
                }
            }
            */
            #endregion
            kdtrees[kdIndex].KnnSearch(query, indices, dist, k, 64);
            if (isHamming && !isFunction)
            {
                Distances[hammingBit] = dist;
                HammingIndices[hammingBit] = indices;
                return null;
            }
            
            #region RestrictTileImage
            if (indices.Cols != 1 && !isFunction)
            {
                for (int i = 0; i < indices.Cols; ++i)
                {
                    int index = indices.Data[0, i];
                    string name = names[index];
                    if (!usedDict.ContainsKey(kdIndex))
                    {
                        usedDict.Add(kdIndex, new List<string>());
                        usedDict[kdIndex].Add(name);
                        return name;
                    }
                    else if (!usedDict[kdIndex].Contains(name))
                    {
                        usedDict[kdIndex].Add(name);
                        return name;
                    }
                }
            }
            #endregion
            #region HammingCode

            string hammingCode = string.Empty;
            if (!isHamming && !isFunction && hammingcheck == "Y")
            {
                hammingCode = HammingCode(img, x, y, blockSize, kdIndex, k, isFunction, folderbits, dist.Data[0, 0], "Lab");
            }

            if (hammingCode != string.Empty)
            {
                hammingcount++;
                return hammingCode;
            }

            #endregion
            if (isHamming)
                return string.Empty;
            if (!isFunction)
                duplicate++;
            return names[indices.Data[0, 0]];
        }
        
        private static string HammingCode(Bitmap img, int x, int y, int blockSize, int kdIndex, int k, bool isFunction, int[] folderbits, float distance, string colorSpace)
        {
            string folder = string.Empty;
            int kdIndex_ = -1;
            int[] folderbits_ = new int[4];
            int[] kdIdxs = new int[4];
            Dictionary<int, string[]> filesDict = new Dictionary<int, string[]>();

            #region HammingSearching
            for (int bit = 0; bit < 4; ++bit)
            {
                if (folderbits[bit] == 1)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        if (i == bit)
                        {
                            folder += "0";
                            folderbits_[i] = 0;
                        }
                        else
                        {
                            folder += folderbits[i].ToString();
                            folderbits_[i] = folderbits[i];
                        }
                    }
                    kdIndex_ = kdIndex - (int)Math.Pow(2, 3 - bit);
                }
                else
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        if (i == bit)
                        {
                            folder += "1";
                            folderbits_[i] = 1;
                        }
                        else
                        {
                            folder += folderbits[i].ToString();
                            folderbits_[i] = folderbits[i];
                        }
                    }
                    kdIndex_ = kdIndex + (int)Math.Pow(2, 3 - bit);
                }
                kdIdxs[bit] = kdIndex_;
                string[] files = System.IO.Directory.GetFiles(MainForm.singleton.CreatingFolderPath + folder);
                if (colorSpace == "RGB")
                    Search4x4RGBOther(img, x, y, blockSize, files, kdIndex_, k, isFunction, folderbits_, true, distance, bit);
                else if (colorSpace == "Lab")
                    Search4x4LabOther(img, x, y, blockSize, files, kdIndex_, k, isFunction, folderbits_, true, distance, bit);
                filesDict.Add(bit, files);
                folder = string.Empty;
            }
            #endregion

            #region CheckUsedDictionary
            float minDist = float.MaxValue;
            int minIdx = 0;
            for (int i = 0; i < Distances[0].Cols; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    if(Distances[j].Data[0, i] < minDist)
                    {
                        minDist = Distances[j].Data[0, i];
                        minIdx = j;
                    }
                }
                int index = HammingIndices[minIdx].Data[0, i];
                string name = filesDict[minIdx][index];
                if(!usedDict.ContainsKey(kdIdxs[minIdx]))
                {
                    usedDict.Add(kdIdxs[minIdx], new List<string>());
                    usedDict[kdIdxs[minIdx]].Add(name);
                    return name;
                }
                else if(!usedDict[kdIdxs[minIdx]].Contains(name))
                {
                    usedDict[kdIdxs[minIdx]].Add(name);
                    return name;
                }
                minDist = float.MaxValue;
            }
            #endregion

            return string.Empty;
        }

        public static string Search4x4RGBOther(Bitmap img, int x, int y, int blockSize, string[] names, int kdIndex, int k, bool isFunction, int[] folderbits, bool isHamming = false, float distance = -1.0f, int hammingBit = -1)
        {
            int quater = blockSize / 4;
            int dq = quater * quater;
            int R = 0, G = 0, B = 0;
            if (isHamming && !isFunction)
                k = hammingK;
            Matrix<int> indices = new Matrix<int>(1, k);
            Matrix<float> dist = new Matrix<float>(1, k);
            Matrix<float> query = new Matrix<float>(1, 48);
            int xb = x * blockSize;
            int yb = y * blockSize;
            int blockIdx = 0;
            OneRGBQuery(img, query, blockSize, xb, yb);
            /*
            for (int i = yb; i < yb + blockSize; i += quater)
            {
                for (int j = xb; j < xb + blockSize; j += quater)
                {
                    for (int m = 0; m < quater; m++)
                    {
                        for (int n = 0; n < quater; n++)
                        {
                            Color pixel = img.GetPixel(j + n, i + m);
                            R += (int)pixel.R;
                            G += (int)pixel.G;
                            B += (int)pixel.B;
                            
                        }
                    }
                    R /= dq;
                    G /= dq;
                    B /= dq;
                    query.Data[0, blockIdx++] = (float)R;
                    query.Data[0, blockIdx++] = (float)G;
                    query.Data[0, blockIdx++] = (float)B;
                    R = G = B = 0;
                }
            }
            */
            kdtrees[kdIndex].KnnSearch(query, indices, dist, k, 64);
            if(isHamming && !isFunction)
            {
                Distances[hammingBit] = dist;
                HammingIndices[hammingBit] = indices;
                return null;
            }

            #region RestrictTileImage
            if (indices.Cols != 1 && !isFunction)
            {
                for (int i = 0; i < indices.Cols; ++i)
                {
                    int index = indices.Data[0, i];
                    string name = names[index];
                    if (!usedDict.ContainsKey(kdIndex))
                    {
                        usedDict.Add(kdIndex, new List<string>());
                        usedDict[kdIndex].Add(name);
                        return name;
                    }
                    else if (!usedDict[kdIndex].Contains(name))
                    {
                        usedDict[kdIndex].Add(name);
                        return name;
                    }
                }
            }
            #endregion
            #region HammingCode
            //string[] hammingCodes = new string[4];
            string hammingCode = string.Empty;
            if(!isHamming && !isFunction && hammingcheck == "Y")
            {
                //int bit = 0;
                //while(hammingCode == string.Empty && bit < 4)
                //{
                //    hammingCode = HammingCode(img, x, y, blockSize, kdIndex, k, isFunction, folderbits, dist.Data[0, 0], "RGB", bit++);
                //}
                hammingCode = HammingCode(img, x, y, blockSize, kdIndex, k, isFunction, folderbits, dist.Data[0, 0], "RGB");
            }

            if (hammingCode != string.Empty)
            {
                hammingcount++;
                return hammingCode;
            }
            
            #endregion
            //FLANN.
            //if(!isFunction && !isHamming)
            //   count++;
            if (isHamming)
                return string.Empty;
            if(!isFunction)
                duplicate++;
            return names[indices.Data[0, 0]];
        }
    }
}
