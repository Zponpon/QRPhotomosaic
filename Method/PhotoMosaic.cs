using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static MainForm main;

        public PhotoMosaic()
        {
            
        }
        
        
        public Bitmap GenerateByNormalMethod(Bitmap src, int blockSize, List<Tile> tiles, int tileSize, int version)
        {
            int dstSize = (version + 17 + 1) * tileSize; // Depend on qr version
            int blockTotal = blockSize * blockSize;
            int width = src.Width - (src.Width % blockSize);
            int height = width;
            //int height = src.Height - (src.Width % blockSize);
            Bitmap newSrc = ImageProc.ScaleImage(src, width, height); // scaled src
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<int> candidates = new List<int>(); // choosen index of candidate'
            int currDstBlock = 0;
            for (int y = 0; y < src.Height; y += tileSize)
            {
                for(int x = 0; x < src.Width; x += tileSize)
                {
                    ColorSpace.RGB blockAvg;
                    blockAvg.R = blockAvg.G = blockAvg.B = 0;
                    for(int i = y; i < y + blockSize; ++i)
                    {
                        for(int j = x; j < x + blockSize; ++j)
                        {
                            blockAvg.R += Convert.ToInt32(newSrc.GetPixel(j, i).R);
                            blockAvg.G += Convert.ToInt32(newSrc.GetPixel(j, i).G);
                            blockAvg.B += Convert.ToInt32(newSrc.GetPixel(j, i).B);
                        }
                    }
                    blockAvg.R /= blockTotal;
                    blockAvg.G /= blockTotal;
                    blockAvg.B /= blockTotal;
                    double min = double.MaxValue;
                    Tile candiate = null;
                    foreach(Tile tile in tiles)
                    {
                        if (tile.isSelected)
                            continue;
                        double r = Math.Pow((double)(tile.avg.R - blockAvg.R), 2);
                        double g = Math.Pow((double)(tile.avg.G - blockAvg.G), 2);
                        double b = Math.Pow((double)(tile.avg.B - blockAvg.B), 2);
                        double d = Math.Sqrt(r + g + b);
                        if (d < min)
                        {
                            min = d;
                            candiate = tile;
                        }
                    }
                    candiate.isSelected = true;
                    // Replace the block by candiate tile
                    int tw = 0, th = 0;
                    for (int h = currDstBlock; h < currDstBlock + tileSize; ++h)
                    {
                        for (int w = currDstBlock; w < currDstBlock + tileSize; ++w)
                        {
                            Color color = candiate.bitmap.GetPixel(tw, th);
                            dst.SetPixel(w, h, color);
                        }
                    }
                    currDstBlock += tileSize;
                    //candidates.Add(tiles.IndexOf(candiate));
                    //candidates.
                }
            }
            return null;
        }
    }
}
