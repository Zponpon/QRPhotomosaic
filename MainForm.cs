using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Client.Result;
using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding.QrCode.Internal;
using ZxingForQRcodeHiding.QrCode;

using Version = ZxingForQRcodeHiding.QrCode.Internal.Version;
using QRPhotoMosaic.Method;

namespace QRPhotoMosaic
{
    public delegate string GetString();
    public partial class MainForm : Form
    {
        // For
        public List<Tile> tiles = new List<Tile>();
        /// <summary>
        /// Use at reading and writing.
        /// </summary>
        public int tileSize;
        private EmbeddingForm embedding;
        private PhotoMosaic photoMosaic;
        private TileProcessForm tileProcessForm;
        private Stopwatch stopWatch;
        private string folderPath;
        
        //  TileProcessForm (Load & calc avg color) function register themselves into this callback function.
        private System.Action<BackgroundWorker, string> cb;
        

        public MainForm()
        {
            InitializeComponent();
            InitClass();
            StateLabel.Text = SrcPathLabel.Text = "";
            tileSize = Convert.ToInt32(numericUpDown1.Value);
            this.Text = "Photomosaic with embedded QR Code Application";
            QRCodeContentBox.Text = "Hello World!!!";
        }
        /// <summary>
        /// Initial the class.
        /// </summary>
        private void InitClass()
        {
            photoMosaic = new PhotoMosaic();
            stopWatch = new Stopwatch();
            tileProcessForm = new TileProcessForm();
            TileProcessForm.main = this;
            PhotoMosaic.main = this;
        }
        #region Generate QR Code photomosaic region
        private void LoadImageBtn_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog fileName = new OpenFileDialog();
            if(fileName.ShowDialog() == DialogResult.OK)
            {
                Image masterImg = Image.FromFile(fileName.FileName);
                Bitmap masterBitmap = new Bitmap(masterImg);
                InputPicBox.Image = masterBitmap;
                
                String masterImgName = fileName.FileName;
                int lastindex = masterImgName.LastIndexOf("\\");
                masterImgName = masterImgName.Substring(lastindex + 1);

                Random randomPixel = new Random(10110101);
                for (int i = 0; i < 50; i++)
                {
                    masterImgName = masterImgName + masterBitmap.GetPixel(randomPixel.Next() % masterBitmap.Width, randomPixel.Next() % masterBitmap.Height).ToString();
                }

                //imageC.ImageHashCode = ImageName.GetHashCode().ToString();

                //masterBitmap.Dispose();
            }
        }

        private void QRAndPhotmosaicBtn_Click(object sender, EventArgs e)
        {
            if (InputPicBox.Image == null) return;

            string folder = comboBox1.SelectedValue.ToString();
            Tile.ReadFile(tiles, ref tileSize, folder);
        }

        private void EmbeddingWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void QRPhotomosaicBtn_Click(object sender, EventArgs e)
        {
            if (InputPicBox.Image != null)
            {
                //EmbeddingForm.photomosaicImg = InputPhotomosaicPicBox.Image as Bitmap;
                embedding.photomosaicImg = InputPicBox.Image as Bitmap;
            }
        }
        #endregion

        #region Tile image region
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            
            if(worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                worker.ReportProgress(0);
                cb(worker, tileProcessForm.stringCB());
                System.Threading.Thread.Sleep(500);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            tileProcessForm.setLabel1_Name = "In the process.....";
            tileProcessForm.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //label26.Text = "Canceled!";
                MessageBox.Show("Canacel");
                //StateLabel.Text = "Loading is canceled!!!";
                stopWatch.Stop();
            }
            else if (e.Error != null)
            {
                //label26.Text = "Error: " + e.Error.Message;
                MessageBox.Show("Error");
                //StateLabel.Text = "Loading is failed!!!";
                stopWatch.Stop();
            }
            else
            {
                stopWatch.Stop();
                MessageBox.Show("Done");
                //StateLabel.Text = "Loading is done!!!";
                TilePicBox.Image = tiles[0].bitmap;
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            }
            cb = null;
            stopWatch.Reset();
            SrcPathLabel.Text = folderPath;
            tileProcessForm.Close();
            tileProcessForm.Dispose();
        }


        private void CancelTileBtn_Click(object sender, EventArgs e)
        {
            // The cancel button in the TileProcessForm.cs
            if (backgroundWorker1.WorkerSupportsCancellation)
            {
                tileProcessForm.Close();
                tileProcessForm.Dispose();
            }
        }
        
        /// <summary>
        /// MainForm tab page 2.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TileFolderBtn_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy) return;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                tileProcessForm = new TileProcessForm();
                tileProcessForm.Canceled += new EventHandler<EventArgs>(CancelTileBtn_Click);
                folderPath = folderBrowser.SelectedPath;
                tileProcessForm.Show();
                cb -= tileProcessForm.LoadFolder;
                cb += tileProcessForm.LoadFolder;
                tileProcessForm.stringCB -= Path;
                tileProcessForm.stringCB += Path;
                stopWatch.Start();
                backgroundWorker1.RunWorkerAsync();
                folderBrowser.Dispose();
            }
        }

        private void CalcAvgBtn_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy || tiles.Count == 0) return;

            tileProcessForm = new TileProcessForm();
            tileProcessForm.Canceled += new EventHandler<EventArgs>(CancelTileBtn_Click);
            tileProcessForm.Show();
            // Register callback function
            cb -= tileProcessForm.CalcAvgRGB;
            cb += tileProcessForm.CalcAvgRGB;
            tileProcessForm.stringCB -= SavingFileName;
            tileProcessForm.stringCB += SavingFileName;
            stopWatch.Start();
            backgroundWorker1.RunWorkerAsync();
            StateLabel.Text = "Calculating is done!!!";
        }

        /// <summary>
        /// MainForm tab page 2.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(numericUpDown1.Value);
            if (val == 33)
                numericUpDown1.Value = 64;
            if (val == 63 || val == 17)
                numericUpDown1.Value = 32;
            if (val == 31 || val == 9)
                numericUpDown1.Value = 16;
            if (val == 15)
                numericUpDown1.Value = 8;
            tileSize = Convert.ToInt32(numericUpDown1.Value);
        }

        private string Path()
        {
            return folderPath;
        }

        private string SavingFileName()
        {
            return Path()+"AvgColor.txt";
        }
        #endregion

        private void MainForm_Load_1(object sender, EventArgs e)
        {
            Tile.Init();
            comboBox1.DataSource = Tile.type;
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "folder";
        }
    }
}
