using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Client.Result;
using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding.QrCode.Internal;
using ZxingForQRcodeHiding.QrCode;
using QRPhotoMosaic.Method;

namespace QRPhotoMosaic
{
    public partial class BasicProcessForm : Form
    {
        public GetString stringCB;
        public static int calcTileSize;

        public int blockSize;
        public int tileSize;
        public string readFilePath;
        public string ecLevel;
        public string check;
        public string search;
        public string space;
        
        public PictureBox QRCodePicBox;
        public PictureBox PhotomosaicPicBox;
        
        // The method of creating a photomosaic and QR Code
        private PhotoMosaic pmMethod;
        private QRCodeEncoding QRCodeEncoder;
        
        //public bool isCancel = false;
        /*private BitMatrix QRMat;
        private Bitmap QRCodeBitmap;
        private Bitmap photomosaicBitmap;
        private int version;

        public BitMatrix QRMatrix
        {
            get
            {
                return QRMat;
            }
        }

       public Bitmap QRCode
        {
            get
            {
                return QRCodeBitmap;
            }
        }
        public Bitmap PhotomosaicBitmap
        {
            get
            {
                return photomosaicBitmap;
            }
        }
        */
        
        public string setLabel1_Name
        {
            set { label1.Text = value; }
        }

        public int ProgressValue
        {
            set 
            {
                if(value > 100)
                    progressBar1.Value = value % 100 + 1;
                else
                    progressBar1.Value = value;
            }
        }

        public BasicProcessForm()
        {
            InitializeComponent();
            pmMethod = new PhotoMosaic();
            QRCodeEncoder = new QRCodeEncoding();
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
                MainForm.singleton.tiles.Add(tile);
                worker.ReportProgress(MainForm.singleton.tiles.Count / total * 100);
            }
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
                tile.CalcNonDivTileAvgRGB(calcTileSize);
                if (MainForm.singleton.tiles.Count == 0) return;
                worker.ReportProgress((++t * 100) /  MainForm.singleton.tiles.Count);
            }
            Tile.SaveFile(MainForm.singleton.tiles, calcTileSize, savingPath);
        }

        private void BasicCancelBtn_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> ea = Canceled;
            if (ea != null)
                ea(this, e);
        }

        public void CreateWorker_DoWork(object sender, DoWorkEventArgs e)
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
                MainForm.singleton.info.QRmatrix
                    = QRCodeEncoder.CreateQR(MainForm.singleton.QRCodeContent, QRCodeEncoder.GetECLevel(ecLevel));
                MainForm.singleton.QRBitmap = QRCodeEncoder.ToBitmap(MainForm.singleton.info.QRmatrix);
                QRCodeReader reader = new QRCodeReader();
                LuminanceSource source = new BitmapLuminanceSource(QRCodeEncoder.ToBitmap(MainForm.singleton.info.QRmatrix));
                BinaryBitmap binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));
                reader.decode(binaryBitmap);
                int version = reader.Version.VersionNumber;
                MainForm.singleton.info.QRVersion = version;

                if (check == "N")
                {
                    // Generate basic photomosaic
                    MainForm.singleton.StopWatch.Start();
                    if (search == "Flann4x4" || search == "Full4x4")
                    {
                        if (space == "RGB" || space == "YUV")
                            Tile.ReadFile4x4(MainForm.singleton.tiles, MainForm.singleton.tileSize, MainForm.singleton.CreatingFolderPath);
                        else if (space == "Lab" || space == "Mid4x4")
                            Tile.ReadFile4x4Lab(MainForm.singleton.tiles, MainForm.singleton.tileSize, MainForm.singleton.CreatingFolderPath);
                        else
                            //Tile.ReadFile16Folder(PathConfig.ClassifyPathMin);
                            //Tile.ReadFile16Folder(PathConfig.ClassifyPathQuater);
                            //Tile.ReadFile16Folder(PathConfig.ClassifyPathHalf);
                            Tile.ReadFile16Folder(PathConfig.ClassifyPathHalfRobust);
                    }
                    else
                        Tile.ReadFile(MainForm.singleton.tiles, MainForm.singleton.tileSize, MainForm.singleton.CreatingFolderPath);
                    //if (tileSize == 0) return;

                    worker.ReportProgress(10);
                    

                    switch(search)
                    { 
                        case "Flann4x4":
                            //if (space == "RGB" || space == "YUV")
                            if(space == "Mid4x4")
                            {
                                MainForm.singleton.info.GetQRCodeInfo(MainForm.singleton.info.QRmatrix, MainForm.singleton.info.QRVersion);
                                MainForm.singleton.result
                                = pmMethod.GenerateByFlann4x4MidMod(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version, 10, space, MainForm.singleton.info.QRmatrix);
                                MainForm.singleton.ResultPicBoxImg = ImageProc.ScaleImage(MainForm.singleton.result, MainForm.singleton.ResultPicBox.Width, MainForm.singleton.ResultPicBox.Height);
                                int w = MainForm.singleton.QRCodePictureBox.Width;
                                int h = MainForm.singleton.QRCodePictureBox.Height;
                                MainForm.singleton.QRCodeImage = ImageProc.ScaleImage(MainForm.singleton.QRBitmap, w, h);
                            }
                            else if (space == "Other")
                            {
                                MainForm.singleton.info.GetQRCodeInfo(MainForm.singleton.info.QRmatrix, MainForm.singleton.info.QRVersion);
                                MainForm.singleton.result
                                = pmMethod.GenerateByFlannCombine(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version, 1000, space, MainForm.singleton.info.QRmatrix);
                                MainForm.singleton.ResultPicBoxImg = ImageProc.ScaleImage(MainForm.singleton.result, MainForm.singleton.ResultPicBox.Width, MainForm.singleton.ResultPicBox.Height);

                                int w = MainForm.singleton.QRCodePictureBox.Width;
                                int h = MainForm.singleton.QRCodePictureBox.Height;
                                MainForm.singleton.QRCodeImage = ImageProc.ScaleImage(MainForm.singleton.QRBitmap, w, h);
                            }
                            else
                                MainForm.singleton.photomosaicImg
                                = pmMethod.GenerateByFlann4x4(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version, 1000, "Lab");
                            //else if(space == "Lab")
                            //    MainForm.singleton.photomosaicImg
                            //= pmMethod.GenerateByFlann4x4(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version, 1000, "Lab");
                            break;
                        case "Full4x4":
                            if(space == "RGB" || space == "YUV")
                             MainForm.singleton.photomosaicImg
                            = pmMethod.GenerateByNormalMethod4x4(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version);
                            else if(space=="Lab")
                             MainForm.singleton.photomosaicImg
                           = pmMethod.GenerateByNormalMethod4x4Lab(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version);
                            break;
                        case "Flann":
                            MainForm.singleton.photomosaicImg
                            = pmMethod.GenerateByFlann(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version, 1000);
                            break;
                        case "Full":
                            MainForm.singleton.photomosaicImg
                            = pmMethod.GenerateByNormalMethod(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, MainForm.singleton.tileSize, version);
                            break;
                    }
                    MainForm.singleton.StopWatch.Stop();
                }
                else
                {
                    MainForm.singleton.photomosaicImg = MainForm.singleton.masterBitmap;
                    //temp
                    //MainForm.singleton.tileSize = 64;
                    //MainForm.singleton.tileSize = 128;
                }
                worker.ReportProgress(100);
                System.Threading.Thread.Sleep(500);
            }
        }

        public void CreateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.setLabel1_Name = "In the process.....";
            this.ProgressValue = e.ProgressPercentage;
        }

        public void CreateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string elapsedTime="";
            if (e.Cancelled == true || MainForm.singleton.isCancel)
            {
                MessageBox.Show("Canacel");
                MainForm.singleton.StopWatch.Stop();
                MainForm.singleton.isCancel = false;
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error");
                MainForm.singleton.StopWatch.Stop();
            }
            else
            {
                //MainForm.singleton.StopWatch.Stop();
                TimeSpan ts = MainForm.singleton.StopWatch.Elapsed;
                elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                MessageBox.Show("Done");
            }

            if(MainForm.singleton.photomosaicImg != null)
            {
                int w = MainForm.singleton.PhotomosaicPictureBox.Width;
                int h = MainForm.singleton.PhotomosaicPictureBox.Height;
                MainForm.singleton.PhotoMosaicImage = ImageProc.ScaleImage(MainForm.singleton.photomosaicImg, w, h);

                w = MainForm.singleton.QRCodePictureBox.Width;
                h = MainForm.singleton.QRCodePictureBox.Height;
                MainForm.singleton.QRCodeImage = ImageProc.ScaleImage(MainForm.singleton.QRBitmap, w, h);
                MainForm.singleton.VersionText = "Version: " + MainForm.singleton.info.QRVersion.ToString();
                MainForm.singleton.ProcessTimeText = elapsedTime;
            }
            MainForm.singleton.StopWatch.Reset();
            Close();
            Dispose();
            GC.Collect();
        }
    }
}
