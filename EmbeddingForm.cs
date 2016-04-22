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
using ZxingForQRcodeHiding;
using ZxingForQRcodeHiding.Client.Result;
using ZxingForQRcodeHiding.Common;
using ZxingForQRcodeHiding.QrCode.Internal;
using ZxingForQRcodeHiding.QrCode;

namespace QRPhotoMosaic
{
    public partial class EmbeddingForm : Form
    {
        public CreatingQRPhotomosaic method;
        public QRCodeInfo info { set; get; }
        public Bitmap QRBitmap { set; get; }
        public Bitmap PhotomosaicImg { set; get; }
        public string ColorSpace { set; get; }
        public int? tileSize { set; get; }  //Get it at basicform
        public int? centerSize { set; get; }
        public int? robustVal { set; get; }
        public string shape;
        public string check;

        public EmbeddingForm()
        {
            InitializeComponent();
        }
        public void EmbeddingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                worker.ReportProgress(0, "Start");

                info.GetQRCodeInfo(info.QRmatrix, info.QRVersion);
                Bitmap result = method.Generate(worker, info, QRBitmap, PhotomosaicImg, tileSize, centerSize, robustVal, ColorSpace, shape, check);
                worker.ReportProgress(100, "It's done");
                

                MainForm.singleton.ResultPicBoxImg = ImageProc.ScaleImage(result, MainForm.singleton.ResultPicBox.Width, MainForm.singleton.ResultPicBox.Height);
                MainForm.singleton.result = new Bitmap(result);

                System.Threading.Thread.Sleep(500);
            }
        }
        public void EmbeddingWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.label1.Text = e.UserState.ToString();
            this.progressBar1.Value = e.ProgressPercentage;
        }
        public void EmbeddingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
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
                MainForm.singleton.StopWatch.Stop();
                MessageBox.Show("Done");
                TimeSpan ts = MainForm.singleton.StopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                MainForm.singleton.ProcessTimeText = elapsedTime;
            }
            MainForm.singleton.StopWatch.Reset();
 
            
            Close();
            Dispose();
            GC.Collect();
        }
    }
}
