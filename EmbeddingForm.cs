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
    public partial class EmbeddingForm : Form
    {
        public CreatingQRPhotomosaic method;
        public QRCodeInfo info { set; get; }
        public Bitmap QRBitmap { set; get; }
        public Bitmap photomosaicImg { set; get; }
        public string ColorSpace { set; get; }
        public int? tileSize { set; get; }  //Get it at basicform
        public int? centerSize { set; get; }
        public int? robustVal { set; get; }

        public EmbeddingForm()
        {
            InitializeComponent();
        }
        public void EmbeddingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //CreatingQRPhotomosaic method = MainForm.singleton.mainMethod;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                MainForm.singleton.isCancel = true;
                return;
            }
            else
            {
                worker.ReportProgress(0);
                info.GetQRCodeInfo(info.QRmatrix, info.QRVersion);
                Bitmap result = method.Generate(info, QRBitmap, photomosaicImg, tileSize, centerSize, robustVal, ColorSpace);
                worker.ReportProgress(100);
                MainForm.singleton.resultPicBoxImg = ImageProc.ScaleImage(result, MainForm.singleton.resultPicBox.Width, MainForm.singleton.resultPicBox.Height);
                MainForm.singleton.result = new Bitmap(result);
                System.Threading.Thread.Sleep(500);
            }
        }
        public void EmbeddingWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Console.Write(e.ProgressPercentage);
            this.progressBar1.Value = e.ProgressPercentage;
            //MainForm.singleton.basicProcess.setLabel1_Name = "In the process.....";
            //MainForm.singleton.basicProcess.ProgressValue = e.ProgressPercentage;
        }
        public void EmbeddingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainForm.singleton.stopWatch.Reset();
            this.Close();
            this.Dispose();
        }
    }
}
