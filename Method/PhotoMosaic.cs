﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;


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

        public struct Block
        {
            public string Size { get; set; }
            public int Value{ get; set; }
        }
        public static List<Block> blockList;

        public static void Init()
        {
            blockList = new List<Block>()
            {
                new Block
                {
                    Size = "8",
                    Value = 8,
                },
                new Block
                {
                    Size = "4",
                    Value = 4,
                },
            };
        }


        public Bitmap GenerateByNormalMethod(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version)
        {
            //int v = (version * 4 + 17 + 1) * 2;
            int v = (version * 4 + 17) + 1;
            int blockSize = MainForm.singleton.blockSize;
            int dstSize = v * tileSize; // Depend on qr version
            double blockTotal = blockSize * blockSize;
            int width = v * blockSize;
            int height = width;
            Bitmap newSrc = ImageProc.ScaleImage(src, width, height); // scaled src
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<int> candidates = new List<int>(); // choosen index of candidate'
            int currDstH = 0;
            int currDstW = 0;
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
                    Tile candiate = null;
                    foreach(Tile tile in tiles)
                    {
                        if (MainForm.singleton.isCancel) return null;

                        double r = Math.Pow(((double)tile.avg.R - blockAvgR), 2);
                        double g = Math.Pow(((double)tile.avg.G - blockAvgG), 2);
                        double b = Math.Pow(((double)tile.avg.B - blockAvgB), 2);
                        double d = Math.Sqrt(r + g + b);
                        if (d < min)
                        {
                            min = d;
                            candiate = tile;
                        }
                        if(d > max)
                        {
                            max = d;
                        }
                    }
                    #region Replace the block by candiate tile
                    currBlock = ImageProc.ScaleImage(currBlock, tileSize);
                    int tw = 0, th = 0;
                    double alpha = min / max;
                    double var_B = (double)candiate.avg.B - blockAvgB;
                    double var_G = (double)candiate.avg.G - blockAvgG;
                    double var_R = (double)candiate.avg.R - blockAvgR;
                    for (int h = currDstH; th < tileSize; ++h)
                    {
                        for (int w = currDstW; tw < tileSize; ++w)
                        {
                            // out of range, overflow
                            if (MainForm.singleton.isCancel) return null;
                            ColorSpace.RGB rgb;
                            //rgb.R = (int)(Convert.ToDouble(currBlock.GetPixel(tw, th).R) * alpha + (1 - alpha) * Convert.ToDouble(candiate.bitmap.GetPixel(tw, th).R));
                            //rgb.G = (int)(Convert.ToDouble(currBlock.GetPixel(tw, th).G) * alpha + (1 - alpha) * Convert.ToDouble(candiate.bitmap.GetPixel(tw, th).G));
                            //rgb.B = (int)(Convert.ToDouble(currBlock.GetPixel(tw, th).B) * alpha + (1 - alpha) * Convert.ToDouble(candiate.bitmap.GetPixel(tw++, th).B));
                            rgb.R = Convert.ToInt32(Convert.ToDouble(candiate.bitmap.GetPixel(tw, th).R) - var_R);
                            rgb.G = Convert.ToInt32(Convert.ToDouble(candiate.bitmap.GetPixel(tw, th).G) - var_G);
                            rgb.B = Convert.ToInt32(Convert.ToDouble(candiate.bitmap.GetPixel(tw++, th).B) - var_B);
                            rgb.R = ImageProc.NormalizeRGB(rgb.R);
                            rgb.G = ImageProc.NormalizeRGB(rgb.G);
                            rgb.B = ImageProc.NormalizeRGB(rgb.B);

                            Color pixel = Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
                            dst.SetPixel(w, h, pixel);
                        }
                        th++;
                        tw = 0;
                    }
                    currDstW += tileSize;
                    #endregion 
                }
                currDstH += tileSize;
            }
            if (MainForm.singleton.isCancel) return null;
            return dst;
        }
    }
}
