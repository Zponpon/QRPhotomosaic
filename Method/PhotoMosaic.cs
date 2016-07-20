using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Flann;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.GPU;


using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Client.Result;
using ZxingForQRcodeHiding.QrCode.Internal;
using ZxingForQRcodeHiding.QrCode;

namespace QRPhotoMosaic.Method
{
    class PhotoMosaic
    {
       // public static MainForm main;

        public PhotoMosaic()
        {
            
        }

        public struct TileSize
        {
            public string Size { get; set; }
            public int Value{ get; set; }
        }
        public static List<TileSize> tileList;

        public static void Init()
        {
            tileList = new List<TileSize>()
            {
                new TileSize
                {
                    Size = "64",
                    Value = 64,
                },
                new TileSize
                {
                    Size = "128",
                    Value = 128,
                },
            };
        }

        private Bitmap AddFunctionPatterns(BitMatrix matrix, Bitmap qr)
        {

            return null;
        }

        public Bitmap GenerateByFlannCombine(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k, string space, BitMatrix QRMatrix)
        {
            BitMatrix matrix = new BitMatrix(QRMatrix.Width + 2, QRMatrix.Height + 2);
            int blockSize = MainForm.singleton.BlockSize;
            int v = (version * 4 + 17) + 1;
            int dstSize = v * tileSize;
            Bitmap newSrc = ImageProc.ScaleImage(src, v * MainForm.singleton.BlockSize);
            Bitmap dst = new Bitmap(dstSize, dstSize);
            Console.Write("\n");
            for (int y = 0; y < matrix.Height; ++y)
            {
                for (int x = 0; x < matrix.Width; ++x)
                {
                    if (x != 0 && x != matrix.Width - 1 && y != 0 && y != matrix.Height - 1)
                    {
                        matrix[x, y] = QRMatrix[x-1, y-1];
                        //Console.Write("1");
                    }
                    else
                    {
                        matrix[x, y] = false;
                        //Console.Write("0");
                    }
                }
                //Console.Write("\n");
            }
            worker.ReportProgress(20);
            int []folderbits = {0,0,0,0};
            int total = matrix.Height * matrix.Width;
            for (int y = 0; y < matrix.Height - 1; ++y)
            {
                worker.ReportProgress(y * 50 / matrix.Height + 20);
                for (int x = 0; x < matrix.Width - 1; ++x)
                {
                    int kdIndex = 0;
                    if (!matrix[x, y])
                    {
                        // folderbit from left to right
                        folderbits[0] = 1;
                        kdIndex += 8;
                    }

                    if (!matrix[x + 1, y])
                    {
                        folderbits[1] = 1;
                        kdIndex += 4;
                    }

                    if (!matrix[x + 1, y + 1])
                    {
                        folderbits[2] = 1;
                        kdIndex += 2;
                    }

                    if (!matrix[x, y + 1])
                    {
                        folderbits[3] = 1;
                        kdIndex += 1;
                    }

                    
                    string folder = folderbits[0].ToString() + folderbits[1].ToString() + folderbits[2].ToString()
                        + folderbits[3].ToString();
                    string[] names = System.IO.Directory.GetFiles(FLANN.ClassifyPath + folder);
                    string name = FLANN.Search4x4LabOther(newSrc, x, y, MainForm.singleton.BlockSize, names, kdIndex, 100);
                    Bitmap tile = Image.FromFile(name) as Bitmap;
                    int px = 0, py = 0;
                    int ytile = y * tileSize;
                    int xtile = x * tileSize;
                    int totalx = xtile + tileSize;
                    int totaly = ytile + tileSize;
                    
                    for (int i = ytile; i < totaly; ++i)
                    {
                        for (int j = xtile; j < totalx; ++j)
                        {
                            dst.SetPixel(j, i, tile.GetPixel(px, py));
                            px++;
                        }
                        px = 0;
                        py++;
                    }
                    folderbits[0] = folderbits[1] = folderbits[2] = folderbits[3] = 0;
                }
            }
            int halftile = tileSize / 2;
            Bitmap result = new Bitmap(dst.Width + 3 * tileSize, dst.Height + 3 * tileSize);
            int AlignmentPatternLocation_X = MainForm.singleton.info.AlignmentPatternLocation_X * tileSize;
            int AlignmentPatternLocation_Y = MainForm.singleton.info.AlignmentPatternLocation_Y * tileSize;
            int originX1 = tileSize + halftile;
            int originY1 = tileSize + halftile;
            int originX2 = result.Width - tileSize - halftile - 1;
            int originY2 = result.Height - tileSize - halftile - 1;
            #region AddFunctionPatterns
                for (int y = 0; y < QRMatrix.Height; ++y)
                {
                    worker.ReportProgress(y * 10 / QRMatrix.Height + 70);
                    for (int x = 0; x < QRMatrix.Width; ++x)
                    {
                        for (int i = y * tileSize + halftile; i < y * tileSize + halftile + tileSize; ++i)
                        {
                            for (int j = x * tileSize + halftile; j < x * tileSize + halftile + tileSize; ++j)
                            {
                                #region Left
                                if (x < 9 && y < 9)
                                {
                                    if (matrix[x + 1, y + 1]) //黑為1 白為0
                                    {
                                        Color pixel = Utility.waterMarkDarkPixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                    else
                                    {
                                        Color pixel = Utility.waterMarkWhitePixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                }
                                #endregion
                                #region Timing
                                else if (y == 6 && x >= 9 && x < matrix.Width - 10)
                                {
                                    if (matrix[x + 1, y + 1])
                                    {
                                        dst.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        dst.SetPixel(j, i, Color.White);
                                    }
                                }
                                else if (x == 6 && y >= 9 && y < matrix.Height - 10)
                                {
                                    if (matrix[x + 1, y + 1])
                                    {
                                        dst.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        dst.SetPixel(j, i, Color.White);
                                    }
                                }
                                #endregion
                                #region FormatInfo
                                else if (y >= matrix.Height - 13 && y < matrix.Height - 10 && x>=0 && x <=5)
                                {
                                    if (matrix[x + 1, y + 1])
                                    {
                                        dst.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        dst.SetPixel(j, i, Color.White);
                                    }
                                }
                                else if (x >= matrix.Width - 13 && x < matrix.Width - 10 && y >= 0 && y <= 5)
                                {
                                    if (matrix[x + 1, y + 1])
                                    {
                                        dst.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        dst.SetPixel(j, i, Color.White);
                                    }
                                }
                                #endregion
                                #region right
                                else if (y < 9 && x >= matrix.Width - 10)
                                {
                                    if (matrix[x + 1, y + 1]) //黑為1 白為0
                                    {
                                        Color pixel = Utility.waterMarkDarkPixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                    else
                                    {
                                        Color pixel = Utility.waterMarkWhitePixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                }
                                #endregion
                                else if (x < 9 && y >= matrix.Height - 10)
                                {
                                    if (matrix[x + 1, y + 1]) //黑為1 白為0
                                    {
                                        Color pixel = Utility.waterMarkDarkPixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                    else
                                    {
                                        Color pixel = Utility.waterMarkWhitePixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                }
                                else if (version > 1 && j >= AlignmentPatternLocation_X + halftile && j < AlignmentPatternLocation_X + tileSize * 5+halftile
                        && i >= AlignmentPatternLocation_Y + halftile && i < AlignmentPatternLocation_X + tileSize * 5 + halftile)
                                {
                                    if (matrix[x + 1, y + 1]) //黑為1 白為0
                                    {
                                        Color pixel = Utility.waterMarkDarkPixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                    else
                                    {
                                        Color pixel = Utility.waterMarkWhitePixel(dst.GetPixel(j, i));
                                        dst.SetPixel(j, i, pixel);
                                    }
                                }
                            }
                        }
                    }
                }
            #endregion
            
            #region AddQuiet
            for (int m = 0; m < result.Height; ++m)
            {
                worker.ReportProgress(m * 10 / result.Height + 80);
                if (MainForm.singleton.isCancel)
                    return null;
                for (int n = 0; n < result.Width; ++n)
                {
                    if (n >= originX1 + halftile && n <= originX2 - halftile && m >= originY1 + halftile && m <= originY2 - halftile)
                    {
                        Color pixel = dst.GetPixel(n - originX1, m - originY1);
                        result.SetPixel(n, m, pixel);
                    }
                    else
                    {
                        result.SetPixel(n, m, Color.White);
                    }
                }
            }
                
            #endregion
            GC.Collect();
            return result;
        }

        public Bitmap GenerateByFlann4x4(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k, string space)
        {
            int dstSize = ((version * 4 + 17) + 1) * tileSize; // Depend on qr version
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<string> candicatetiles = new List<string>();
            if(space == "RGB")
                FLANN.Search4x4(src, version, k);
            else if(space == "Lab")
                FLANN.Search4x4Lab(src, version, k);

            int currBlockIdx = 0;
            Bitmap candidateImg = null;
            for (int y = 0; y < dst.Height; y += tileSize)
            {
                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress((y * 90) / dst.Height + 10);
                int index = 0;
                for (int x = 0; x < dst.Width; x += tileSize)
                {
                    //int tileIdx = 0;
                    for (int l = 0; l < k; ++l)
                    {
                        if (candicatetiles.Contains(tiles[FLANN.Indices4x4.Data[currBlockIdx, l]].Name))
                            continue;
                        else
                        {
                            //tileIdx = l;
                            index = FLANN.Indices4x4.Data[currBlockIdx, l];
                            candicatetiles.Add(tiles[index].Name);
                            break;
                        }
                    }
                    if (MainForm.singleton.isCancel) return null;
                    candidateImg = Image.FromFile(tiles[index].Name) as Bitmap;
                    //tryMethod(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]], currBlockIdx, y, x, candidateImg, dst);
                    
                    for (int i = 0; i < tileSize; i++)
                    {
                        
                        for (int j = 0; j < tileSize; j ++)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            //tryMethod(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]], currBlockIdx, y, x, candidateImg, dst);
                            dst.SetPixel(j + x, i + y, candidateImg.GetPixel(j, i));
                        }
                    }
                    candidateImg.Dispose();
                    currBlockIdx++;
                }
            }
            GC.Collect();
            return dst;
        }

        public Bitmap GenerateByFlann(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k)
        {
            //int v = (version * 4 + 17) + 1;
            int dstSize = ((version * 4 + 17) + 1) * tileSize; // Depend on qr version
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            Matrix<int> indices = new Matrix<int>(dstSize, 1);
            Matrix<float> dist = new Matrix<float>(dstSize, 1);
            List<string> candicatetiles = new List<string>();
            FLANN.Search(src, version, k);
            //int test = 0;
            
            int currBlockIdx = 0;
            Bitmap candidateImg = null;
            for (int y = 0; y < dst.Height; y += tileSize)
            {
                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress((y*90) / dst.Height  + 10);
                for (int x = 0; x < dst.Width; x += tileSize)
                {
                    //if (MainForm.singleton.isCancel) return null;

                    int tileIdx = 0;
                    

                    for (int l = 0; l < k; ++l)
                    {
                        if (MainForm.singleton.isCancel) return null;

                        if (!candicatetiles.Contains(tiles[FLANN.Indices.Data[currBlockIdx, l]].Name))
                        {
                            tileIdx = l;
                            candicatetiles.Add(tiles[FLANN.Indices.Data[currBlockIdx, tileIdx]].Name);
                            break;
                        }
                    }
                    /*
                    if (tileIdx == -1)
                    {
                        tileIdx = 0;
                        test++;
                    }*/
                    

                    //if (MainForm.singleton.isCancel) return null;
                    int index = FLANN.Indices.Data[currBlockIdx, tileIdx];
                    Tile tile = tiles[index];
                    candidateImg = Image.FromFile(tile.Name) as Bitmap;

                    float var_B = (float)tile.avgRGB.B - FLANN.Query.Data[currBlockIdx, 2];
                    float var_G = (float)tile.avgRGB.G - FLANN.Query.Data[currBlockIdx, 1];
                    float var_R = (float)tile.avgRGB.R - FLANN.Query.Data[currBlockIdx, 0];

                    for (int i = 0; i < tileSize; ++i)
                    {
                        //if (MainForm.singleton.isCancel) return null;
                        for (int j = 0; j < tileSize; ++j)
                        {
                            //if (MainForm.singleton.isCancel) return null;
                            
                            
                            ColorSpace.RGB rgb;
                            Color p = candidateImg.GetPixel(j, i);
                            
                            rgb.R = (int)((float)(p.R) - var_R);
                            rgb.G = (int)((float)(p.G) - var_G);
                            rgb.B = (int)((float)(p.B) - var_B);

                            rgb.R = ImageProc.NormalizeRGB(rgb.R);
                            rgb.G = ImageProc.NormalizeRGB(rgb.G);
                            rgb.B = ImageProc.NormalizeRGB(rgb.B);

                            Color pixel = Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
                             
                            dst.SetPixel(j+x, i+y, pixel);
                            
                            //dst.SetPixel(j + x, i + y, candidateImg.GetPixel(i,j));
                        }
                    }
                    candidateImg.Dispose();
                    currBlockIdx++;
                }
            }

            if (MainForm.singleton.isCancel)
            {
                GC.Collect();
                return null;
            }
            return dst;
        }


        public Bitmap GenerateByNormalMethod(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version)
        {
            //int v = (version * 4 + 17 + 1) * 2;
            int v = (version * 4 + 17) + 1;
            int blockSize = MainForm.singleton.BlockSize;
            //blockSize = 8;
            int dstSize = v * tileSize; // Depend on qr version
            double blockTotal = blockSize * blockSize;
            int width = v * blockSize;
            int height = width;
            Bitmap newSrc = ImageProc.ScaleImage(src, width, height); // scaled src
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<int> candidates = new List<int>(); // choosen index of candidate'
            int currDstH = 0;
            int currDstW = 0;
            
            //double max = double.MinValue;
            //FLANN.Search(src, version);

            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                currDstW = 0;
                if (MainForm.singleton.isCancel) return null;

                worker.ReportProgress( (y*90)/newSrc.Height + 10);
                for (int x = 0; x < newSrc.Width; x += blockSize)
                {
                    ColorSpace.RGB blockAvg;
                    double blockAvgR = 0.0f, blockAvgG = 0.0f, blockAvgB = 0.0f;
                    //Bitmap currBlock = new Bitmap(blockSize, blockSize);
                    //int currX = 0, currY = 0;
                    blockAvg.R = blockAvg.G = blockAvg.B = 0;
                    if (MainForm.singleton.isCancel) return null;
                    for(int i = y; i < y + blockSize; ++i)
                    {
                        for(int j = x; j < x + blockSize; ++j)
                        {
                            Color pixel = newSrc.GetPixel(j, i);
                            blockAvgR += (double)(pixel.R);
                            blockAvgG += (double)(pixel.G);
                            blockAvgB += (double)(pixel.B);
                            //currBlock.SetPixel(currX++, currY, pixel);
                        }
                        //currY++;
                        //currX = 0;
                    }
                    blockAvgR /= blockTotal;
                    blockAvgG /= blockTotal;
                    blockAvgB /= blockTotal;
                    double min = double.MaxValue;
                    Tile candidateTile = null;
                    Bitmap candidateImg = null;
                    foreach(Tile tile in tiles)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        if (tile.UseTimes == 1)
                            continue;
                        

                        double r = Math.Pow(((double)tile.avgRGB.R - blockAvgR), 2);
                        double g = Math.Pow(((double)tile.avgRGB.G - blockAvgG), 2);
                        double b = Math.Pow(((double)tile.avgRGB.B - blockAvgB), 2);
                        double d = Math.Sqrt(r + g + b);
                        if (d < min)
                        {
                            min = d;
                            candidateTile = tile;
                            candidateImg = Image.FromFile(tile.Name) as Bitmap;
                            //if (candidateImg.Width != tileSize || candidateImg.Height != tileSize)
                            //    candidateImg = ImageProc.ScaleImage(candidateImg, tileSize);
                        }
                        /*
                        if(d > max)
                        {
                            max = d;
                        }
                        */
                    }
                    #region Replace the block by candiate tile
                    //currBlock = ImageProc.ScaleImage(currBlock, tileSize);
                    candidateTile.UseTimes++;
                    int tw = 0, th = 0;
                    //double alpha = min / max;
                    double var_B = (double)candidateTile.avgRGB.B - blockAvgB;
                    double var_G = (double)candidateTile.avgRGB.G - blockAvgG;
                    double var_R = (double)candidateTile.avgRGB.R - blockAvgR;
                    for (int h = currDstH; th < tileSize; ++h)
                    {
                        for (int w = currDstW; tw < tileSize; ++w)
                        {
                            //if (MainForm.singleton.isCancel) return null;
                            ColorSpace.RGB rgb;

                            Color p = candidateImg.GetPixel(tw, th);
                            rgb.R = Convert.ToInt32(Convert.ToDouble(p.R) - var_R);
                            rgb.G = Convert.ToInt32(Convert.ToDouble(p.G) - var_G);
                            rgb.B = Convert.ToInt32(Convert.ToDouble(p.B) - var_B);

                            rgb.R = ImageProc.NormalizeRGB(rgb.R);
                            rgb.G = ImageProc.NormalizeRGB(rgb.G);
                            rgb.B = ImageProc.NormalizeRGB(rgb.B);

                            tw++;

                            Color pixel = Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
                            dst.SetPixel(w, h, pixel);
                        }
                        th++;
                        tw = 0;
                    }
                    candidateImg.Dispose();
                    currDstW += tileSize;
                    #endregion 
                }
                currDstH += tileSize;
            }

            if (MainForm.singleton.isCancel)
            {
                GC.Collect();
                return null; 
            }
            return dst;
        }

        public Bitmap GenerateByNormalMethod4x4(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version)
        {
            //int v = (version * 4 + 17 + 1) * 2;
            int v = (version * 4 + 17) + 1;
            int blockSize = MainForm.singleton.BlockSize;
            //blockSize = 8;
            int dstSize = v * tileSize; // Depend on qr version
            double blockTotal = blockSize * blockSize;
            int width = v * blockSize;
            int height = width;
            Bitmap newSrc = ImageProc.ScaleImage(src, width, height); // scaled src
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<int> candidates = new List<int>(); // choosen index of candidate'
            int currDstH = 0;
            int currDstW = 0;
            int quater = blockSize / 4;
            int dq = quater * quater;

            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                currDstW = 0;
                if (MainForm.singleton.isCancel) return null;

                worker.ReportProgress((y * 90) / newSrc.Height + 10);
                for (int x = 0; x < newSrc.Width; x += blockSize)
                {
                    ColorSpace.RGB [] vec = new ColorSpace.RGB [16];
                    if (MainForm.singleton.isCancel) return null;
                    int vecIdx = 0;
                    for (int i = y; i < y + blockSize; i+=quater)
                    {
                        for (int j = x; j < x + blockSize; j+=quater)
                        {
                            for(int m = 0; m < quater; ++m)
                            {
                                for (int n = 0; n < quater; ++n)
                                {
                                    Color pixel = newSrc.GetPixel(j+n, i+m);
                                    vec[vecIdx].R += pixel.R;
                                    vec[vecIdx].G += pixel.G;
                                    vec[vecIdx].B += pixel.B;
                                }
                            }
                            vec[vecIdx].R /= dq;
                            vec[vecIdx].G /= dq;
                            vec[vecIdx].B /= dq;
                            vecIdx++;
                        }
                    }

                    double min = double.MaxValue;
                    Tile candidateTile = null;
                    Bitmap candidateImg = null;
                    string name = string.Empty;
                    foreach (Tile tile in tiles)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        //candidateImg = Image.FromFile("..\\data\\2957.jpg") as Bitmap;
                        if (tile.UseTimes == 1)
                            continue;
                        double d = 0;
                        for (int t = 0; t < 16; ++t)
                        {
                            double r = Math.Pow(Math.Abs(tile.rgb4x4[t].R - vec[t].R), 2);
                            double g = Math.Pow(Math.Abs(tile.rgb4x4[t].G - vec[t].G), 2);
                            double b = Math.Pow(Math.Abs(tile.rgb4x4[t].B - vec[t].B), 2);
                            d += (r + g + b);
                        }
                        d = Math.Sqrt(d);
                        if (d < min)
                        {
                            min = d;
                            candidateTile = tile;
                            name = tile.Name;
                            //candidateImg = Image.FromFile(tile.Name) as Bitmap;
                        }
                    }
                    candidateImg = Image.FromFile(name) as Bitmap;
                    #region Replace the block by candiate tile
                    //currBlock = ImageProc.ScaleImage(currBlock, tileSize);
                    candidateTile.UseTimes++;
                    int tw = 0, th = 0;
                    for (int h = currDstH; th < tileSize; ++h)
                    {
                        for (int w = currDstW; tw < tileSize; ++w)
                        {
                            Color p = candidateImg.GetPixel(tw, th);
                            tw++;
                            dst.SetPixel(w, h, p);
                        }
                        th++;
                        tw = 0;
                    }
                    candidateImg.Dispose();
                    currDstW += tileSize;
                    #endregion
                }
                currDstH += tileSize;
            }

            if (MainForm.singleton.isCancel)
            {
                GC.Collect();
                return null;
            }
            return dst;
        }

        public Bitmap GenerateByNormalMethod4x4Lab(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version)
        {
            //int v = (version * 4 + 17 + 1) * 2;
            int v = (version * 4 + 17) + 1;
            int blockSize = MainForm.singleton.BlockSize;
            //blockSize = 8;
            int dstSize = v * tileSize; // Depend on qr version
            double blockTotal = blockSize * blockSize;
            int width = v * blockSize;
            int height = width;
            ColorSpace cs = new ColorSpace();
            Bitmap newSrc = ImageProc.ScaleImage(src, width, height); // scaled src
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<int> candidates = new List<int>(); // choosen index of candidate'
            int currDstH = 0;
            int currDstW = 0;
            int quater = blockSize / 4;
            int dq = quater * quater;

            for (int y = 0; y < newSrc.Height; y += blockSize)
            {
                currDstW = 0;
                if (MainForm.singleton.isCancel) return null;

                worker.ReportProgress((y * 90) / newSrc.Height + 10);
                for (int x = 0; x < newSrc.Width; x += blockSize)
                {
                    ColorSpace.Lab[] LAB = new ColorSpace.Lab[16];
                    int R = 0, G = 0, B = 0;
                    if (MainForm.singleton.isCancel) return null;
                    int vecIdx = 0;
                    for (int i = y; i < y + blockSize; i += quater)
                    {
                        for (int j = x; j < x + blockSize; j += quater)
                        {
                            for (int m = 0; m < quater; ++m)
                            {
                                for (int n = 0; n < quater; ++n)
                                {
                                    Color pixel = newSrc.GetPixel(j + n, i + m);
                                    R += pixel.R;
                                    G += pixel.G;
                                    B += pixel.B;
                                    //vec[vecIdx].R += pixel.R;
                                    //vec[vecIdx].G += pixel.G;
                                    //vec[vecIdx].B += pixel.B;
                                }
                            }
                            R /= dq;
                            G /= dq;
                            B /= dq;
                            LAB[vecIdx] = cs.RGB2Lab(R, G, B);
                            vecIdx++;
                            R = G = B = 0;
                        }
                    }

                    double min = double.MaxValue;
                    Tile candidateTile = null;
                    Bitmap candidateImg = null;
                    string name = string.Empty;
                    foreach (Tile tile in tiles)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        //candidateImg = Image.FromFile("..\\data\\2957.jpg") as Bitmap;
                        if (tile.UseTimes == 1)
                            continue;
                        double d = 0;
                        for (int t = 0; t < 16; ++t)
                        {
                            ColorSpace.Lab Lab= tile.lab4x4[t];

                            double r = Math.Pow(Math.Abs(Lab.L - LAB[t].L), 2);
                            double g = Math.Pow(Math.Abs(Lab.a - LAB[t].a), 2);
                            double b = Math.Pow(Math.Abs(Lab.b - LAB[t].b), 2);
                            d += (r + g + b);
                        }
                        d = Math.Sqrt(d);
                        if (d < min)
                        {
                            min = d;
                            candidateTile = tile;
                            name = tile.Name;
                            //candidateImg = Image.FromFile(tile.Name) as Bitmap;
                        }
                    }
                    candidateImg = Image.FromFile(name) as Bitmap;
                    #region Replace the block by candiate tile
                    //currBlock = ImageProc.ScaleImage(currBlock, tileSize);
                    candidateTile.UseTimes++;
                    int tw = 0, th = 0;
                    for (int h = currDstH; th < tileSize; ++h)
                    {
                        for (int w = currDstW; tw < tileSize; ++w)
                        {
                            Color p = candidateImg.GetPixel(tw, th);
                            tw++;
                            dst.SetPixel(w, h, p);
                        }
                        th++;
                        tw = 0;
                    }
                    candidateImg.Dispose();
                    currDstW += tileSize;
                    #endregion
                }
                currDstH += tileSize;
            }

            if (MainForm.singleton.isCancel)
            {
                GC.Collect();
                return null;
            }
            return dst;
        }
    }
}
