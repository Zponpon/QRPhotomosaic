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
        public int version;
        public int blockSize;
        public int tileSize;

        private OpenFileDialog LoadingFile;
        private Image masterImg;
        private Bitmap masterBitmap;
        private Bitmap photomosaic;
        private Bitmap QRCode;
        private EmbeddingForm embedding;
        private CreatingQRAndPhotomosaic firstStep;
        private PhotoMosaic photoMosaic;
        private QRCodeEncoding QRWriter;
        private ProcessForm processForm;
        private Stopwatch stopWatch;
        private string CalcAvgFolderPath; // Page2
        private string CreatingFolderPath;// Page1
        
        //  TileProcessForm (Load & calc avg color) function register themselves into this callback function.
        private System.Action<BackgroundWorker, string> cb;
        

        public MainForm()
        {
            InitializeComponent();
            Init();
        }
        /// <summary>
        /// Initial the class.
        /// </summary>
        private void Init()
        {
            photoMosaic = new PhotoMosaic();
            QRWriter = new QRCodeEncoding();
            stopWatch = new Stopwatch();
            processForm = new ProcessForm();
            ProcessForm.main = this;
            PhotoMosaic.main = this;
            StateLabel.Text = SrcPathLabel.Text = "";

            // Calculate avg color of tile's size : 64, 32, 16, 8
            ProcessForm.calcTileSize = Convert.ToInt32(numericUpDown1.Value);
            //tileSize = Convert.ToInt32(numericUpDown2.Value);
            this.Text = "Photomosaic with embedded QR Code Application";
            QRCodeContentBox.Text = "Hello World!!!";
        }

        #region Our implementation
        #region Loading and Saving
        private void LoadImageBtn_Click(object sender, System.EventArgs e)
        {
            LoadingFile = new OpenFileDialog();
            if (LoadingFile.ShowDialog() == DialogResult.OK)
            {
                masterImg = Image.FromFile(LoadingFile.FileName);
                masterBitmap = new Bitmap(masterImg);
                //Bitmap inputPic = new Bitmap(masterImg, InputPicBox.Width, InputPicBox.Height);
                Bitmap inputPic = new Bitmap(masterImg, InputPicBox.Width, InputPicBox.Height);
                //inputPic = ImageProc.ScaleImage(inputPic, InputPicBox.Width);
                InputPicBox.Image = inputPic;

                String masterImgName = LoadingFile.FileName;
                int lastindex = masterImgName.LastIndexOf("\\");
                masterImgName = masterImgName.Substring(lastindex + 1);

                // Hash Code???

                Random randomPixel = new Random(10110101);
                for (int i = 0; i < 50; i++)
                {
                    masterImgName = masterImgName + masterBitmap.GetPixel(randomPixel.Next() % masterBitmap.Width, randomPixel.Next() % masterBitmap.Height).ToString();
                }

                //imageC.ImageHashCode = ImageName.GetHashCode().ToString();

                //masterBitmap.Dispose();
            }
        }
        private void SavingBtnBasic_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();
            System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
            photomosaic.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            fs.Close();
        }
        private void SaveImageBtn_Click(object sender, EventArgs e)
        {
            //dst = ImageProc.ScaleImage(dst, 4096);
            /*
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();
            System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
            photomosaic.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            fs.Close();
             */
        }
        #endregion

        #region Create Basic QR Code and Photomosaic
        /// <summary>
        /// At MainForm tab page1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(numericUpDown2.Value);
            if (val == 33)
                numericUpDown2.Value = 64;
            if (val == 63 || val == 17)
                numericUpDown2.Value = 32;
            if (val == 31 || val == 9)
                numericUpDown2.Value = 16;
            if (val == 15)
                numericUpDown2.Value = 8;
        }
        private void QRAndPhotmosaicBtn_Click(object sender, EventArgs e)
        {
            if (CreateWorker.IsBusy||masterBitmap == null) return;
            if (processForm == null)
                processForm = new ProcessForm();
            if (CreatingFolderPath != FolderComboBox.SelectedValue.ToString())
            {
                CreatingFolderPath = FolderComboBox.SelectedValue.ToString();
                tiles.Clear();
            }
            blockSize = Convert.ToInt32(BlockcomboBox.SelectedValue);
            version = Convert.ToInt32(numericUpDown3.Value);
            processForm.Show();
            stopWatch.Stop();
            CreateWorker.RunWorkerAsync();
        }
        private void CreateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                worker.ReportProgress(0);
                Tile.ReadFile(tiles, ref tileSize, CreatingFolderPath);
                //QRWriter.
                BitMatrix QRMatrix = QRWriter.CreateQR("AAA",QRWriter.GetECLevel("L"));
                QRCode = QRWriter.ToBitmap(QRMatrix);
                QRCodePicBox.Image = QRCode;
                worker.ReportProgress(10);
                photomosaic = photoMosaic.GenerateByNormalMethod(worker, masterBitmap, blockSize, tiles, tileSize, version);
                worker.ReportProgress(100);
                System.Threading.Thread.Sleep(500);
            }
        }
        private void CreateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processForm.setLabel1_Name = "In the process.....";
            processForm.ProgressValue = e.ProgressPercentage;
        }
        private void CreateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show("Canacel");
                stopWatch.Stop();
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error");
                stopWatch.Stop();
            }
            else
            {
                stopWatch.Stop();
                MessageBox.Show("Done");
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            }

            Bitmap photomosaicPicBox = new Bitmap(photomosaic, PhotomosaicPicBox.Width, PhotomosaicPicBox.Height);
            PhotomosaicPicBox.Image = photomosaicPicBox;
            processForm.Close();
            processForm.Dispose();
        }
        #endregion

        #region Embedding region
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
        #endregion

        #region Tile image region
        private void CalcAvgWorker_DoWork(object sender, DoWorkEventArgs e)
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
                cb(worker, processForm.stringCB());
                System.Threading.Thread.Sleep(500);
            }
        }

        private void CalcAvgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processForm.setLabel1_Name = "In the process.....";
            processForm.ProgressValue = e.ProgressPercentage;
        }

        private void CalcAvgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            SrcPathLabel.Text = CalcAvgFolderPath;
            processForm.Close();
            processForm.Dispose();
        }


        private void CancelTileBtn_Click(object sender, EventArgs e)
        {
            // The cancel button in the TileProcessForm.cs
            if (CalcAvgWorker.WorkerSupportsCancellation)
            {
                processForm.Close();
                processForm.Dispose();
            }
        }
        
        /// <summary>
        /// MainForm tab page 2.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TileFolderBtn_Click(object sender, EventArgs e)
        {
            if (CalcAvgWorker.IsBusy) return;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                processForm = new ProcessForm();
                processForm.Canceled += new EventHandler<EventArgs>(CancelTileBtn_Click);
                CalcAvgFolderPath = folderBrowser.SelectedPath;
                processForm.Show();
                cb -= processForm.LoadFolder;
                cb += processForm.LoadFolder;
                processForm.stringCB -= Path;
                processForm.stringCB += Path;
                stopWatch.Start();
                CalcAvgWorker.RunWorkerAsync();
                folderBrowser.Dispose();
            }
        }

        private void CalcAvgBtn_Click(object sender, EventArgs e)
        {
            if (CalcAvgWorker.IsBusy || tiles.Count == 0) return;

            processForm = new ProcessForm();
            processForm.Canceled += new EventHandler<EventArgs>(CancelTileBtn_Click);
            processForm.Show();
            // Register callback function
            cb -= processForm.CalcAvgRGB;
            cb += processForm.CalcAvgRGB;
            processForm.stringCB -= SavingFileName;
            processForm.stringCB += SavingFileName;
            stopWatch.Start();
            CalcAvgWorker.RunWorkerAsync();
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
            ProcessForm.calcTileSize = Convert.ToInt32(numericUpDown1.Value);
        }

        private string Path()
        {
            return CalcAvgFolderPath;
        }

        private string SavingFileName()
        {
            string fileName = ProcessForm.calcTileSize.ToString() + "AvgColor.txt";
            return Path() + fileName;
        }
        #endregion

        private void MainForm_Load_1(object sender, EventArgs e)
        {
            Tile.Init();
            PhotoMosaic.Init();
            FolderComboBox.DataSource = Tile.type;
            FolderComboBox.DisplayMember = "name";
            FolderComboBox.ValueMember = "folder";
            BlockcomboBox.DataSource = PhotoMosaic.blockList;
            BlockcomboBox.DisplayMember = "size";
            BlockcomboBox.ValueMember = "value";
        }
    }
}
