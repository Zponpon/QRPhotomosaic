using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace QRPhotoMosaic.Method
{
    public class Tile
    {
        public Bitmap bitmap;
        public ColorSpace.RGB avgRGB;
        public bool isSelected = false;
        public string Name { get; set; }
        public int UseTimes { get; set; }

        //public List<ColorSpace.RGB> rgb4x4 = new List<ColorSpace.RGB>();
        public List<ColorSpace.RGB> rgb4x4 = new List<ColorSpace.RGB>();
        public static string avgtxt = "AvgColor.txt";
        public static string avgtxt4x4 = "AvgColor4x4.txt";

        public struct TileType
        {
            public string Name { get; set; }
            public string Folder { get; set; }
        }
        public static List<TileType> typeList;


        public static void Init()
        {
            typeList = new List<TileType>()
            {/*
                new TileType
                {
                    Name = "fleur",
                    Folder = "..\\fleur",
                },
                new TileType
                {
                    Name = "food",
                    Folder = "..\\food"
                },
                new TileType
                {
                    Name = "all",
                    Folder = "..\\all"
                },*/
                new TileType
                {
                    Name="Data",
                    Folder ="..\\data"
                },

                new TileType
                {
                    Name="Data2",
                    Folder ="..\\data2"
                },
                new TileType
                {
                    Name="fleur",
                    Folder ="..\\fleur"
                }
            };
        }

        public Tile()
        {

        }


        public Tile(string s)
        {
            Name = s;
            //bitmap = new Bitmap(s);
            avgRGB = new ColorSpace.RGB();
            UseTimes = 0;
        }

        public void CalcNonDivTileAvgRGB4x4(int s)
        {
            int r = 0, g = 0, b = 0;
            Bitmap tileImg = Image.FromFile(Name) as Bitmap;

            if (s != tileImg.Width || s != tileImg.Height)
                tileImg = ImageProc.ScaleImage(tileImg, s);

            int quater = tileImg.Height / 4;
            int dq = quater * quater;
            for (int y = 0; y < tileImg.Height; y += quater)
            {
                for (int x = 0; x < tileImg.Width; x += quater)
                {
                    for (int i = 0; i < quater; ++i)
                    {
                        for (int j = 0; j < quater; ++j)
                        {
                            r += (int)tileImg.GetPixel(x+j, y+i).R;
                            g += (int)tileImg.GetPixel(x+j, y+i).G;
                            b += (int)tileImg.GetPixel(x+j, y+i).B;
                        }
                    }
                    r /= dq;
                    g /= dq;
                    b /= dq;

                    ColorSpace.RGB avg;
                    avg.R = r;
                    avg.G = g;
                    avg.B = b;
                    rgb4x4.Add(avg);
                    r = g = b = 0;
                }
            }
            
            tileImg.Dispose();
        }


        public void CalcNonDivTileAvgRGB(int s)
        {
            int r = 0, g = 0, b = 0;
            Bitmap tileImg = Image.FromFile(Name) as Bitmap;
            //if (s != bitmap.Width)
            //    bitmap = ImageProc.ScaleImage(bitmap, s);
            if (s != tileImg.Width || s != tileImg.Height)
                tileImg = ImageProc.ScaleImage(tileImg, s);
            //int size = bitmap.Height * bitmap.Height;
            int size = tileImg.Height * tileImg.Height;
            for (int y = 0; y < tileImg.Height; ++y)
            {
                for (int x = 0; x < tileImg.Height; ++x)
                {
                    r += (int)tileImg.GetPixel(x, y).R;
                    g += (int)tileImg.GetPixel(x, y).G;
                    b += (int)tileImg.GetPixel(x, y).B;
                }
            }
            r /= size;
            g /= size;
            b /= size;

            avgRGB.R = r;
            avgRGB.G = g;
            avgRGB.B = b;
            tileImg.Dispose();
        }

        public static void ReadFile4x4(List<Tile> tiles, out int tileSize, string folder)
        {
            //if (tiles.Count != 0) tiles.Clear();
            //folder = "..\\data";
            tileSize = tiles.Count;
            if (tiles.Count != 0)
            {
                tiles.Clear();
            }

            try
            {
                int total = System.IO.Directory.GetFiles(folder).Length;
                string txtName = folder + avgtxt4x4;
                FileStream file = File.Open(txtName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file);
                tileSize = Convert.ToInt32(reader.ReadByte());

                //tileSize = 128;
                foreach (string tileName in System.IO.Directory.GetFiles(folder))
                {
                    //Image img = Image.FromFile(tileName);
                    Tile tile = new Tile(tileName);
                    //tile.rgb4x4
                    readtrans(tile, reader);
                    tiles.Add(tile);
                }

                FLANN.features4x4 = new Emgu.CV.Matrix<float>(tiles.Count, 48);
                transfeature(tiles);
                FLANN.kdtree = new Emgu.CV.Flann.Index(FLANN.features4x4, 5);
                file.Close();
                file.Dispose();
                reader.Close();
                reader.Dispose();
                GC.Collect();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void ReadFile(List<Tile> tiles, out int tileSize, string folder)
        {
            //if (tiles.Count != 0) tiles.Clear();
            //folder = "..\\data";
            tileSize = tiles.Count;
            if (tiles.Count != 0)
            {
                tiles.Clear();
            }

            try
            {
                int total = System.IO.Directory.GetFiles(folder).Length;
                string txtName = folder + avgtxt;
                FileStream file = File.Open(txtName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file);
                tileSize = Convert.ToInt32(reader.ReadByte());
                
                int index = 0;
                //tileSize = 128;
                foreach (string tileName in System.IO.Directory.GetFiles(folder))
                {
                    //Image img = Image.FromFile(tileName);
                    Tile tile = new Tile(tileName);
                    tile.avgRGB.R = Convert.ToInt32(reader.ReadByte());
                    tile.avgRGB.G = Convert.ToInt32(reader.ReadByte());
                    tile.avgRGB.B = Convert.ToInt32(reader.ReadByte());
                    
                    //FLANN.features.Data[index, 0] = (float)tile.avgRGB.R;
                    //FLANN.features.Data[index, 1] = (float)tile.avgRGB.G;
                    //FLANN.features.Data[index, 2] = (float)tile.avgRGB.B;
                    //index++;
                    tiles.Add(tile);
                }

                FLANN.features = new Emgu.CV.Matrix<float>(tiles.Count, 3);
                for (int i = 0; i < tiles.Count; ++i)
                {
                    FLANN.features.Data[i, 0] = (float)tiles[i].avgRGB.R;
                    FLANN.features.Data[i, 1] = (float)tiles[i].avgRGB.G;
                    FLANN.features.Data[i, 2] = (float)tiles[i].avgRGB.B;
                }
                FLANN.kdtree = new Emgu.CV.Flann.Index(FLANN.features, 5);
                file.Close();
                file.Dispose();
                reader.Close();
                reader.Dispose();
                GC.Collect();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SaveFile(List<Tile> tiles, int tileSize, string path)
        {
            try
            {
                FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(Convert.ToSByte(tileSize));
                byte[] rgb = new byte[3];
                foreach (Tile tile in tiles)
                {
                    rgb[0] = Convert.ToByte(tile.avgRGB.R);
                    rgb[1] = Convert.ToByte(tile.avgRGB.G);
                    rgb[2] = Convert.ToByte(tile.avgRGB.B);

                    writer.Write(rgb, 0, 3);
                }
                file.Close();
                file.Dispose();
                writer.Close();
                writer.Dispose();
                GC.Collect();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SaveFile4x4(List<Tile> tiles, int tileSize, string path)
        {
            try
            {
                FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(Convert.ToSByte(tileSize));
                
                foreach (Tile tile in tiles)
                {
                    trans(tile, writer);
                }
                file.Close();
                file.Dispose();
                writer.Close();
                writer.Dispose();
                GC.Collect();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void transfeature(List<Tile> tiles)
        {
            for (int i = 0; i < tiles.Count; ++i)
            {
                FLANN.features4x4.Data[i, 0] = (float)tiles[i].rgb4x4[0].R;
                FLANN.features4x4.Data[i, 1] = (float)tiles[i].rgb4x4[0].G;
                FLANN.features4x4.Data[i, 2] = (float)tiles[i].rgb4x4[0].B;

                FLANN.features4x4.Data[i, 3] = (float)tiles[i].rgb4x4[1].R;
                FLANN.features4x4.Data[i, 4] = (float)tiles[i].rgb4x4[1].G;
                FLANN.features4x4.Data[i, 5] = (float)tiles[i].rgb4x4[1].B;

                FLANN.features4x4.Data[i, 6] = (float)tiles[i].rgb4x4[2].R;
                FLANN.features4x4.Data[i, 7] = (float)tiles[i].rgb4x4[2].G;
                FLANN.features4x4.Data[i, 8] = (float)tiles[i].rgb4x4[2].B;

                FLANN.features4x4.Data[i, 9] =  (float)tiles[i].rgb4x4[3].R;
                FLANN.features4x4.Data[i, 10] = (float)tiles[i].rgb4x4[3].G;
                FLANN.features4x4.Data[i, 11] = (float)tiles[i].rgb4x4[3].B;

                FLANN.features4x4.Data[i, 12] = (float)tiles[i].rgb4x4[4].R;
                FLANN.features4x4.Data[i, 13] = (float)tiles[i].rgb4x4[4].G;
                FLANN.features4x4.Data[i, 14] = (float)tiles[i].rgb4x4[4].B;

                FLANN.features4x4.Data[i, 15] = (float)tiles[i].rgb4x4[5].R;
                FLANN.features4x4.Data[i, 16] = (float)tiles[i].rgb4x4[5].G;
                FLANN.features4x4.Data[i, 17] = (float)tiles[i].rgb4x4[5].B;

                FLANN.features4x4.Data[i, 18] = (float)tiles[i].rgb4x4[6].R;
                FLANN.features4x4.Data[i, 19] = (float)tiles[i].rgb4x4[6].G;
                FLANN.features4x4.Data[i, 20] = (float)tiles[i].rgb4x4[6].B;

                FLANN.features4x4.Data[i, 21] = (float)tiles[i].rgb4x4[7].R;
                FLANN.features4x4.Data[i, 22] = (float)tiles[i].rgb4x4[7].G;
                FLANN.features4x4.Data[i, 23] = (float)tiles[i].rgb4x4[7].B;

                FLANN.features4x4.Data[i, 24] = (float)tiles[i].rgb4x4[8].R;
                FLANN.features4x4.Data[i, 25] = (float)tiles[i].rgb4x4[8].G;
                FLANN.features4x4.Data[i, 26] = (float)tiles[i].rgb4x4[8].B;

                FLANN.features4x4.Data[i, 27] = (float)tiles[i].rgb4x4[9].R;
                FLANN.features4x4.Data[i, 28] = (float)tiles[i].rgb4x4[9].G;
                FLANN.features4x4.Data[i, 29] = (float)tiles[i].rgb4x4[9].B;

                FLANN.features4x4.Data[i, 30] = (float)tiles[i].rgb4x4[10].R;
                FLANN.features4x4.Data[i, 31] = (float)tiles[i].rgb4x4[10].G;
                FLANN.features4x4.Data[i, 32] = (float)tiles[i].rgb4x4[10].B;

                FLANN.features4x4.Data[i, 33] = (float)tiles[i].rgb4x4[11].R;
                FLANN.features4x4.Data[i, 34] = (float)tiles[i].rgb4x4[11].G;
                FLANN.features4x4.Data[i, 35] = (float)tiles[i].rgb4x4[11].B;

                FLANN.features4x4.Data[i, 36] = (float)tiles[i].rgb4x4[12].R;
                FLANN.features4x4.Data[i, 37] = (float)tiles[i].rgb4x4[12].G;
                FLANN.features4x4.Data[i, 38] = (float)tiles[i].rgb4x4[12].B;

                FLANN.features4x4.Data[i, 39] = (float)tiles[i].rgb4x4[13].R;
                FLANN.features4x4.Data[i, 40] = (float)tiles[i].rgb4x4[13].G;
                FLANN.features4x4.Data[i, 41] = (float)tiles[i].rgb4x4[13].B;

                FLANN.features4x4.Data[i, 42] = (float)tiles[i].rgb4x4[14].R;
                FLANN.features4x4.Data[i, 43] = (float)tiles[i].rgb4x4[14].G;
                FLANN.features4x4.Data[i, 44] = (float)tiles[i].rgb4x4[14].B;

                FLANN.features4x4.Data[i, 45] = (float)tiles[i].rgb4x4[15].R;
                FLANN.features4x4.Data[i, 46] = (float)tiles[i].rgb4x4[15].G;
                FLANN.features4x4.Data[i, 47] = (float)tiles[i].rgb4x4[15].B;
            }
        }

        private static void readtrans(Tile tile, BinaryReader reader)
        {
            ColorSpace.RGB rgb;
            
            //  0
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);

            //  1
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  2
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  3
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  4
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  5
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  6
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  7
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  8
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  9
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  10
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  11
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  12
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  13
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  14
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);


            //  15
            rgb.R = Convert.ToInt32(reader.ReadByte());
            rgb.G = Convert.ToInt32(reader.ReadByte());
            rgb.B = Convert.ToInt32(reader.ReadByte());
            tile.rgb4x4.Add(rgb);
        }

        private static void trans(Tile tile, BinaryWriter writer)
        {
            byte[] rgb = new byte[48];
            rgb[0] = Convert.ToByte(tile.rgb4x4[0].R);
            rgb[1] = Convert.ToByte(tile.rgb4x4[0].G);
            rgb[2] = Convert.ToByte(tile.rgb4x4[0].B);

            rgb[3] = Convert.ToByte(tile.rgb4x4[1].R);
            rgb[4] = Convert.ToByte(tile.rgb4x4[1].G);
            rgb[5] = Convert.ToByte(tile.rgb4x4[1].B);

            rgb[6] = Convert.ToByte(tile.rgb4x4[2].R);
            rgb[7] = Convert.ToByte(tile.rgb4x4[2].G);
            rgb[8] = Convert.ToByte(tile.rgb4x4[2].B);

            rgb[9]  = Convert.ToByte(tile.rgb4x4[3].R);
            rgb[10] = Convert.ToByte(tile.rgb4x4[3].G);
            rgb[11] = Convert.ToByte(tile.rgb4x4[3].B);

            rgb[12] = Convert.ToByte(tile.rgb4x4[4].R);
            rgb[13] = Convert.ToByte(tile.rgb4x4[4].G);
            rgb[14] = Convert.ToByte(tile.rgb4x4[4].B);

            rgb[15] = Convert.ToByte(tile.rgb4x4[5].R);
            rgb[16] = Convert.ToByte(tile.rgb4x4[5].G);
            rgb[17] = Convert.ToByte(tile.rgb4x4[5].B);

            rgb[18] = Convert.ToByte(tile.rgb4x4[6].R);
            rgb[19] = Convert.ToByte(tile.rgb4x4[6].G);
            rgb[20] = Convert.ToByte(tile.rgb4x4[6].B);

            rgb[21] = Convert.ToByte(tile.rgb4x4[7].R);
            rgb[22] = Convert.ToByte(tile.rgb4x4[7].G);
            rgb[23] = Convert.ToByte(tile.rgb4x4[7].B);

            rgb[24] = Convert.ToByte(tile.rgb4x4[8].R);
            rgb[25] = Convert.ToByte(tile.rgb4x4[8].G);
            rgb[26] = Convert.ToByte(tile.rgb4x4[8].B);

            rgb[27] = Convert.ToByte(tile.rgb4x4[9].R);
            rgb[28] = Convert.ToByte(tile.rgb4x4[9].G);
            rgb[29] = Convert.ToByte(tile.rgb4x4[9].B);

            rgb[30] = Convert.ToByte(tile.rgb4x4[10].R);
            rgb[31] = Convert.ToByte(tile.rgb4x4[10].G);
            rgb[32] = Convert.ToByte(tile.rgb4x4[10].B);

            rgb[33] = Convert.ToByte(tile.rgb4x4[11].R);
            rgb[34] = Convert.ToByte(tile.rgb4x4[11].G);
            rgb[35] = Convert.ToByte(tile.rgb4x4[11].B);

            rgb[36] = Convert.ToByte(tile.rgb4x4[12].R);
            rgb[37] = Convert.ToByte(tile.rgb4x4[12].G);
            rgb[38] = Convert.ToByte(tile.rgb4x4[12].B);

            rgb[39] = Convert.ToByte(tile.rgb4x4[13].R);
            rgb[40] = Convert.ToByte(tile.rgb4x4[13].G);
            rgb[41] = Convert.ToByte(tile.rgb4x4[13].B);

            rgb[42] = Convert.ToByte(tile.rgb4x4[14].R);
            rgb[43] = Convert.ToByte(tile.rgb4x4[14].G);
            rgb[44] = Convert.ToByte(tile.rgb4x4[14].B);

            rgb[45] = Convert.ToByte(tile.rgb4x4[15].R);
            rgb[46] = Convert.ToByte(tile.rgb4x4[15].G);
            rgb[47] = Convert.ToByte(tile.rgb4x4[15].B);

            writer.Write(rgb, 0, 48);
        }
    }
}
