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

namespace QRPhotoMosaic.Method
{
    class CreatingQRAndPhotomosaic
    {
        public int blockSize;
        public int tileSize;
        public string CreatingFolderPath;
        public BitMatrix QRMatrix;
        public PictureBox QRCodePicBox;
        public PictureBox PhotomosaicPicBox;
        public Stopwatch stopWatch;
        
        // The method of creating a photomosaic and QR Code
        private PhotoMosaic pmMethod;
        private QRCodeEncoding QRCodeEncoder;

        private Bitmap mQRCode;
        private Bitmap photomosaicBitmap;

        public Bitmap QRCode
        {
            get
            {
                return mQRCode;
            }
        }
        public Bitmap PhotomosaicBitmap
        {
            get
            {
                return photomosaicBitmap;
            }
        }

        public void Reset()
        {

        }

        public CreatingQRAndPhotomosaic()
        {
            pmMethod = new PhotoMosaic();
            QRCodeEncoder = new QRCodeEncoding();
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
                Tile.ReadFile(MainForm.singleton.tiles, ref tileSize, CreatingFolderPath);
                QRMatrix = QRCodeEncoder.CreateQR(MainForm.singleton.QRCodeContent, QRCodeEncoder.GetECLevel("L"));
                mQRCode = QRCodeEncoder.ToBitmap(QRMatrix);
                QRCodePicBox.Image = QRCode;
                worker.ReportProgress(10);
                photomosaicBitmap = pmMethod.GenerateByNormalMethod(worker, MainForm.singleton.masterBitmap, blockSize, MainForm.singleton.tiles, tileSize, 1);
                worker.ReportProgress(100);
                System.Threading.Thread.Sleep(500);
            }
        }

        public void CreateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainForm.singleton.processForm.setLabel1_Name = "In the process.....";
            MainForm.singleton.processForm.ProgressValue = e.ProgressPercentage;
        }
        public void CreateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

            Bitmap photomosaicPicBox = new Bitmap(photomosaicBitmap, PhotomosaicPicBox.Width, PhotomosaicPicBox.Height);
            PhotomosaicPicBox.Image = photomosaicPicBox;
            MainForm.singleton.processForm.Close();
            MainForm.singleton.processForm.Dispose();
        }
    }
}
