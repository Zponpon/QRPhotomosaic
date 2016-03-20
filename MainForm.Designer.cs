namespace QRPhotoMosaic
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.QRAndPhotmosaicBtn = new System.Windows.Forms.Button();
            this.QRPhotomosaicBtn = new System.Windows.Forms.Button();
            this.SaveImageBtn = new System.Windows.Forms.Button();
            this.LoadImageBtn = new System.Windows.Forms.Button();
            this.OutputPhotomosaicPicBox = new System.Windows.Forms.PictureBox();
            this.InputPicBox = new System.Windows.Forms.PictureBox();
            this.QRCodePhotomosaicPicBox = new System.Windows.Forms.PictureBox();
            this.QRCodePicBox = new System.Windows.Forms.PictureBox();
            this.QRCodeContentBox = new System.Windows.Forms.TextBox();
            this.InputQRCodeLabel = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.TilePicBox = new System.Windows.Forms.PictureBox();
            this.SrcPathLabel = new System.Windows.Forms.Label();
            this.StateLabel = new System.Windows.Forms.Label();
            this.CalcAvgBtn = new System.Windows.Forms.Button();
            this.TileFolderBtnAvg = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.EmbeddingWorker = new System.ComponentModel.BackgroundWorker();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputPhotomosaicPicBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputPicBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.QRCodePhotomosaicPicBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.QRCodePicBox)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TilePicBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Location = new System.Drawing.Point(1, 1);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1526, 860);
            this.tabControl.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.comboBox1);
            this.tabPage1.Controls.Add(this.QRAndPhotmosaicBtn);
            this.tabPage1.Controls.Add(this.QRPhotomosaicBtn);
            this.tabPage1.Controls.Add(this.SaveImageBtn);
            this.tabPage1.Controls.Add(this.LoadImageBtn);
            this.tabPage1.Controls.Add(this.OutputPhotomosaicPicBox);
            this.tabPage1.Controls.Add(this.InputPicBox);
            this.tabPage1.Controls.Add(this.QRCodePhotomosaicPicBox);
            this.tabPage1.Controls.Add(this.QRCodePicBox);
            this.tabPage1.Controls.Add(this.QRCodeContentBox);
            this.tabPage1.Controls.Add(this.InputQRCodeLabel);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1518, 834);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(303, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "label1";
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(308, 576);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(135, 31);
            this.comboBox1.TabIndex = 11;
            // 
            // QRAndPhotmosaicBtn
            // 
            this.QRAndPhotmosaicBtn.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QRAndPhotmosaicBtn.Location = new System.Drawing.Point(455, 731);
            this.QRAndPhotmosaicBtn.Name = "QRAndPhotmosaicBtn";
            this.QRAndPhotmosaicBtn.Size = new System.Drawing.Size(339, 72);
            this.QRAndPhotmosaicBtn.TabIndex = 10;
            this.QRAndPhotmosaicBtn.Text = "CreateQRCode and Photomosaic";
            this.QRAndPhotmosaicBtn.UseVisualStyleBackColor = true;
            this.QRAndPhotmosaicBtn.Click += new System.EventHandler(this.QRAndPhotmosaicBtn_Click);
            // 
            // QRPhotomosaicBtn
            // 
            this.QRPhotomosaicBtn.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QRPhotomosaicBtn.Location = new System.Drawing.Point(1069, 731);
            this.QRPhotomosaicBtn.Name = "QRPhotomosaicBtn";
            this.QRPhotomosaicBtn.Size = new System.Drawing.Size(211, 72);
            this.QRPhotomosaicBtn.TabIndex = 8;
            this.QRPhotomosaicBtn.Text = "Embedding";
            this.QRPhotomosaicBtn.UseVisualStyleBackColor = true;
            this.QRPhotomosaicBtn.Click += new System.EventHandler(this.QRPhotomosaicBtn_Click);
            // 
            // SaveImageBtn
            // 
            this.SaveImageBtn.Location = new System.Drawing.Point(1219, 576);
            this.SaveImageBtn.Name = "SaveImageBtn";
            this.SaveImageBtn.Size = new System.Drawing.Size(124, 41);
            this.SaveImageBtn.TabIndex = 7;
            this.SaveImageBtn.Text = "SaveImage";
            this.SaveImageBtn.UseVisualStyleBackColor = true;
            // 
            // LoadImageBtn
            // 
            this.LoadImageBtn.Location = new System.Drawing.Point(625, 576);
            this.LoadImageBtn.Name = "LoadImageBtn";
            this.LoadImageBtn.Size = new System.Drawing.Size(124, 41);
            this.LoadImageBtn.TabIndex = 6;
            this.LoadImageBtn.Text = "LoadImage";
            this.LoadImageBtn.UseVisualStyleBackColor = true;
            this.LoadImageBtn.Click += new System.EventHandler(this.LoadImageBtn_Click);
            // 
            // OutputPhotomosaicPicBox
            // 
            this.OutputPhotomosaicPicBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.OutputPhotomosaicPicBox.Location = new System.Drawing.Point(1000, 58);
            this.OutputPhotomosaicPicBox.Name = "OutputPhotomosaicPicBox";
            this.OutputPhotomosaicPicBox.Size = new System.Drawing.Size(500, 500);
            this.OutputPhotomosaicPicBox.TabIndex = 5;
            this.OutputPhotomosaicPicBox.TabStop = false;
            // 
            // InputPicBox
            // 
            this.InputPicBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.InputPicBox.Location = new System.Drawing.Point(442, 58);
            this.InputPicBox.Name = "InputPicBox";
            this.InputPicBox.Size = new System.Drawing.Size(500, 500);
            this.InputPicBox.TabIndex = 4;
            this.InputPicBox.TabStop = false;
            // 
            // QRCodePhotomosaicPicBox
            // 
            this.QRCodePhotomosaicPicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.QRCodePhotomosaicPicBox.Location = new System.Drawing.Point(38, 576);
            this.QRCodePhotomosaicPicBox.Name = "QRCodePhotomosaicPicBox";
            this.QRCodePhotomosaicPicBox.Size = new System.Drawing.Size(250, 250);
            this.QRCodePhotomosaicPicBox.TabIndex = 3;
            this.QRCodePhotomosaicPicBox.TabStop = false;
            // 
            // QRCodePicBox
            // 
            this.QRCodePicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.QRCodePicBox.Location = new System.Drawing.Point(38, 287);
            this.QRCodePicBox.Name = "QRCodePicBox";
            this.QRCodePicBox.Size = new System.Drawing.Size(250, 250);
            this.QRCodePicBox.TabIndex = 2;
            this.QRCodePicBox.TabStop = false;
            // 
            // QRCodeContentBox
            // 
            this.QRCodeContentBox.Location = new System.Drawing.Point(38, 58);
            this.QRCodeContentBox.Multiline = true;
            this.QRCodeContentBox.Name = "QRCodeContentBox";
            this.QRCodeContentBox.Size = new System.Drawing.Size(250, 200);
            this.QRCodeContentBox.TabIndex = 1;
            // 
            // InputQRCodeLabel
            // 
            this.InputQRCodeLabel.AutoSize = true;
            this.InputQRCodeLabel.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputQRCodeLabel.Location = new System.Drawing.Point(77, 32);
            this.InputQRCodeLabel.Name = "InputQRCodeLabel";
            this.InputQRCodeLabel.Size = new System.Drawing.Size(160, 23);
            this.InputQRCodeLabel.TabIndex = 0;
            this.InputQRCodeLabel.Text = "QR Code Content";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.numericUpDown1);
            this.tabPage2.Controls.Add(this.TilePicBox);
            this.tabPage2.Controls.Add(this.SrcPathLabel);
            this.tabPage2.Controls.Add(this.StateLabel);
            this.tabPage2.Controls.Add(this.CalcAvgBtn);
            this.tabPage2.Controls.Add(this.TileFolderBtnAvg);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1518, 834);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDown1.Location = new System.Drawing.Point(926, 485);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(140, 29);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown1.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // TilePicBox
            // 
            this.TilePicBox.Location = new System.Drawing.Point(233, 282);
            this.TilePicBox.Name = "TilePicBox";
            this.TilePicBox.Size = new System.Drawing.Size(221, 166);
            this.TilePicBox.TabIndex = 4;
            this.TilePicBox.TabStop = false;
            // 
            // SrcPathLabel
            // 
            this.SrcPathLabel.AutoSize = true;
            this.SrcPathLabel.Font = new System.Drawing.Font("Monaco", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SrcPathLabel.Location = new System.Drawing.Point(225, 224);
            this.SrcPathLabel.Name = "SrcPathLabel";
            this.SrcPathLabel.Size = new System.Drawing.Size(100, 45);
            this.SrcPathLabel.TabIndex = 3;
            this.SrcPathLabel.Text = "Path";
            this.SrcPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StateLabel
            // 
            this.StateLabel.AutoSize = true;
            this.StateLabel.Font = new System.Drawing.Font("Monaco", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StateLabel.Location = new System.Drawing.Point(225, 139);
            this.StateLabel.Name = "StateLabel";
            this.StateLabel.Size = new System.Drawing.Size(220, 45);
            this.StateLabel.TabIndex = 2;
            this.StateLabel.Text = "Statelabel";
            this.StateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CalcAvgBtn
            // 
            this.CalcAvgBtn.Location = new System.Drawing.Point(687, 468);
            this.CalcAvgBtn.Name = "CalcAvgBtn";
            this.CalcAvgBtn.Size = new System.Drawing.Size(207, 65);
            this.CalcAvgBtn.TabIndex = 1;
            this.CalcAvgBtn.Text = "AvgRGB";
            this.CalcAvgBtn.UseVisualStyleBackColor = true;
            this.CalcAvgBtn.Click += new System.EventHandler(this.CalcAvgBtn_Click);
            // 
            // TileFolderBtnAvg
            // 
            this.TileFolderBtnAvg.Location = new System.Drawing.Point(233, 465);
            this.TileFolderBtnAvg.Name = "TileFolderBtnAvg";
            this.TileFolderBtnAvg.Size = new System.Drawing.Size(223, 71);
            this.TileFolderBtnAvg.TabIndex = 0;
            this.TileFolderBtnAvg.Text = "ChooseTileFolder";
            this.TileFolderBtnAvg.UseVisualStyleBackColor = true;
            this.TileFolderBtnAvg.Click += new System.EventHandler(this.TileFolderBtn_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // EmbeddingWorker
            // 
            this.EmbeddingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.EmbeddingWorker_DoWork);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1523, 861);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load_1);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputPhotomosaicPicBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputPicBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.QRCodePhotomosaicPicBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.QRCodePicBox)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TilePicBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label InputQRCodeLabel;
        private System.Windows.Forms.TextBox QRCodeContentBox;
        private System.Windows.Forms.PictureBox QRCodePicBox;
        private System.Windows.Forms.PictureBox QRCodePhotomosaicPicBox;
        private System.Windows.Forms.Button LoadImageBtn;
        private System.Windows.Forms.PictureBox OutputPhotomosaicPicBox;
        private System.Windows.Forms.PictureBox InputPicBox;
        private System.Windows.Forms.Button SaveImageBtn;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button QRPhotomosaicBtn;
        private System.Windows.Forms.Button TileFolderBtnAvg;
        private System.Windows.Forms.Button CalcAvgBtn;
        public System.Windows.Forms.Label StateLabel;
        public System.Windows.Forms.Label SrcPathLabel;
        private System.Windows.Forms.PictureBox TilePicBox;
        private System.Windows.Forms.Button QRAndPhotmosaicBtn;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.ComponentModel.BackgroundWorker EmbeddingWorker;
    }
}

