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
        public ColorSpace.RGB avg;
        public bool isSelected = false;

        public static string avgtxt = "AvgColor.txt";

        public struct TileType
        {
            public string name { get; set; }
            public string folder { get; set; }
        }
        public static List<TileType> type;

        public static void Init()
        {
            type = new List<TileType>()
            {
                new TileType
                {
                    name = "fleur",
                    folder = "..\\fleur",
                },
                new TileType
                {
                    name = "food",
                    folder = "..\\food"
                    //size = 
                }
            };
            //tileSize = 
        }

        public Tile()
        {

        }



        public Tile(string s)
        {
            bitmap = new Bitmap(s);
            avg = new ColorSpace.RGB();
        }


        public void CalcNonDivTileAvgRGB(int s)
        {
            int r = 0, g = 0, b = 0;
            if (s != bitmap.Width)
                bitmap = ImageProc.ScaleImage(bitmap, s);
            int size = bitmap.Height * bitmap.Height;
            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Height; ++x)
                {
                    r += (int)bitmap.GetPixel(x, y).R;
                    g += (int)bitmap.GetPixel(x, y).G;
                    b += (int)bitmap.GetPixel(x, y).B;
                }
            }
            r /= size;
            g /= size;
            b /= size;

            avg.R = r;
            avg.G = g;
            avg.B = b;
        }

        public static void ReadFile(List<Tile> tiles, ref int size, string folder)
        {
            if (tiles.Count != 0) tiles.Clear();
            try
            {
                int total = System.IO.Directory.GetFiles(folder).Length;
                string txtName = folder + avgtxt;
                FileStream file = File.Open(txtName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file);
                size = Convert.ToInt32(reader.ReadByte());
                foreach (string tileName in System.IO.Directory.GetFiles(folder))
                {
                    Image img = Image.FromFile(tileName);
                    Tile tile = new Tile(tileName);
                    tile.avg.R = Convert.ToInt32(reader.ReadByte());
                    //Console.WriteLine(tile.avg.R);
                    tile.avg.G = Convert.ToInt32(reader.ReadByte());
                    //Console.WriteLine(tile.avg.G);
                    tile.avg.B = Convert.ToInt32(reader.ReadByte());
                    //Console.WriteLine(tile.avg.B);

                    tiles.Add(tile);
                }
                file.Close();
                reader.Close();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void SaveFile(List<Tile> tiles, int size, string savingPath)
        {
            try
            {
                FileStream file = File.Open(savingPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(Convert.ToSByte(size));
                byte[] rgb = new byte[3];
                foreach (Tile tile in tiles)
                {
                    rgb[0] = Convert.ToByte(tile.avg.R);
                    rgb[1] = Convert.ToByte(tile.avg.G);
                    rgb[2] = Convert.ToByte(tile.avg.B);

                    writer.Write(rgb, 0, 3);
                }
                file.Close();
                writer.Close();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
