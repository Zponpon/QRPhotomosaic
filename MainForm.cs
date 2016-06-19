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
        public static MainForm singleton;

        //  TileProcessForm (Load & calc avg color) function register themselves into this callback function.
        public System.Action<BackgroundWorker, string> tileCB;

        public bool isCancel = false;
        public List<Tile> tiles = new List<Tile>();
        public QRCodeInfo info;
        public Bitmap masterBitmap;
        public Bitmap QRBitmap;
        public Bitmap photomosaicImg;
        public Bitmap result;
        public int tileSize;
        public String masterImgName;

        private BasicProcessForm basicProcess;
        private EmbeddingForm embedding;
        private BasicMethod basicMethod;
        private int blockSize;
        private Stopwatch stopWatch;
        private string calcAvgFolderPath; // Page2
        private string creatingFolderPath;// Page1

        #region GetAndSetRegion
        public Stopwatch StopWatch
        {
            get
            {
                return stopWatch;
            }
        }

        public int TileSize
        {
            get
            {
                return Convert.ToInt32(TileSizecomboBox.SelectedValue);
            }
        }

        public string CreatingFolderPath
        {
            get { return creatingFolderPath; }
        }

        public string ProcessTimeText
        {
            set
            {
                this.ProcessTime.Text = value;
            }
        }

        public string QRCodeContent
        {
            get
            {
                return this.QRCodeContentBox.Text;
            }
        }

        public string VersionText
        {
            set
            {
                this.VersionLabel.Text = value;
            }
        }

        public PictureBox ResultPicBox
        {
            get
            {
                return this.resultPicBox;
            }
        }

        public Image ResultPicBoxImg
        {
            set
            {
                this.resultPicBox.Image = value;
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

        public int BlockSize
        {
            get
            {
                return blockSize;
            }
            private set
            {
                blockSize = value;
            }
        }

        public string Check
        {
            get
            {
                return CheckInputComboBox.SelectedText;
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
            SearchMethodComboBox.SelectedIndex = 0;
            //ColorSpaceComboBox.SelectedIndex = 1;
            CheckInputComboBox.SelectedIndex = 1;
            ShapeCombobox.SelectedIndex = 1;
            this.ProcessTime.Text = "";
            Console.Write(LevelComboBox.Text);
            basicProcess = new BasicProcessForm();
            CreateEventRegister();
            embedding = new EmbeddingForm();
            EmbeddingEventRegister();

            basicMethod = new BasicMethod();

            stopWatch = new Stopwatch();
            StateLabel.Text = SrcPathLabel.Text = "";

            // Calculate avg color of tile's size : 64, 32, 16, 8
            BasicProcessForm.calcTileSize = Convert.ToInt32(numericUpDown1.Value);
            this.Text = "Photomosaic with embedded QR Code Application";
            QRCodeContentBox.Text = "http://www.cse.yzu.edu.tw/";
        }

        private void EmbeddingEventRegister()
        {
            EmbeddingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(embedding.EmbeddingWorker_DoWork);
            EmbeddingWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(embedding.EmbeddingWorker_ProgressChanged);
            EmbeddingWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(embedding.EmbeddingWorker_RunWorkerCompleted);
        }

        private void CreateEventRegister()
        {
            CreateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(basicProcess.CreateWorker_DoWork);
            CreateWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(basicProcess.CreateWorker_ProgressChanged);
            CreateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(basicProcess.CreateWorker_RunWorkerCompleted);
        }

        private void EmbeddingEventCancel()
        {
            EmbeddingWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(embedding.EmbeddingWorker_DoWork);
            EmbeddingWorker.ProgressChanged -= new System.ComponentModel.ProgressChangedEventHandler(embedding.EmbeddingWorker_ProgressChanged);
            EmbeddingWorker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(embedding.EmbeddingWorker_RunWorkerCompleted);
        }

        private void CreateEventCancel()
        {
            CreateWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(basicProcess.CreateWorker_DoWork);
            CreateWorker.ProgressChanged -= new System.ComponentModel.ProgressChangedEventHandler(basicProcess.CreateWorker_ProgressChanged);
            CreateWorker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(basicProcess.CreateWorker_RunWorkerCompleted);
        }

        #region Our implementation
        #region Loading and Saving
        private void LoadImageBtn_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog loadingFile;
            loadingFile = new OpenFileDialog();
            if (loadingFile.ShowDialog() == DialogResult.OK)
            {
                Image masterImg = Image.FromFile(loadingFile.FileName);
                masterBitmap = new Bitmap(masterImg);
                Bitmap inputPic = new Bitmap(masterImg, InputPicBox.Width, InputPicBox.Height);
                InputPicBox.Image = inputPic;

                masterImgName = loadingFile.FileName;
                int lastindex = masterImgName.LastIndexOf("\\");
                masterImgName = masterImgName.Substring(lastindex + 1);
            }
        }
        private void SaveMosaicBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|JPeg Image|*.jpg";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName == "")
            {
                saveFileDialog.Dispose();
                GC.Collect();
                return;
            }
            System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
            Bitmap savePic = ImageProc.ScaleImage(photomosaicImg, photomosaicImg.Width, photomosaicImg.Height);
            savePic = ImageProc.OverlappingArea(savePic, savePic.Width - TileSize, savePic.Height - TileSize, TileSize);
            savePic.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            saveFileDialog.Dispose();
            fs.Close();
            GC.Collect();
        }
        private void SaveResultBtn_Click(object sender, EventArgs e)
        {
            if (result == null) return;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|JPeg Image|*.jpg";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName == "")
            {
                saveFileDialog.Dispose();
                GC.Collect();
                return;
            }
            System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
            Bitmap savePic = ImageProc.ScaleImage(result, result.Width, result.Height);
            savePic.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            saveFileDialog.Dispose();
            fs.Close();
            GC.Collect();
        }
        #endregion

        #region Create Basic QR Code and Photomosaic
        /// <summary>
        /// At MainForm tab page1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QRAndPhotmosaicBtn_Click(object sender, EventArgs e)
        {
            if (CreateWorker.IsBusy||masterBitmap == null) return;
            if (basicProcess.IsDisposed)
            {
                CreateEventCancel();
                basicProcess = new BasicProcessForm();
                CreateEventRegister();
            }
            if (embedding.info == null)
                info = new QRCodeInfo();
            if (creatingFolderPath != FolderComboBox.SelectedValue.ToString())
            {
                creatingFolderPath = FolderComboBox.SelectedValue.ToString();
                tiles.Clear();
            }
            basicProcess.Canceled -= CancelBtn_Click;
            basicProcess.Canceled += CancelBtn_Click;
            basicProcess.ecLevel = QRECLevel;
            basicProcess.search = SearchMethodComboBox.Text;
            basicProcess.check = this.CheckInputComboBox.Text;
            tileSize = TileSize;
            //BlockSize = Convert.ToInt32(BlockcomboBox.SelectedValue);
            BlockSize = 8;
            basicProcess.Show();
            stopWatch.Start();
            GC.Collect();
            CreateWorker.RunWorkerAsync();
        }
        #endregion

        #region Embedding region
        
        private void QRPhotomosaicBtn_Click(object sender, EventArgs e)
        {
            if (CreateWorker.IsBusy || EmbeddingWorker.IsBusy || TileWorker.IsBusy)
                return;
            if (PhotomosaicPicBox.Image == null || QRCodePicBox.Image == null || InputPicBox.Image == null)
                return;
            if (embedding.IsDisposed)
            {
                EmbeddingEventCancel();
                embedding = new EmbeddingForm();
                EmbeddingEventRegister();
            }

            //embedding.centerSize = Convert.ToInt32(this.CenterSizenumDown.Value);
            //embedding.robustVal = Convert.ToInt32(RobustValue.Value);
            embedding.centerSize = 26;
            embedding.robustVal = 64;
            embedding.minLum = Convert.ToDouble(this.MinLum.Value);
            embedding.maxLum = Convert.ToDouble(this.MaxLum.Value);
            
            //embedding.ColorSpace = ColorSpaceComboBox.Text;
            embedding.ColorSpace = "YUV";
            embedding.method = new CreatingQRPhotomosaic();
            embedding.info = info;
            embedding.QRBitmap = QRBitmap;
            embedding.PhotomosaicImg = photomosaicImg;
            //embedding.tileSize = tileSize;
            embedding.tileSize = TileSize;
            embedding.shape = ShapeCombobox.Text;
            embedding.check = CheckInputComboBox.Text;
            embedding.Show();
            stopWatch.Reset();
            this.ProcessTimeText = stopWatch.ElapsedMilliseconds.ToString();
            stopWatch.Start();
            GC.Collect();
            EmbeddingWorker.RunWorkerAsync();
        }
        #endregion
        #endregion

        #region Work of tile images region
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
                tileCB(worker, basicMethod.stringCB());
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
            if (e.Cancelled || isCancel)
            {
                isCancel = false;
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
                StateLabel.Text = "Calculating is done!!!";
                //TilePicBox.Image = tiles[0].bitmap;
                TilePicBox.Image = Image.FromFile(tiles[0].Name);
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                this.ProcessTimeText = elapsedTime;
            }
            tileCB = null;
            stopWatch.Reset();
            SrcPathLabel.Text = calcAvgFolderPath;
            basicProcess.Close();
            basicProcess.Dispose();
            GC.Collect();
        }
        
        // Read tile for calculating avgerage color.
        private void TileFolderBtn_Click(object sender, EventArgs e)
        {
            if (TileWorker.IsBusy || CreateWorker.IsBusy || EmbeddingWorker.IsBusy) return;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                basicProcess = new BasicProcessForm();
                basicProcess.Canceled -= CancelBtn_Click;
                basicProcess.Canceled += CancelBtn_Click;
                calcAvgFolderPath = folderBrowser.SelectedPath;
                basicProcess.Show();
                tileCB -= basicMethod.LoadFolder;
                tileCB += basicMethod.LoadFolder;
                basicMethod.stringCB -= Path;
                basicMethod.stringCB += Path;
                stopWatch.Start();
                folderBrowser.Dispose();
                GC.Collect();
                TileWorker.RunWorkerAsync();
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
            //tileCB -= basicMethod.CalcAvgRGB;
            //tileCB += basicMethod.CalcAvgRGB;
            tileCB -= basicMethod.CalcAvgRGB4x4;
            tileCB += basicMethod.CalcAvgRGB4x4;
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
            return calcAvgFolderPath;
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
            FolderComboBox.DataSource = Tile.typeList;
            FolderComboBox.DisplayMember = "name";
            FolderComboBox.ValueMember = "folder";
            TileSizecomboBox.DataSource = PhotoMosaic.tileList;
            TileSizecomboBox.DisplayMember = "Size";
            TileSizecomboBox.ValueMember = "Value";
            TileSizecomboBox.SelectedIndex = 0;
            //FolderComboBox.SelectedIndex = 3;
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
            basicProcess.Close();
            basicProcess.Dispose();
            GC.Collect();
        }

        private void SaveQRCodeBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|JPeg Image|*.jpg";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName == "")
            {
                saveFileDialog.Dispose();
                GC.Collect();
                return;
            }
            System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
            //this.QRCodePicBox
            Bitmap savePic = ImageProc.ScaleImage(QRCodePicBox.Image as Bitmap, QRCodePicBox.Width, QRCodePicBox.Height);
            //savePic = ImageProc.OverlappingArea(savePic, savePic.Width - tileSize, savePic.Height - tileSize, tileSize);
            savePic.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
            saveFileDialog.Dispose();
            fs.Close();
            GC.Collect();
        }

        private void DecodeBtn_Click(object sender, EventArgs e)
        {
            Bitmap img = InputPicBox.Image as Bitmap;
            //return;
            //Bitmap img = resultPicBox.Image as Bitmap;
            LuminanceSource source;
            source = new BitmapLuminanceSource(img);
            BinaryBitmap binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));
            
        }
    }
}
