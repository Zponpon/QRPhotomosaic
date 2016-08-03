using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Drawing;

namespace QRPhotoMosaic.Method
{
    public class BasicMethod
    {
        public static string avgtxt = "AvgColor.txt";
        public GetString stringCB;

        public void LoadFolder(BackgroundWorker worker, string path)
        {
            double total = (double)System.IO.Directory.GetFiles(path).Length;
            if (MainForm.singleton.tiles.Count > 0)
                MainForm.singleton.tiles.Clear();
            foreach (string fileName in System.IO.Directory.GetFiles(path))
            {
                Tile tile = new Tile(fileName);
                MainForm.singleton.tiles.Add(tile);
                worker.ReportProgress((int)((MainForm.singleton.tiles.Count / total) * 100));
            }
            worker.ReportProgress (0);
        }
        /// <summary>
        /// Calculate the avg rgb of tile image 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="savingPath"></param>
        public void CalcAvgRGB(BackgroundWorker worker, string savingPath)
        {
            if (MainForm.singleton.tiles.Count == 0) return;
            int t = 0;
            foreach (Tile tile in MainForm.singleton.tiles)
            {
                tile.CalcNonDivTileAvgRGB(BasicProcessForm.calcTileSize);
                if (MainForm.singleton.tiles.Count == 0) return;
                worker.ReportProgress((++t * 100)/ MainForm.singleton.tiles.Count );
            }
            Tile.SaveFile(MainForm.singleton.tiles, BasicProcessForm.calcTileSize, savingPath);
        }

        public void CalcAvgLab4x4(BackgroundWorker worker, string savingPath)
        {
            if (MainForm.singleton.tiles.Count == 0) return;
            int t = 0;
            foreach (Tile tile in MainForm.singleton.tiles)
            {
                tile.CalcNonDivTileAvgLab4x4(BasicProcessForm.calcTileSize);
                if (MainForm.singleton.tiles.Count == 0) return;
                worker.ReportProgress((++t * 100) / MainForm.singleton.tiles.Count);
            }
            Tile.SaveFile4x4Lab(MainForm.singleton.tiles, BasicProcessForm.calcTileSize, savingPath);
        }

        public void CalcAvgRGB4x4(BackgroundWorker worker, string savingPath)
        {
            if (MainForm.singleton.tiles.Count == 0) return;
            int t = 0;
            foreach (Tile tile in MainForm.singleton.tiles)
            {
                tile.CalcNonDivTileAvgRGB4x4(BasicProcessForm.calcTileSize);
                if (MainForm.singleton.tiles.Count == 0) return;
                worker.ReportProgress((++t * 100) / MainForm.singleton.tiles.Count);
            }
            Tile.SaveFile4x4(MainForm.singleton.tiles, BasicProcessForm.calcTileSize, savingPath);
        }

        public void ClassifyLab(BackgroundWorker worker, string savingPath)
        {

        }

        public static void ReadFile(List<Tile> tiles, ref int tileSize, string folder)
        {
            if (tiles.Count != 0) tiles.Clear();
            try
            {
                int total = System.IO.Directory.GetFiles(folder).Length;
                string txtName = folder + avgtxt;
                FileStream file = File.Open(txtName, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file);
                tileSize = Convert.ToInt32(reader.ReadByte());
                foreach (string tileName in System.IO.Directory.GetFiles(folder))
                {
                    Image img = Image.FromFile(tileName);
                    Tile tile = new Tile(tileName);
                    tile.avgRGB.R = Convert.ToInt32(reader.ReadByte());
                    tile.avgRGB.G = Convert.ToInt32(reader.ReadByte());
                    tile.avgRGB.B = Convert.ToInt32(reader.ReadByte());

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
                writer.Close();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
