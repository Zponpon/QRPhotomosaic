﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
        public string functionPattern = string.Empty;
        public string versionModule = string.Empty;

        private static int[] alignments = { 0, 0, 16, 20, 24, 28, 32};

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

        private void InitDict()
        {
            if (FLANN.folderDict.Count > 0)
                return;
            for(int i = 0; i < 16; ++i)
            {
                int num = i;
                FLANN.folderDict.Add(i, new List<int>());
                for (int j = 0; j < 4; ++j)
                {
                    FLANN.folderDict[i].Add(0);
                }
                for (int j = 3; num > 1; --j)
                {
                    FLANN.folderDict[i][j] = (num % 2);
                    num /= 2;
                }
                FLANN.folderDict[i][0] = (num);
            }
        }

        private void ImportantModuleByTile1By1(Bitmap src, Bitmap module, int tileSize, int startX, int startY, int endX, int endY, string space, string path)
        {
            string name = string.Empty;
            if(space == "Lab")
            {
                //name = FLANN.SearchFunctionPatternsLab(module, tileSize, PathConfig.FunctionDarkPath, 200);
                name = FLANN.SearchFunctionPatternsLab(module, tileSize, path, 200);
            }
            else if (space == "RGB")
            {
                name = FLANN.SearchFunctionPatternsRGB(module, tileSize, 200);
            }

            Image img = Image.FromFile(name);
            Bitmap tile = img as Bitmap;
            int tx = 0, ty = 0;
            for (int y = startY; y < endY; ++y)
            {
                for (int x = startX; x < endX; ++x)
                {
                    Color pixel = tile.GetPixel(tx++, ty);
                    src.SetPixel(x, y, pixel);
                }
                ty++;
                tx = 0;
            }

            img.Dispose();
            tile.Dispose();
        }

        private int GetBlockSize(int size)
        {
            if (size <= 6)
            {
                return 4;
            }
            else if(size >= 6 && size <=12)
            {
                return 8;
            }
            else if(size >= 12 && size <= 24)
            {
                return 16;
            }
            else if(size >= 24 && size <= 48)
            {
                return 32;
            }

            return 64;
        }

        private void BitSwitch(int bit, int tileSize, int x, int y, out int x_, out int y_)
        {
            switch (bit)
            {
                case 0:
                    x_ = x;
                    y_ = y;
                    break;
                case 1:
                    x_ = x + tileSize;
                    y_ = y;
                    break;
                case 2:
                    x_ = x + tileSize;
                    y_ = y + tileSize;
                    break;
                case 3:
                    x_ = x;
                    y_ = y + tileSize;
                    break;
                default:
                    x_ = x;
                    y_ = y;
                    break;
            }
        }

        private void ReplaceColor(Bitmap img, int x, int y, int tileSize, bool value, int K = 2)
        {
            int dt = tileSize * K;
            if(value)
            {
                for(int i = 0; i < dt; ++i)
                {
                    for (int j = 0; j < dt; ++j)
                    {
                        img.SetPixel(x + j, y + i, Color.Black);
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt; ++i)
                {
                    for (int j = 0; j < dt; ++j)
                    {
                        img.SetPixel(x + j, y + i, Color.White);
                    }
                }
            }
        }

        /// <summary>
        /// 2By1 = Module : Tile = 2 : 1
        /// </summary>
        private void ImportantModuleByTile1By2(Bitmap scaledSrc, int sx, int sy, Bitmap metaImg, int mx, int my, int tileSize, int blockSize, bool value) 
        {
            string path = (value) ? PathConfig.FunctionBlackPath : PathConfig.FunctionWhitePath;
            for (int bit = 0; bit < 4; ++bit)
            {
                int fx, fy, nx, ny;

                #region Bit Switch
                switch (bit)
                {
                    case 0:
                        nx = sx;
                        ny = sy;
                        fx = mx;
                        fy = my;
                        break;
                    case 1:
                        nx = sx + blockSize;
                        ny = sy;
                        fx = mx + tileSize;
                        fy = my;
                        break;
                    case 2:
                        nx = sx + blockSize;
                        ny = sy + blockSize;
                        fx = mx + tileSize;
                        fy = my + tileSize;
                        break;
                    case 3:
                        nx = sx;
                        ny = sy + blockSize;
                        fx = mx;
                        fy = my + tileSize;
                        break;
                    default:
                        nx = sx;
                        ny = sy;
                        fx = mx;
                        fy = my;
                        break;
                }
                #endregion

                string tileName = FLANN.SearchFunctionPatternsLab(scaledSrc, blockSize, path, 350, nx, ny);
                Image img = Image.FromFile(tileName);
                Bitmap tile = img as Bitmap;
                for (int i = 0; i < tileSize; ++i)
                {
                    for (int j = 0; j < tileSize; ++j)
                    {
                        metaImg.SetPixel(fx + j, fy + i, tile.GetPixel(j, i));
                    }
                }
                img.Dispose();
                tile.Dispose();
            }
        }
        
        /// <summary>
        /// 1By3 = Tile : Module = 1 : 3
        /// </summary>
        private void ImportantModuleByTile1By3(Bitmap scaledSrc, int sx, int sy, Bitmap metaImg, int mx, int my, int tileSize, int blockSize, bool value)
        {
            string path = (value) ? PathConfig.FunctionBlackPath : PathConfig.FunctionWhitePath;
           

            for (int mY = 0; mY < 3; ++mY)
            {
                for(int mX = 0; mX < 3; ++mX)
                {
                    int x = mx + mX * tileSize;
                    int y = my + mY * tileSize;
                    string tileName = FLANN.SearchFunctionPatternsLab(scaledSrc, blockSize, path, 1000, sx, sy);
                    Image img = Image.FromFile(tileName);
                    Bitmap tile = img as Bitmap;
                    for (int i = 0; i < tileSize; ++i)
                    {
                        for (int j = 0; j < tileSize; ++j)
                        {
                            metaImg.SetPixel(x + j, y + i, tile.GetPixel(j, i));
                        }
                    }
                    img.Dispose();
                    tile.Dispose();
                }
            }
        }

        public Bitmap GenerateByFlann4x4MidMod(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k, string space, BitMatrix QRMatrix)
        {
            int dstSize = (version * 4 + 17) * tileSize; // Depend on qr version
            Bitmap dst = new Bitmap(dstSize, dstSize); // final result
            List<string> candicatetiles = new List<string>();

            FLANN.Search4x4MidLab(src, version, k, QRMatrix);

            int currBlockIdx = 0;
            Bitmap candidateImg = null;
            int bitX = 0, bitY = 0;
            int quater = tileSize / 4;
            int half = tileSize / 2;
            int AlignmentPatternLocation_X = MainForm.singleton.info.AlignmentPatternLocation_X*tileSize;
            int AlignmentPatternLocation_Y = MainForm.singleton.info.AlignmentPatternLocation_Y*tileSize;
            for (int y = 0; y < dst.Height; y += tileSize)
            {

                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress((y * 80) / dst.Height);
                for (int x = 0; x < dst.Width; x += tileSize)
                {
                    int index = -1;
                    //for (int l = 0; l < k; ++l)
                    //{
                    //    if (candicatetiles.Contains(tiles[FLANN.Indices4x4.Data[currBlockIdx, l]].Name))
                    //        continue;
                    //    else
                    //    {
                    //        index = FLANN.Indices4x4.Data[currBlockIdx, l];
                    //        candicatetiles.Add(tiles[index].Name);
                    //        break;
                    //    }
                    //}
                    if(index == -1)
                        index = FLANN.Indices4x4.Data[currBlockIdx, 0];
                    if (MainForm.singleton.isCancel) return null;
                    candidateImg = Image.FromFile(tiles[index].Name) as Bitmap;

                    for (int i = 0; i < tileSize; i++)
                    {
                        for (int j = 0; j < tileSize; j++)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            Color p = candidateImg.GetPixel(j, i);
                            
                            if(bitX < 8 && bitY < 8)
                            {
                                if (QRMatrix[bitX, bitY])
                                {
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkDarkPixel(p));
                                }
                                else
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkWhitePixel(p));
                            }
                            else if (bitX >= QRMatrix.Width - 8 && bitY < 8)
                            {
                                if (QRMatrix[bitX, bitY])
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkDarkPixel(p));
                                else
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkWhitePixel(p));
                            }
                            else if (bitX < 8 && bitY >= QRMatrix.Height - 8)
                            {
                                if (QRMatrix[bitX, bitY])
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkDarkPixel(p));
                                else
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkWhitePixel(p));
                            }
                            else if (version > 1 && j + x >= AlignmentPatternLocation_X && j + x < AlignmentPatternLocation_X + tileSize * 5
                        && i + y >= AlignmentPatternLocation_Y && i + y < AlignmentPatternLocation_X + tileSize * 5)
                            {
                                if (QRMatrix[bitX, bitY])
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkDarkPixel(p));
                                else
                                    dst.SetPixel(j + x, i + y, Utility.waterMarkWhitePixel(p));
                            }
                            else
                            {
                                //if (j >= quater && j <= quater + half - 1 && i >= quater && i <= quater + half - 1)
                                //{
                                //    if (QRMatrix[bitX, bitY])
                                //    {
                                //        int R = Convert.ToInt32(p.R * 0.8);
                                //        int G = Convert.ToInt32(p.G * 0.8);
                                //        int B = Convert.ToInt32(p.B * 0.8);
                                //        dst.SetPixel(j + x, i + y, Color.FromArgb(R, G, B));
                                //    }
                                //    else
                                //    {
                                //        int R = Convert.ToInt32(p.R * 0.8 + 255 * 0.2);
                                //        int G = Convert.ToInt32(p.G * 0.8 + 255 * 0.2);
                                //        int B = Convert.ToInt32(p.B * 0.8 + 255 * 0.2);
                                //        dst.SetPixel(j + x, i + y, Color.FromArgb(R, G, B));
                                //    }
                                //}
                                //else
                                    dst.SetPixel(j + x, i + y, p);
                            }
                        }
                    }
                    candidateImg.Dispose();
                    currBlockIdx++;
                    bitX++;
                }
                bitX = 0;
                bitY++;
            }
            Bitmap result = new Bitmap(dst.Width + 8 * tileSize, dst.Height + 8 * tileSize);
            for (int i = 0; i < result.Height; ++i)
            {
                worker.ReportProgress(i * 20 / result.Height + 80);
                for (int j = 0; j < result.Width; ++j)
                {
                    if (i >= 4 * tileSize && i <= 4 * tileSize + dst.Height - 1 && j >= 4 * tileSize && j <= 4 * tileSize + dst.Width - 1)
                    {
                        result.SetPixel(j, i, dst.GetPixel(j-4*tileSize, i-4*tileSize));
                    }
                    else
                        result.SetPixel(j, i, Color.White);
                }
            }
            GC.Collect();
            return result;
        }

        /// <summary>
        /// Tile : Module = 3 : 1
        /// </summary>
        /// <returns></returns>
        public Bitmap FlannCombine1By3(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k, string space, BitMatrix QRMatrix)
        {
            int v = version * 4 + 17;
            int blockSize = MainForm.singleton.BlockSize;
            int scaledSize = v * blockSize * 3;
            int metaSize = v * 3 * tileSize;
            int tileSize_12 = 12 * tileSize;

            string[] tileImages = Directory.GetFiles(MainForm.singleton.CreatingFolderPath);
            Bitmap scaledSrc = new Bitmap(src, scaledSize, scaledSize);
            Bitmap metaImg = new Bitmap(metaSize, metaSize);
            // 64bits application, REASON: 32bits application does not have enough virtual memory.
            Bitmap result = new Bitmap(metaSize + tileSize_12 * 2, metaSize + tileSize_12 * 2);

            #region Determined Important Module
            bool functionByTile = false;
            if (functionPattern == "WB")
            {
                functionByTile = true;
                if (FLANN.kdtree_Patterns_Black == null)
                {
                    Tile.ReadFileFunctionLab(PathConfig.FunctionBlackPath, 0);
                    FLANN.kdtree_Patterns_Black = new Index(FLANN.FunctionBlackFeatures4x4, 5);
                }
                if (FLANN.kdtree_Patterns_White == null)
                {
                    Tile.ReadFileFunctionLab(PathConfig.FunctionWhitePath, 1);
                    FLANN.kdtree_Patterns_White = new Index(FLANN.FunctionWhiteFeatures4x4, 5);
                }
            }
            bool openInfoModule = false;
            bool usingSpecialTile = false;
            if (versionModule == "Normal" || versionModule == "SpecialTile")
            {
                openInfoModule = true;
                if (versionModule == "SpecialTile")
                    usingSpecialTile = true;
            }
            #endregion

            #region Replace QR Code
            for (int y = 0; y < QRMatrix.Height; ++y)
            {
                if (MainForm.singleton.isCancel)
                    return null;
                worker.ReportProgress(y * 70 / QRMatrix.Height + 10);
                for (int x = 0; x < QRMatrix.Width; ++x)
                {
                    int px = tileSize * x * 3;
                    int py = tileSize * y * 3;
                    int sx = blockSize * x * 3;
                    int sy = blockSize * y * 3;
                    #region Left Up Function Pattern
                    if (x < 8 && y < 8)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Right Up Function Pattern
                    else if (x > QRMatrix.Width - 9 && y < 8)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Left Down Function Pattern
                    else if (x < 8 && y > QRMatrix.Height - 9)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Timing
                    else if (y == 6 && x >= 8 && x < QRMatrix.Width - 8 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    else if (x == 6 && y >= 8 && y < QRMatrix.Height - 8 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Version
                    else if (y > QRMatrix.Height - 12 && y < QRMatrix.Height - 8 && x >= 0 && x <= 5 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    else if (x > QRMatrix.Width - 12 && x < QRMatrix.Width - 8 && y >= 0 && y <= 5 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Format
                    else if (((x == 8 && y < 8) || (y == 8 && x > QRMatrix.Width - 9) || (x == 8 && y > QRMatrix.Height - 9)) && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Alignment Pattern

                    else if (version > 1 && x >= alignments[version] && x < alignments[version] + 5
                   && y >= alignments[version] && y < alignments[version] + 5)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y], 3);
                        }
                        else
                        {
                            ImportantModuleByTile1By3(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }

                    #endregion

                    #region Embedding region
                    else
                    {
                        int luminance = (QRMatrix[x, y]) ? 0 : 1;
                        int bDex = 0;
                        for (int mY = 0; mY < 3; ++mY)
                        {
                            for (int mX = 0; mX < 3; ++mX)
                            {
                                Image img = null;
                                Bitmap tile = null;
                                int bx = sx + mX * blockSize;
                                int by = sy + mY * blockSize;
                                int tx = px + mX * tileSize;
                                int ty = py + mY * tileSize;
                                if (bDex != 4)
                                {
                                    //string tileName = FLANN.Search4x4Lab1By3(scaledSrc, blockSize, nx, ny, luminance, 300, tileImages);
                                    string tileName = FLANN.Search4x4Lab1By3(scaledSrc, blockSize, bx, by, luminance, 2000, tileImages);
                                    img = Image.FromFile(tileName);
                                    tile = img as Bitmap;
                                    for (int i = 0; i < tileSize; ++i)
                                    {
                                        for (int j = 0; j < tileSize; ++j)
                                        {
                                            metaImg.SetPixel(tx + j, ty + i, tile.GetPixel(j, i));
                                        }
                                    }
                                }
                                else
                                {
                                    string path = (QRMatrix[x, y]) ? PathConfig.FunctionBlackPath : PathConfig.FunctionWhitePath;
                                    string tileName = FLANN.SearchFunctionPatternsLab(scaledSrc, blockSize, path, 1000, bx, by, true);
                                    img = Image.FromFile(tileName);
                                    tile = img as Bitmap;
                                    for (int i = 0; i < tileSize; ++i)
                                    {
                                        for (int j = 0; j < tileSize; ++j)
                                        {
                                            metaImg.SetPixel(tx + j, ty + i, tile.GetPixel(j, i));
                                        }
                                    }
                                }
                                bDex++;

                                if (img != null)
                                {
                                    img.Dispose();
                                    tile.Dispose();
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region Add Quiet Zone
            int startX = tileSize_12;
            int startY = tileSize_12;
            int endX = startX + metaImg.Width;
            int endY = startY + metaImg.Height;
            for (int i = 0; i < result.Height; ++i)
            {
                if (MainForm.singleton.isCancel)
                    return null;
                worker.ReportProgress(i * 20 / result.Height + 80);
                for(int j = 0; j < result.Width; ++j)
                {
                    if (MainForm.singleton.isCancel)
                        return null;
                    if (j >= startX && j < endX && i >= startY && i < endY)
                    {
                        //Color pixel = metaImg.GetPixel(j - startX, i - startY);
                        result.SetPixel(j, i, metaImg.GetPixel(j - startX, i - startY));
                    }
                    else
                    {
                        result.SetPixel(j, i, Color.White);
                    }
                }
            }
            #endregion

            scaledSrc.Dispose();
            metaImg.Dispose();
            FLANN.usedList.Clear();
            FLANN.functionList.Clear();
            GC.Collect();
            return result;
        }

        /// <summary>
        /// (Tile | Block) : Module = 1 : 2
        /// </summary>
        /// <returns></returns>
        public Bitmap FlannCombine1By2(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k, string space, BitMatrix QRMatrix)
        {
            int v = version * 4 + 17;
            int blockSize = MainForm.singleton.BlockSize;
            int scaledSize = v * blockSize * 2;
            int metaSize = v * 2 * tileSize;
            int eightTileSize = 8 * tileSize;
            Bitmap scaledSrc = new Bitmap(src, scaledSize, scaledSize);
            Bitmap metaImg = new Bitmap(metaSize, metaSize);

            // 64bits application, REASON: 32bits application does not have enough virtual memory.
            Bitmap result = new Bitmap(metaSize + eightTileSize * 2, metaSize + eightTileSize * 2);

            Dictionary<string, string[]> loadingFolders = new Dictionary<string, string[]>();

            #region Determined Important Module
            bool functionByTile = false;
            if (functionPattern == "WB")
            {
                functionByTile = true;
                FLANN.functionList.Clear();
                if (FLANN.kdtree_Patterns_Black == null)
                {
                    Tile.ReadFileFunctionLab(PathConfig.FunctionBlackPath, 0);
                    FLANN.kdtree_Patterns_Black = new Index(FLANN.FunctionBlackFeatures4x4, 5);
                }
                if (FLANN.kdtree_Patterns_White == null)
                {
                    Tile.ReadFileFunctionLab(PathConfig.FunctionWhitePath, 1);
                    FLANN.kdtree_Patterns_White = new Index(FLANN.FunctionWhiteFeatures4x4, 5);
                }
            }
            bool openInfoModule = false;
            bool usingSpecialTile = false;
            if (versionModule == "Normal" || versionModule == "SpecialTile")
            {
                openInfoModule = true;
                if (versionModule == "SpecialTile")
                    usingSpecialTile = true;
            }
            #endregion

            #region Meta Image
            for (int y = 0; y < QRMatrix.Height; ++y)
            {
                if (MainForm.singleton.isCancel)
                    return null;
                worker.ReportProgress(y * 70 / QRMatrix.Height + 10);
                for(int x = 0; x < QRMatrix.Width; ++x)
                {
                    if (MainForm.singleton.isCancel)
                        return null;
                    int px = tileSize * x * 2;
                    int py = tileSize * y * 2;
                    int sx = blockSize * x * 2;
                    int sy = blockSize * y * 2;
                    #region Left Up Function Pattern
                    if (x < 8 && y < 8)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Right Up Function Pattern
                    else if(x > QRMatrix.Width - 9 && y < 8)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Left Down Function Pattern
                    else if(x < 8 && y > QRMatrix.Height - 9)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Timing
                    else if (y == 6 && x >= 8 && x < QRMatrix.Width - 8 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    else if (x == 6 && y >= 8 && y < QRMatrix.Height - 8 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Version
                    else if (y > QRMatrix.Height - 12 && y < QRMatrix.Height - 8 && x >= 0 && x <= 5 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    else if (x > QRMatrix.Width - 12 && x < QRMatrix.Width - 8 && y >= 0 && y <= 5 && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Format
                    else if (((x == 8 && y < 8) || (y == 8 && x > QRMatrix.Width - 9) || (x == 8 && y > QRMatrix.Height - 9)) && openInfoModule)
                    {
                        if (!usingSpecialTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                    #endregion

                    #region Alignment Pattern

                    else if (version > 1 && x >= alignments[version] && x < alignments[version] + 5
                   && y >= alignments[version] && y < alignments[version] + 5)
                    {
                        if (!functionByTile)
                        {
                            ReplaceColor(metaImg, px, py, tileSize, QRMatrix[x, y]);
                        }
                        else
                        {
                            ImportantModuleByTile1By2(scaledSrc, sx, sy, metaImg, px, py, tileSize, blockSize, QRMatrix[x, y]);
                        }
                    }
                        
                    #endregion

                    #region Embedding region
                    else
                    {
                        int luminance = (QRMatrix[x, y]) ? 0 : 1;
                        int mx = 0, my = 0, nx = 0, ny = 0, cornerBit = 0;
                        for (int bit = 0; bit < 4; ++bit)
                        {
                            #region Bit switch
                            switch (bit)
                            {
                                case 0:
                                    nx = sx;
                                    ny = sy;
                                    mx = px;
                                    my = py;
                                    cornerBit = 2;
                                    break;
                                case 1:
                                    nx = sx + blockSize;
                                    ny = sy;
                                    mx = px + tileSize;
                                    my = py;
                                    cornerBit = 3;
                                    break;
                                case 2:
                                    nx = sx + blockSize;
                                    ny = sy + blockSize;
                                    mx = px + tileSize;
                                    my = py + tileSize;
                                    cornerBit = 0;
                                    break;
                                case 3:
                                    nx = sx;
                                    ny = sy + blockSize;
                                    mx = px;
                                    my = py + tileSize;
                                    cornerBit = 1;
                                    break;
                            }
                            #endregion
                            string path = MainForm.singleton.CreatingFolderPath + cornerBit.ToString() + luminance.ToString() + "\\";
                            string[] tileImages = null;
                            if(!loadingFolders.ContainsKey(path))
                            {
                                tileImages = Directory.GetFiles(path);
                                loadingFolders.Add(path, tileImages);
                            }
                            else
                            {
                                tileImages = loadingFolders[path];
                            }

                            // TEST : k = 200
                            k = 200;
                            string tileName = FLANN.Search4x4Lab1By2(scaledSrc, blockSize, nx, ny, cornerBit, luminance, k, tileImages);
                            Image img = Image.FromFile(tileName);
                            Bitmap tile = img as Bitmap;
                            for (int i = 0; i < tileSize; ++i )
                            {
                                for(int j = 0; j < tileSize; ++j)
                                {
                                    metaImg.SetPixel(mx + j, my + i, tile.GetPixel(j, i));
                                }
                            }
                            img.Dispose();
                            tile.Dispose();
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region Add Quiet Zone
            int startX = eightTileSize;
            int startY = eightTileSize;
            int endX = startX + metaImg.Width;
            int endY = startY + metaImg.Height;
            for (int i = 0; i < result.Height; ++i)
            {
                if (MainForm.singleton.isCancel)
                    return null;
                worker.ReportProgress(i * 20 / result.Height + 80);
                for (int j = 0; j < result.Width; ++j)
                {
                    if (MainForm.singleton.isCancel)
                        return null;
                    if (j >= startX && j < endX && i >= startY && i < endY)
                    {
                        result.SetPixel(j, i, metaImg.GetPixel(j - startX, i - startY));
                    }
                    else
                    {
                        result.SetPixel(j, i, Color.White);
                    }
                }
            }
            #endregion
            scaledSrc.Dispose();
            metaImg.Dispose();
            FLANN.usedList.Clear();
            loadingFolders.Clear();
            GC.Collect();

            return result;
        }

        public Bitmap FlannCombine1By1(BackgroundWorker worker, Bitmap src, List<Tile> tiles, int tileSize, int version, int k, string space, BitMatrix QRMatrix)
        {
            Dictionary<string, string[]> loadingFolders = new Dictionary<string, string[]>();
            BitMatrix matrix = new BitMatrix(QRMatrix.Width + 2, QRMatrix.Height + 2);
            int v = (version * 4 + 17) + 1;
            int metaSize = v * tileSize;
            
            int blockSize = MainForm.singleton.BlockSize;
            Bitmap newSrc = ImageProc.ScaleImage(src, v * blockSize);
            //int w = MainForm.singleton.PhotomosaicPictureBox.Width;
            //int h = MainForm.singleton.PhotomosaicPictureBox.Height;
            //MainForm.singleton.PhotoMosaicImage = ImageProc.ScaleImage(newSrc, w, h);
            Bitmap metaImg = new Bitmap(metaSize, metaSize);
            int halftile = tileSize / 2;
            int AlignmentPatternLocation_X = MainForm.singleton.info.AlignmentPatternLocation_X * tileSize;
            int AlignmentPatternLocation_Y = MainForm.singleton.info.AlignmentPatternLocation_Y * tileSize;

            #region CreateMosaicQRCodeMatrix
            for (int y = 0; y < matrix.Height; ++y)
            {
                for (int x = 0; x < matrix.Width; ++x)
                {
                    if (x != 0 && x != matrix.Width - 1 && y != 0 && y != matrix.Height - 1)
                    {
                        matrix[x, y] = QRMatrix[x-1, y-1];
                    }
                    else
                    {
                        if(x % 2 == 0)
                            matrix[x, y] = true;
                        else
                            matrix[x, y] = false;
                    }
                }
            }
            #endregion

            worker.ReportProgress(20);

            #region Replace Meta Image
            int[] folderbits = { 0, 0, 0, 0 };
            int bitX = 0, bitY = 0;
            for (int y = 0; y < matrix.Height - 1; ++y)
            {
                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress(y * 50 / matrix.Height + 20);
                for (int x = 0; x < matrix.Width - 1; ++x)
                {
                    int kdIndex = 0;
                    if (!matrix[x, y])
                    {
                        // Left Top [0] -> Right Top [1] -> Right Down [2] -> Left Down [3]
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

                    bool isFunction = false;
                    if((x < 8 && y < 8) || (x < 8 && y >= matrix.Height - 9) || (y < 8 && x >= matrix.Width - 9))
                    {
                        isFunction = true;
                    }
                    
                    
                    string folder = folderbits[0].ToString() + folderbits[1].ToString() + folderbits[2].ToString()
                        + folderbits[3].ToString();
                    string[] names = null;
                    string path = MainForm.singleton.CreatingFolderPath + folder;
                    if(!loadingFolders.ContainsKey(path))
                    {
                        names = System.IO.Directory.GetFiles(path);
                        loadingFolders.Add(path, names);
                    }
                    else
                    {
                        names = loadingFolders[path];
                    }
                    //string[] names = System.IO.Directory.GetFiles(MainForm.singleton.CreatingFolderPath + folder);
                    string name = string.Empty;
                    if(space == "RGB")
                        //name = FLANN.Search4x4RGBOther(newSrc, x, y, MainForm.singleton.BlockSize, names, kdIndex, k, isFunction, folderbits);
                        name = FLANN.Search4x4RGB1By1(newSrc, x, y, blockSize, names, kdIndex, k, isFunction, folderbits);
                    else if(space == "Lab")
                        //name = FLANN.Search4x4LabOther(newSrc, x, y, MainForm.singleton.BlockSize, names, kdIndex, k, isFunction, folderbits);
                        name = FLANN.Search4x4Lab1By1(newSrc, x, y, blockSize, names, kdIndex, k, isFunction, folderbits);

                    Image img = Image.FromFile(name);
                    Bitmap tile = img as Bitmap;
                    int px = 0, py = 0;
                    int tileY = y * tileSize;
                    int tileX = x * tileSize;
                    int totalx = tileX + tileSize;
                    int totaly = tileY + tileSize;
                    for (int ty = tileY; ty < totaly; ++ty)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        for (int tx = tileX; tx < totalx; ++tx)
                        {
                            metaImg.SetPixel(tx, ty, tile.GetPixel(px, py));
                            px++;
                        }
                        px = 0;
                        py++;
                    }
                    folderbits[0] = folderbits[1] = folderbits[2] = folderbits[3] = 0;
                    bitX++;
                    img.Dispose();
                    tile.Dispose();
                }
                bitX = 0;
                bitY++;
            }
            #endregion

            Bitmap result = new Bitmap(metaImg.Width + 7 * tileSize, metaImg.Height + 7 * tileSize);
            int originX1 = 3 * tileSize + halftile;
            int originY1 = 3 * tileSize + halftile;
            int originX2 = result.Width - 3 * tileSize - halftile - 1;
            int originY2 = result.Height - 3 * tileSize - halftile - 1;

            #region Determining Important Module
            bool openInfoModule = false;
            bool usingSpecialTile = false; // Info modules are replaced by special tile images.
            if (versionModule == "Normal" || versionModule == "SpecialTile")
            {
                openInfoModule = true;
                if (versionModule == "SpecialTile")
                    usingSpecialTile = true;
            }

            FLANN.functionList.Clear();
            bool blackByTile = false, whiteByTile = false;
            if (functionPattern == "WB")
            {
                blackByTile = true;
                whiteByTile = true;
            }
            else if (functionPattern == "W")
            {
                blackByTile = false;
                whiteByTile = true;
            }
            else if (functionPattern == "B")
            {
                blackByTile = true;
                whiteByTile = false;
            }

            if (blackByTile || usingSpecialTile)
            {
                if (FLANN.kdtree_Patterns_Black == null)
                {
                    if (space == "Lab")
                    {
                        Tile.ReadFileFunctionLab(PathConfig.FunctionBlackPath, 0);
                    }
                    else if (space == "RGB")
                    {
                        Tile.ReadFileFunctionRGB(PathConfig.FunctionBlackPath);
                    }
                    FLANN.kdtree_Patterns_Black = new Index(FLANN.FunctionBlackFeatures4x4, 5);
                }
            }

            if(whiteByTile || usingSpecialTile)
            {
                if (FLANN.kdtree_Patterns_White == null)
                {
                    if (space == "Lab")
                    {
                        Tile.ReadFileFunctionLab(PathConfig.FunctionWhitePath, 1);
                    }
                    else if (space == "RGB")
                    {
                        Tile.ReadFileFunctionRGB(PathConfig.FunctionWhitePath);
                    }
                    FLANN.kdtree_Patterns_White = new Index(FLANN.FunctionWhiteFeatures4x4, 5);
                }
            }

            if(blackByTile || whiteByTile)
            {
                FLANN.functionList.Clear();
            }
            #endregion

            #region Add Function Patterns

            for (int y = 0; y < QRMatrix.Height; ++y)
            {
                if (MainForm.singleton.isCancel) return null;
                //worker.ReportProgress(y * 10 / QRMatrix.Height + 70);
                worker.ReportProgress(y * 60 / QRMatrix.Height + 20);
                for (int x = 0; x < QRMatrix.Width; ++x)
                {
                    Bitmap module = new Bitmap(tileSize, tileSize);
                    int mx = 0, my = 0; // Module index
                    bool isBlack = false;
                    bool isWhite = false;
                    int startX = x * tileSize + halftile;
                    int startY = y * tileSize + halftile;
                    int endX = startX + tileSize;
                    int endY = startY + tileSize;
                    //int endX = x * tileSize + halftile + tileSize;
                    //int endY = y * tileSize + halftile + tileSize;
                    for (int i = startY; i < endY; ++i)
                    {
                        if (MainForm.singleton.isCancel) return null;
                        for (int j = startX; j < endX; ++j)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            #region Left Up Function Patterns
                            if (x < 8 && y < 8)
                            {
                                if (matrix[x + 1, y + 1]) //黑為1 白為0
                                {
                                    if (!blackByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!whiteByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                            #endregion

                            #region Info Module
                            
                            #region Timing
                            else if (y == 6 && x >= 8 && x < matrix.Width - 10 && openInfoModule)
                            {
                                if (matrix[x + 1, y + 1])
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                            else if (x == 6 && y >= 8 && y < matrix.Height - 10 && openInfoModule)
                            {
                                if (matrix[x + 1, y + 1])
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                            #endregion
                            #region Version
                            else if (y >= matrix.Height - 13 && y < matrix.Height - 10 && x >= 0 && x <= 5 && openInfoModule)
                            {
                                if (matrix[x + 1, y + 1])
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                            else if (x >= matrix.Width - 13 && x < matrix.Width - 10 && y >= 0 && y <= 5 && openInfoModule)
                            {
                                if (matrix[x + 1, y + 1])
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                            #endregion
                            #region Format
                            else if (((x == 8 && y < 8) || (y == 8 && x >= matrix.Width - 10) || (x == 8 && y >= matrix.Height - 10)) && openInfoModule)
                            {
                                if (matrix[x + 1, y + 1])
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!usingSpecialTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                            #endregion

                            #endregion

                            #region Right Up Function Patterns
                            else if (y < 8 && x >= matrix.Width - 10)
                            {
                                if (matrix[x + 1, y + 1]) //黑為1 白為0
                                {
                                    if (!blackByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!whiteByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                                #endregion

                            #region LeftDown Function Patterns
                            else if (x < 8 && y >= matrix.Height - 10)
                            {
                                if (matrix[x + 1, y + 1]) //黑為1 白為0
                                {
                                    if (!blackByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!whiteByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                                #endregion

                            #region AlignmentPattern
                            else if (version > 1 && j >= AlignmentPatternLocation_X + halftile && j < AlignmentPatternLocation_X + tileSize * 5 + halftile
                    && i >= AlignmentPatternLocation_Y + halftile && i < AlignmentPatternLocation_X + tileSize * 5 + halftile)
                            {
                                if (matrix[x + 1, y + 1]) //黑為1 白為0
                                {
                                    if (!blackByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.Black);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isBlack = true;
                                    }
                                }
                                else
                                {
                                    if (!whiteByTile)
                                    {
                                        metaImg.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        module.SetPixel(mx++, my, metaImg.GetPixel(j, i));
                                        isWhite = true;
                                    }
                                }
                            }
                                #endregion
                        }
                        if (blackByTile || whiteByTile || usingSpecialTile)
                        {
                            mx = 0;
                            my++;
                        }
                    }
                    if(isBlack)
                    {
                        ImportantModuleByTile1By1(metaImg, module, tileSize, startX, startY, endX, endY, space, PathConfig.FunctionBlackPath);
                    }
                    else if(isWhite)
                    {
                        ImportantModuleByTile1By1(metaImg, module, tileSize, startX, startY, endX, endY, space, PathConfig.FunctionWhitePath);
                    }
                    module.Dispose();
                }
            }
            
            #endregion
  
            #region Add Quiet Zone
            for (int m = 0; m < result.Height; ++m)
            {
                if (MainForm.singleton.isCancel) return null;
                worker.ReportProgress(m * 20 / result.Height + 80);

                for (int n = 0; n < result.Width; ++n)
                {
                    if (MainForm.singleton.isCancel) return null;
                    if (n >= originX1 + halftile && n <= originX2 - halftile && m >= originY1 + halftile && m <= originY2 - halftile)
                    //if (n >= originX1 && n <= originX2 && m >= originY1 && m <= originY2)
                    {
                        Color pixel = metaImg.GetPixel(n - originX1, m - originY1);
                        result.SetPixel(n, m, pixel);
                    }
                    else
                    {
                        result.SetPixel(n, m, Color.White);
                    }
                }
            }
            #endregion
            
            FLANN.usedDict.Clear();
            matrix.clear();
            metaImg.Dispose();
            newSrc.Dispose();
            loadingFolders.Clear();
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
                    Image img = Image.FromFile(tiles[index].Name);
                    candidateImg = img as Bitmap;
                    
                    for (int i = 0; i < tileSize; i++)
                    {
                        
                        for (int j = 0; j < tileSize; j ++)
                        {
                            if (MainForm.singleton.isCancel) return null;
                            //tryMethod(tiles[FLANN.Indices4x4.Data[currBlockIdx, tileIdx]], currBlockIdx, y, x, candidateImg, dst);
                            dst.SetPixel(j + x, i + y, candidateImg.GetPixel(j, i));
                        }
                    }
                    img.Dispose();
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
            GC.Collect();
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
