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
        private CreatingQRAndPhotomosaic BasicStep;
        //public int version;
        //public int blockSize;
        //public int tileSize;
        public List<Tile> tiles = new List<Tile>();
        public Bitmap masterBitmap;
        public ProcessForm processForm;
        private QRCodeInfo infoOfQRCode;
        public static MainForm singleton;
        
        private OpenFileDialog LoadingFile;
        private bool isCancel = false;
        //private Image masterImg;
        
        private EmbeddingForm embeddingForm;
        
        
        private Stopwatch stopWatch;
        private string CalcAvgFolderPath; // Page2
        private string CreatingFolderPath;// Page1
        
        //  TileProcessForm (Load & calc avg color) function register themselves into this callback function.
        public System.Action<BackgroundWorker, string> cb;

        public string QRCodeContent
        {
            get
            {
                return this.QRCodeContentBox.Text;
            }
        }

        public string QRECLevel
        {
            get
            {
                return this.LevelComboBox.Text;
            }
        }
        

        public MainForm()
        {
            InitializeComponent();
            Init();
            singleton = this;
            
        }
        /// <summary>
        /// Initial the class.
        /// </summary>
        private void Init()
        {
            stopWatch = new Stopwatch();
            processForm = new ProcessForm();
            infoOfQRCode = new QRCodeInfo();
            ProcessForm.main = this;
            PhotoMosaic.main = this;
            LevelComboBox.SelectedIndex = 0;
            this.BasicStep = new CreatingQRAndPhotomosaic();
            CreateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(BasicStep.CreateWorker_DoWork);
            CreateWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(BasicStep.CreateWorker_ProgressChanged);
            CreateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(BasicStep.CreateWorker_RunWorkerCompleted);
            BasicStep.QRCodePicBox = QRCodePicBox;
            BasicStep.PhotomosaicPicBox = PhotomosaicPicBox;
            BasicStep.stopWatch = stopWatch;
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
                Image masterImg = Image.FromFile(LoadingFile.FileName);
                masterBitmap = new Bitmap(masterImg);
                Bitmap inputPic = new Bitmap(masterImg, InputPicBox.Width, InputPicBox.Height);
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
            BasicStep.PhotomosaicBitmap.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
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
            if (processForm.IsDisposed)
                processForm = new ProcessForm();
            if (CreatingFolderPath != FolderComboBox.SelectedValue.ToString())
            {
                CreatingFolderPath = FolderComboBox.SelectedValue.ToString();
                tiles.Clear();
            }
            processForm.Canceled -= CancelBtn_Click;
            processForm.Canceled += CancelBtn_Click;
            BasicStep.blockSize = Convert.ToInt32(BlockcomboBox.SelectedValue);
            BasicStep.CreatingFolderPath = CreatingFolderPath;
            //version = Convert.ToInt32(numericUpDown3.Value);
            processForm.Show();
            stopWatch.Stop();
            CreateWorker.RunWorkerAsync();
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
            }
        }
        #endregion
        #endregion

        #region Tile image region
        private void TileWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void TileWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processForm.setLabel1_Name = "In the process.....";
            processForm.ProgressValue = e.ProgressPercentage;
        }

        private void TileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //BackgroundWorker w = sender as BackgroundWorker;
            if (e.Cancelled || isCancel)
            {
                //label26.Text = "Canceled!";
                isCancel = false;
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
                StateLabel.Text = "Calculating is done!!!";
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
        
        /// <summary>
        /// MainForm tab page 2.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TileFolderBtn_Click(object sender, EventArgs e)
        {
            if (TileWorker.IsBusy) return;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                processForm = new ProcessForm();
                processForm.Canceled -= CancelBtn_Click;
                processForm.Canceled += CancelBtn_Click;
                CalcAvgFolderPath = folderBrowser.SelectedPath;
                processForm.Show();
                cb -= processForm.LoadFolder;
                cb += processForm.LoadFolder;
                processForm.stringCB -= Path;
                processForm.stringCB += Path;
                stopWatch.Start();
                TileWorker.RunWorkerAsync();
                folderBrowser.Dispose();
            }
        }

        private void CalcAvgBtn_Click(object sender, EventArgs e)
        {
            if (TileWorker.IsBusy || tiles.Count == 0) return;

            processForm = new ProcessForm();
            processForm.Canceled -= CancelBtn_Click;
            processForm.Canceled += CancelBtn_Click;
            processForm.Show();
            // Register callback function
            cb -= processForm.CalcAvgRGB;
            cb += processForm.CalcAvgRGB;
            processForm.stringCB -= SavingFileName;
            processForm.stringCB += SavingFileName;
            stopWatch.Start();
            TileWorker.RunWorkerAsync();
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
            string fileName = "AvgColor.txt";
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

        private void Clear()
        {
            tiles.Clear();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            if (TileWorker.WorkerSupportsCancellation && TileWorker.IsBusy)
                TileWorker.CancelAsync();
            if (CreateWorker.WorkerSupportsCancellation)
                CreateWorker.CancelAsync();
            if (EmbeddingWorker.WorkerSupportsCancellation)
                EmbeddingWorker.CancelAsync();
            isCancel = true;
            Clear();
            processForm.Close();
        }
    }
}
