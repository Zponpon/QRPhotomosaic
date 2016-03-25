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
        public bool isCancel = false;
        
        public CreatingQRPhotomosaic mainMethod;
        public BasicMethod basicMethod;
        public List<Tile> tiles = new List<Tile>();
        public Bitmap masterBitmap;
        public BasicProcessForm basicProcess;
        public EmbeddingForm embedding;
        public static MainForm singleton;
        
        private OpenFileDialog loadingFile;
        
        
        
        public Stopwatch stopWatch;
        private int m_blockSize;
        private string CalcAvgFolderPath; // Page2
        public string CreatingFolderPath;// Page1
        
        //  TileProcessForm (Load & calc avg color) function register themselves into this callback function.
        public System.Action<BackgroundWorker, string> cb;

        #region GetAndSetRegion
        public string QRCodeContent
        {
            get
            {
                return this.QRCodeContentBox.Text;
            }
        }

        public PictureBox QRCodePictureBox
        {
            get
            {
                return QRCodePicBox;
            }
        }

        public PictureBox PhotomosaicPictureBox
        {
            get
            {
                return PhotomosaicPicBox;
            }
        }

        public string QRECLevel
        {
            get
            {
                return LevelComboBox.Text;
            }
        }

        public Image QRCodeImage
        {
            set
            {
                this.QRCodePicBox.Image = value;
            }
        }

        public Image PhotoMosaicImage
        {
            set
            {
                this.PhotomosaicPicBox.Image = value;
            }
        }

        public int blockSize
        {
            get
            {
                return m_blockSize;
            }
            protected set
            {
                m_blockSize = value;
            }
        }
#endregion

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
            LevelComboBox.SelectedIndex = 0;

            basicProcess = new BasicProcessForm();
            embedding = new EmbeddingForm();
            EventRegister();

            basicMethod = new BasicMethod();
            mainMethod = new CreatingQRPhotomosaic();

            stopWatch = new Stopwatch();
            StateLabel.Text = SrcPathLabel.Text = "";

            // Calculate avg color of tile's size : 64, 32, 16, 8
            BasicProcessForm.calcTileSize = Convert.ToInt32(numericUpDown1.Value);
            this.Text = "Photomosaic with embedded QR Code Application";
            QRCodeContentBox.Text = "Hello World!!!";
        }

        private void EventRegister()
        {
            CreateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(basicProcess.CreateWorker_DoWork);
            CreateWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(basicProcess.CreateWorker_ProgressChanged);
            CreateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(basicProcess.CreateWorker_RunWorkerCompleted);
            EmbeddingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(embedding.EmbeddingWorker_DoWork);
            EmbeddingWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(embedding.EmbeddingWorker_ProgressChanged);
            EmbeddingWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(embedding.EmbeddingWorker_RunWorkerCompleted);
        }

        #region Our implementation
        #region Loading and Saving
        private void LoadImageBtn_Click(object sender, System.EventArgs e)
        {
            loadingFile = new OpenFileDialog();
            if (loadingFile.ShowDialog() == DialogResult.OK)
            {
                Image masterImg = Image.FromFile(loadingFile.FileName);
                masterBitmap = new Bitmap(masterImg);
                Bitmap inputPic = new Bitmap(masterImg, InputPicBox.Width, InputPicBox.Height);
                InputPicBox.Image = inputPic;

                String masterImgName = loadingFile.FileName;
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
            basicProcess.PhotomosaicBitmap.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
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
            if (basicProcess.IsDisposed)
                basicProcess = new BasicProcessForm();
            if (mainMethod.infoOfQRCode == null)
                mainMethod.infoOfQRCode = new QRCodeInfo();
            if (CreatingFolderPath != FolderComboBox.SelectedValue.ToString())
            {
                CreatingFolderPath = FolderComboBox.SelectedValue.ToString();
                tiles.Clear();
            }
            basicProcess.Canceled -= CancelBtn_Click;
            basicProcess.Canceled += CancelBtn_Click;
            blockSize = Convert.ToInt32(BlockcomboBox.SelectedValue);
            basicProcess.Show();
            stopWatch.Stop();
            CreateWorker.RunWorkerAsync();
        }
        #endregion

        #region Embedding region
        
        private void QRPhotomosaicBtn_Click(object sender, EventArgs e)
        {
            if (PhotomosaicPicBox.Image == null || QRCodePicBox.Image == null || InputPicBox.Image == null)
                return;
            mainMethod.centerSize = Convert.ToInt32(this.CenterSizenumDown.Value);
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
                cb(worker, basicMethod.stringCB());
                System.Threading.Thread.Sleep(500);
            }
        }

        private void TileWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            basicProcess.setLabel1_Name = "In the process.....";
            basicProcess.ProgressValue = e.ProgressPercentage;
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
            basicProcess.Close();
            basicProcess.Dispose();
            GC.Collect();
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
                basicProcess = new BasicProcessForm();
                basicProcess.Canceled -= CancelBtn_Click;
                basicProcess.Canceled += CancelBtn_Click;
                CalcAvgFolderPath = folderBrowser.SelectedPath;
                basicProcess.Show();
                cb -= basicMethod.LoadFolder;
                cb += basicMethod.LoadFolder;
                basicMethod.stringCB -= Path;
                basicMethod.stringCB += Path;
                stopWatch.Start();
                TileWorker.RunWorkerAsync();
                folderBrowser.Dispose();
            }
        }

        private void CalcAvgBtn_Click(object sender, EventArgs e)
        {
            if (TileWorker.IsBusy || tiles.Count == 0) return;

            basicProcess = new BasicProcessForm();
            basicMethod = new BasicMethod();
            basicProcess.Canceled -= CancelBtn_Click;
            basicProcess.Canceled += CancelBtn_Click;
            basicProcess.Show();
            // Register callback function
            //cb -= basicProcess.CalcAvgRGB;
            //cb += basicProcess.CalcAvgRGB;
            cb -= basicMethod.CalcAvgRGB;
            cb += basicMethod.CalcAvgRGB;
            basicMethod.stringCB -= SavingFileName;
            basicMethod.stringCB += SavingFileName;
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
            BasicProcessForm.calcTileSize = Convert.ToInt32(numericUpDown1.Value);
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
            if (CreateWorker.WorkerSupportsCancellation && CreateWorker.IsBusy)
                CreateWorker.CancelAsync();
            if (EmbeddingWorker.WorkerSupportsCancellation && EmbeddingWorker.IsBusy)
                EmbeddingWorker.CancelAsync();
            isCancel = true;
            Clear();
            basicProcess.Close();
        }
    }
}
