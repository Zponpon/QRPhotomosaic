﻿using System;
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
            set { progressBar1.Value = value; }
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
                //MainForm.singleton.embedding.info.QRmatrix;
                MainForm.singleton.info.QRmatrix
                    = QRCodeEncoder.CreateQR(MainForm.singleton.QRCodeContent, QRCodeEncoder.GetECLevel(ecLevel));
                MainForm.singleton.QRBitmap = QRCodeEncoder.ToBitmap(MainForm.singleton.info.QRmatrix);
                //MainForm.singleton.embedding.QRBitmap = QRCodeEncoder.ToBitmap(MainForm.singleton.embedding.info.QRmatrix);
                QRCodeReader reader = new QRCodeReader();
               // LuminanceSource source = new BitmapLuminanceSource(MainForm.singleton.embedding.QRBitmap);
                LuminanceSource source = new BitmapLuminanceSource(QRCodeEncoder.ToBitmap(MainForm.singleton.info.QRmatrix));
                BinaryBitmap binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));
                reader.decode(binaryBitmap);
                int version = reader.Version.VersionNumber;
                //MainForm.singleton.embedding.info.QRVersion = version;
                MainForm.singleton.info.QRVersion = version;

                if (check == "N")
                {
                    // Generate basic photomosaic
                    Tile.ReadFile(MainForm.singleton.tiles, out tileSize, MainForm.singleton.CreatingFolderPath);
                    if (tileSize == 0) return;

                    worker.ReportProgress(10);
                    //MainForm.singleton.embedding.tileSize = tileSize;
                    MainForm.singleton.tileSize = tileSize;

                    //MainForm.singleton.embedding.PhotomosaicImg
                    MainForm.singleton.photomosaicImg
                    = pmMethod.GenerateByNormalMethod(worker, MainForm.singleton.masterBitmap, MainForm.singleton.tiles, tileSize, version);

                    //worker.ReportProgress(100);

                }
                else
                {
                    MainForm.singleton.photomosaicImg = MainForm.singleton.masterBitmap;
                    //temp
                    MainForm.singleton.tileSize = 64;
                }
                worker.ReportProgress(100);
                System.Threading.Thread.Sleep(500);
            }
        }

        public void CreateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainForm.singleton.basicProcess.setLabel1_Name = "In the process.....";
            MainForm.singleton.basicProcess.ProgressValue = e.ProgressPercentage;
        }
        public void CreateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true || MainForm.singleton.isCancel)
            {
                MessageBox.Show("Canacel");
                MainForm.singleton.stopWatch.Stop();
                MainForm.singleton.isCancel = false;
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error");
                MainForm.singleton.stopWatch.Stop();
            }
            else
            {
                MainForm.singleton.stopWatch.Stop();
                MessageBox.Show("Done");
                TimeSpan ts = MainForm.singleton.stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            }

            if(MainForm.singleton.photomosaicImg != null)
            {
                int w = MainForm.singleton.PhotomosaicPictureBox.Width;
                int h = MainForm.singleton.PhotomosaicPictureBox.Height;
                MainForm.singleton.PhotoMosaicImage = ImageProc.ScaleImage(MainForm.singleton.photomosaicImg, w, h);

                w = MainForm.singleton.QRCodePictureBox.Width;
                h = MainForm.singleton.QRCodePictureBox.Height;
                MainForm.singleton.QRCodeImage = ImageProc.ScaleImage(MainForm.singleton.QRBitmap, w, h);
            }
            MainForm.singleton.stopWatch.Reset();
            Close();
            Dispose();
            GC.Collect();
        }
    }
}
