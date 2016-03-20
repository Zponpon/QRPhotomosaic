using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRPhotoMosaic.Method;

namespace QRPhotoMosaic
{
    public partial class TileProcessForm : Form
    {
        public GetString stringCB;
        public static MainForm main;
        
        public string setLabel1_Name
        {
            set { label1.Text = value; }
        }

        public int ProgressValue
        {
            set { progressBar1.Value = value; }
        }

        public TileProcessForm()
        {
            InitializeComponent();
        }
        public event EventHandler<EventArgs> Canceled;

        public void LoadFolder(BackgroundWorker worker, string path)
        {
            //label1.Text = "Load Tiles from " + path;
            int total = System.IO.Directory.GetFiles(path).Length;
            foreach (string fileName in System.IO.Directory.GetFiles(path))
            {
                Image file = Image.FromFile(fileName);
                Tile tile = new Tile(fileName);
                main.tiles.Add(tile);
                worker.ReportProgress(main.tiles.Count / total * 100);
            }
        }

        /// <summary>
        /// Calculate the avg rgb of tile image 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="savingPath"></param>
        public void CalcAvgRGB(BackgroundWorker worker, string savingPath)
        {
            if (main.tiles.Count == 0) return;
            int t = 0;
            foreach (Tile tile in main.tiles)
            {
                tile.CalcNonDivTileAvgRGB(main.tileSize);
                worker.ReportProgress(++t / main.tiles.Count * 100);
            }
            Tile.SaveFile(main.tiles, main.tileSize, savingPath);
        }

        private void ProcessText_TextChanged(object sender, EventArgs e)
        {

        }

        private void TileProcessForm_Load(object sender, EventArgs e)
        {

        }

        private void CancelTileBtn_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ea = Canceled;
            if (ea != null)
                ea(this, e);
        }

    }
}
