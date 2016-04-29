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

        public static string avgtxt = "AvgColor.txt";

        public struct TileType
        {
            public string Name { get; set; }
            public string Folder { get; set; }
        }
        public static List<TileType> typeList;


        public static void Init()
        {
            typeList = new List<TileType>()
            {
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
                },
                new TileType
                {
                    Name="Data",
                    Folder ="..\\data"
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


        public void CalcNonDivTileAvgRGB(int s)
        {
            int r = 0, g = 0, b = 0;
            Bitmap tileImg = Image.FromFile(Name) as Bitmap;
            //if (s != bitmap.Width)
            //    bitmap = ImageProc.ScaleImage(bitmap, s);
            if (s != tileImg.Width)
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

        public static void ReadFile(List<Tile> tiles, out int tileSize, string folder)
        {
            if (tiles.Count != 0) tiles.Clear();
            tileSize = 0;
            try
            {
                int total = System.IO.Directory.GetFiles(folder).Length;
                string txtName = folder + avgtxt;
                FileStream file = File.Open(txtName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file);
                tileSize = Convert.ToInt32(reader.ReadByte());
                foreach (string tileName in System.IO.Directory.GetFiles(folder))
                {
                    //Image img = Image.FromFile(tileName);
                    Tile tile = new Tile(tileName);
                    tile.avgRGB.R = Convert.ToInt32(reader.ReadByte());
                    tile.avgRGB.G = Convert.ToInt32(reader.ReadByte());
                    tile.avgRGB.B = Convert.ToInt32(reader.ReadByte());

                    tiles.Add(tile);
                }
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
    }
}
