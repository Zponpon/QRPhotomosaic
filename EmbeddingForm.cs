using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QRPhotoMosaic
{
    public partial class EmbeddingForm : Form
    {
        public EmbeddingForm()
        {
            InitializeComponent();
        }
        public Bitmap masterImg { set; get; }
        public Bitmap photomosaicImg { set; get; }
        public Bitmap QRCode { set; get; }
        public String QRcodeContent { set; get; }
        public String ColorSpace { set; get; }
        public int tileSize { set; get; }
        public int centerSize { set; get; }
        public int robustVal { set; get; }
    }
}
