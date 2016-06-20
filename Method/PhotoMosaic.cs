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

        private void tryMethod(Tile tile, int idx, int y, int x, Bitmap img, Bitmap dst)
        {
            ColorSpace.RGB rgb;
            int index = 0;
            int blockidxX = 0;
            int blockidxY = 0;
            for (int m = 0; m < 48;)
            {
                float var_R = (float)tile.rgb4x4[index].R - FLANN.Query4x4.Data[idx, m++];
                float var_G = (float)tile.rgb4x4[index].G - FLANN.Query4x4.Data[idx, m++];
                float var_B = (float)tile.rgb4x4[index].B - FLANN.Query4x4.Data[idx, m++];
                if (blockidxY >= 64)
                {
                    blockidxY = blockidxX = 0;
                }
                if (blockidxX == 64)
                {
                    blockidxX = 0;
                    blockidxY += 16;
                }

                for (int n = 0; n < 16; ++n )
                {
                    for(int k = 0; k < 16; ++k)
                    {                                                                       
                        rgb.R = Convert.ToInt32(Convert.ToSingle(img.GetPixel(k+blockidxX, n+blockidxY).R) - var_R);
                        rgb.G = Convert.ToInt32(Convert.ToSingle(img.GetPixel(k+blockidxX, n+blockidxY).G) - var_G);
                        rgb.B = Convert.ToInt32(Convert.ToSingle(img.GetPixel(k+blockidxX, n+blockidxY).B) - var_B);

                        rgb.R = ImageProc.NormalizeRGB(rgb.R);
                        rgb.G = ImageProc.NormalizeRGB(rgb.G);
                        rgb.B = ImageProc.NormalizeRGB(rgb.B);
                        Color pixel = Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
                        dst.SetPixel(x + k + blockidxX, y + n + blockidxY, pixel);
                    }
                }
                //x += 64;
                blockidxX += 16;
                index++;
            }

        }

        public Bitmap GenerateByFlann4x4(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k)
        {
            int v = (version * 4 + 17) + 1;
            int dstSize = v * tileSize; // Depend on qr version
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<string> candicatetiles = new List<string>();
            FLANN.Search4x4(src, version, k);

            int currBlockIdx = 0;
            Bitmap candidateImg = null;
            for (int y = 0; y < dst.Height; y += tileSize)
            {
                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress((y * 90) / dst.Height + 10);
                for (int x = 0; x < dst.Width; x += tileSize)
                {
                    if (MainForm.singleton.isCancel) return null;

                    int tileIdx = -1;
                    for (int l = 0; l < k; ++l)
                    {
                        if (MainForm.singleton.isCancel) return null;

                        if (candicatetiles.Contains(tiles[FLANN.Indices4x4.Data[currBlockIdx, l]].Name))
                            continue;
                        else
                        {
                            tileIdx = l;
                            candicatetiles.Add(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]].Name);
                            break;
                        }
                    }

                    if (tileIdx < 0)
                        tileIdx = 0;

                    if (MainForm.singleton.isCancel) return null;
                    candidateImg = Image.FromFile(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]].Name) as Bitmap;
                    if (candidateImg.Width != tileSize || candidateImg.Height != tileSize)
                        candidateImg = ImageProc.ScaleImage(candidateImg, tileSize);

                    if (MainForm.singleton.isCancel) return null;
                    //tryMethod(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]], currBlockIdx, y, x, candidateImg, dst);
                    
                    for (int i = 0; i < tileSize; i++)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        for (int j = 0; j < tileSize; j ++)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            //tryMethod(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]], currBlockIdx, y, x, candidateImg, dst);
                            dst.SetPixel(j + x, i + y, candidateImg.GetPixel(j, i));
                            //Color pixel = Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
                            //dst.SetPixel(j + x, i + y, pixel);
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
            int v = (version * 4 + 17) + 1;
            int dstSize = v * tileSize; // Depend on qr version
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            Matrix<int> indices = new Matrix<int>(dstSize, 1);
            Matrix<float> dist = new Matrix<float>(dstSize, 1);
            //Search(src, version, indices, dist);
            List<string> candicatetiles = new List<string>();
            FLANN.Search(src, version, k);
            
            int currBlockIdx = 0;
            Bitmap candidateImg = null;
            for (int y = 0; y < dst.Height; y += tileSize)
            {
                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress((y*90) / dst.Height  + 10);
                for (int x = 0; x < dst.Width; x += tileSize)
                {
                    if (MainForm.singleton.isCancel) return null;

                    int tileIdx = -1;
                    for (int l = 0; l < k; ++l)
                    {
                        if (MainForm.singleton.isCancel) return null;

                        if (candicatetiles.Contains(tiles[FLANN.Indices.Data[currBlockIdx, l]].Name))
                            continue;
                        else
                        {
                            tileIdx = l;
                            candicatetiles.Add(tiles[FLANN.Indices.Data[currBlockIdx, tileIdx]].Name);
                            break;
                        }
                    }

                    if (tileIdx < 0)
                        tileIdx = 0;

                    if (MainForm.singleton.isCancel) return null;
                    candidateImg = Image.FromFile(tiles[FLANN.Indices.Data[currBlockIdx, tileIdx]].Name) as Bitmap;
                    if (candidateImg.Width != tileSize || candidateImg.Height != tileSize)
                        candidateImg = ImageProc.ScaleImage(candidateImg, tileSize);

                    for (int i = 0; i < tileSize; ++i)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        for (int j = 0; j < tileSize; ++j)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            ColorSpace.RGB rgb;

                            float var_B = (float)tiles[FLANN.Indices.Data[currBlockIdx, tileIdx]].avgRGB.B - FLANN.Query.Data[currBlockIdx, 2];
                            float var_G = (float)tiles[FLANN.Indices.Data[currBlockIdx, tileIdx]].avgRGB.G - FLANN.Query.Data[currBlockIdx, 1];
                            float var_R = (float)tiles[FLANN.Indices.Data[currBlockIdx, tileIdx]].avgRGB.R - FLANN.Query.Data[currBlockIdx, 0];
                            
                            
                            rgb.R = Convert.ToInt32(Convert.ToSingle(candidateImg.GetPixel(j, i).R) - var_R);
                            rgb.G = Convert.ToInt32(Convert.ToSingle(candidateImg.GetPixel(j, i).G) - var_G);
                            rgb.B = Convert.ToInt32(Convert.ToSingle(candidateImg.GetPixel(j, i).B) - var_B);

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
            GC.Collect();
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
                    Bitmap currBlock = new Bitmap(blockSize, blockSize);
                    int currX = 0, currY = 0;
                    blockAvg.R = blockAvg.G = blockAvg.B = 0;
                    if (MainForm.singleton.isCancel) return null;
                    for(int i = y; i < y + blockSize; ++i)
                    {
                        for(int j = x; j < x + blockSize; ++j)
                        {
                            blockAvgR += Convert.ToDouble(newSrc.GetPixel(j, i).R);
                            blockAvgG += Convert.ToDouble(newSrc.GetPixel(j, i).G);
                            blockAvgB += Convert.ToDouble(newSrc.GetPixel(j, i).B);
                            currBlock.SetPixel(currX++, currY, newSrc.GetPixel(j, i));
                        }
                        currY++;
                        currX = 0;
                    }
                    blockAvgR /= blockTotal;
                    blockAvgG /= blockTotal;
                    blockAvgB /= blockTotal;
                    double min = double.MaxValue;
                    double max = double.MinValue;
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
                            if (candidateImg.Width != tileSize || candidateImg.Height != tileSize)
                                candidateImg = ImageProc.ScaleImage(candidateImg, tileSize);
                        }
                        if(d > max)
                        {
                            max = d;
                        }
                    }
                    #region Replace the block by candiate tile
                    currBlock = ImageProc.ScaleImage(currBlock, tileSize);
                    candidateTile.UseTimes++;
                    int tw = 0, th = 0;
                    double alpha = min / max;
                    double var_B = (double)candidateTile.avgRGB.B - blockAvgB;
                    double var_G = (double)candidateTile.avgRGB.G - blockAvgG;
                    double var_R = (double)candidateTile.avgRGB.R - blockAvgR;
                    for (int h = currDstH; th < tileSize; ++h)
                    {
                        for (int w = currDstW; tw < tileSize; ++w)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            ColorSpace.RGB rgb;

                            rgb.R = Convert.ToInt32(Convert.ToDouble(candidateImg.GetPixel(tw, th).R) - var_R);
                            rgb.G = Convert.ToInt32(Convert.ToDouble(candidateImg.GetPixel(tw, th).G) - var_G);
                            rgb.B = Convert.ToInt32(Convert.ToDouble(candidateImg.GetPixel(tw++, th).B) - var_B);

                            rgb.R = ImageProc.NormalizeRGB(rgb.R);
                            rgb.G = ImageProc.NormalizeRGB(rgb.G);
                            rgb.B = ImageProc.NormalizeRGB(rgb.B);

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
            GC.Collect();
            if (MainForm.singleton.isCancel) return null;
            return dst;
        }
    }
}
