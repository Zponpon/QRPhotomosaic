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
            this.CheckInputComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RobustValue = new System.Windows.Forms.NumericUpDown();
            this.ProcessTime = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ColorSpaceComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CenterSizenumDown = new System.Windows.Forms.NumericUpDown();
            this.LevelComboBox = new System.Windows.Forms.ComboBox();
            this.SaveMosaicBtn = new System.Windows.Forms.Button();
            this.BlockcomboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.FolderComboBox = new System.Windows.Forms.ComboBox();
            this.QRAndPhotmosaicBtn = new System.Windows.Forms.Button();
            this.QRPhotomosaicBtn = new System.Windows.Forms.Button();
            this.SaveResultBtn = new System.Windows.Forms.Button();
            this.LoadImageBtn = new System.Windows.Forms.Button();
            this.resultPicBox = new System.Windows.Forms.PictureBox();
            this.InputPicBox = new System.Windows.Forms.PictureBox();
            this.PhotomosaicPicBox = new System.Windows.Forms.PictureBox();
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
            this.TileWorker = new System.ComponentModel.BackgroundWorker();
            this.EmbeddingWorker = new System.ComponentModel.BackgroundWorker();
            this.CreateWorker = new System.ComponentModel.BackgroundWorker();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RobustValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CenterSizenumDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resultPicBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputPicBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PhotomosaicPicBox)).BeginInit();
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
            this.tabControl.Size = new System.Drawing.Size(1537, 921);
            this.tabControl.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.VersionLabel);
            this.tabPage1.Controls.Add(this.CheckInputComboBox);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.RobustValue);
            this.tabPage1.Controls.Add(this.ProcessTime);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.ColorSpaceComboBox);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.CenterSizenumDown);
            this.tabPage1.Controls.Add(this.LevelComboBox);
            this.tabPage1.Controls.Add(this.SaveMosaicBtn);
            this.tabPage1.Controls.Add(this.BlockcomboBox);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.FolderComboBox);
            this.tabPage1.Controls.Add(this.QRAndPhotmosaicBtn);
            this.tabPage1.Controls.Add(this.QRPhotomosaicBtn);
            this.tabPage1.Controls.Add(this.SaveResultBtn);
            this.tabPage1.Controls.Add(this.LoadImageBtn);
            this.tabPage1.Controls.Add(this.resultPicBox);
            this.tabPage1.Controls.Add(this.InputPicBox);
            this.tabPage1.Controls.Add(this.PhotomosaicPicBox);
            this.tabPage1.Controls.Add(this.QRCodePicBox);
            this.tabPage1.Controls.Add(this.QRCodeContentBox);
            this.tabPage1.Controls.Add(this.InputQRCodeLabel);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1529, 895);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // CheckInputComboBox
            // 
            this.CheckInputComboBox.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckInputComboBox.FormattingEnabled = true;
            this.CheckInputComboBox.Items.AddRange(new object[] {
            "N",
            "Y"});
            this.CheckInputComboBox.Location = new System.Drawing.Point(807, 576);
            this.CheckInputComboBox.Name = "CheckInputComboBox";
            this.CheckInputComboBox.Size = new System.Drawing.Size(135, 31);
            this.CheckInputComboBox.TabIndex = 29;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(301, 299);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(112, 21);
            this.label6.TabIndex = 28;
            this.label6.Text = "Robust Value";
            // 
            // RobustValue
            // 
            this.RobustValue.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RobustValue.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.RobustValue.Location = new System.Drawing.Point(297, 333);
            this.RobustValue.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.RobustValue.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.RobustValue.Name = "RobustValue";
            this.RobustValue.Size = new System.Drawing.Size(131, 29);
            this.RobustValue.TabIndex = 27;
            this.RobustValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.RobustValue.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // ProcessTime
            // 
            this.ProcessTime.AutoSize = true;
            this.ProcessTime.Location = new System.Drawing.Point(816, 816);
            this.ProcessTime.Name = "ProcessTime";
            this.ProcessTime.Size = new System.Drawing.Size(33, 12);
            this.ProcessTime.TabIndex = 26;
            this.ProcessTime.Text = "label6";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(317, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 21);
            this.label5.TabIndex = 25;
            this.label5.Text = "ColorSpace";
            // 
            // ColorSpaceComboBox
            // 
            this.ColorSpaceComboBox.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ColorSpaceComboBox.FormattingEnabled = true;
            this.ColorSpaceComboBox.Items.AddRange(new object[] {
            "RGB",
            "YUV",
            "Lab"});
            this.ColorSpaceComboBox.Location = new System.Drawing.Point(293, 152);
            this.ColorSpaceComboBox.Name = "ColorSpaceComboBox";
            this.ColorSpaceComboBox.Size = new System.Drawing.Size(135, 31);
            this.ColorSpaceComboBox.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(317, 196);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 21);
            this.label1.TabIndex = 23;
            this.label1.Text = "CenterSize";
            // 
            // CenterSizenumDown
            // 
            this.CenterSizenumDown.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.CenterSizenumDown.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.CenterSizenumDown.Location = new System.Drawing.Point(297, 220);
            this.CenterSizenumDown.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.CenterSizenumDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.CenterSizenumDown.Name = "CenterSizenumDown";
            this.CenterSizenumDown.Size = new System.Drawing.Size(131, 29);
            this.CenterSizenumDown.TabIndex = 22;
            this.CenterSizenumDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CenterSizenumDown.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // LevelComboBox
            // 
            this.LevelComboBox.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LevelComboBox.FormattingEnabled = true;
            this.LevelComboBox.Items.AddRange(new object[] {
            "L",
            "M",
            "Q",
            "H"});
            this.LevelComboBox.Location = new System.Drawing.Point(293, 73);
            this.LevelComboBox.Name = "LevelComboBox";
            this.LevelComboBox.Size = new System.Drawing.Size(135, 31);
            this.LevelComboBox.TabIndex = 21;
            // 
            // SaveMosaicBtn
            // 
            this.SaveMosaicBtn.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveMosaicBtn.Location = new System.Drawing.Point(23, 839);
            this.SaveMosaicBtn.Name = "SaveMosaicBtn";
            this.SaveMosaicBtn.Size = new System.Drawing.Size(281, 32);
            this.SaveMosaicBtn.TabIndex = 20;
            this.SaveMosaicBtn.Text = "Save Photomosaic";
            this.SaveMosaicBtn.UseVisualStyleBackColor = true;
            this.SaveMosaicBtn.Click += new System.EventHandler(this.SaveMosaicBtn_Click);
            // 
            // BlockcomboBox
            // 
            this.BlockcomboBox.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BlockcomboBox.FormattingEnabled = true;
            this.BlockcomboBox.Location = new System.Drawing.Point(305, 711);
            this.BlockcomboBox.Name = "BlockcomboBox";
            this.BlockcomboBox.Size = new System.Drawing.Size(135, 31);
            this.BlockcomboBox.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(308, 674);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 23);
            this.label3.TabIndex = 14;
            this.label3.Text = "Block Size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(308, 583);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 23);
            this.label2.TabIndex = 13;
            this.label2.Text = "Tile folder";
            // 
            // FolderComboBox
            // 
            this.FolderComboBox.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FolderComboBox.FormattingEnabled = true;
            this.FolderComboBox.Location = new System.Drawing.Point(305, 609);
            this.FolderComboBox.Name = "FolderComboBox";
            this.FolderComboBox.Size = new System.Drawing.Size(135, 31);
            this.FolderComboBox.TabIndex = 11;
            // 
            // QRAndPhotmosaicBtn
            // 
            this.QRAndPhotmosaicBtn.Font = new System.Drawing.Font("Monaco", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QRAndPhotmosaicBtn.Location = new System.Drawing.Point(558, 700);
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
            this.QRPhotomosaicBtn.Location = new System.Drawing.Point(1180, 700);
            this.QRPhotomosaicBtn.Name = "QRPhotomosaicBtn";
            this.QRPhotomosaicBtn.Size = new System.Drawing.Size(211, 72);
            this.QRPhotomosaicBtn.TabIndex = 8;
            this.QRPhotomosaicBtn.Text = "Embedding";
            this.QRPhotomosaicBtn.UseVisualStyleBackColor = true;
            this.QRPhotomosaicBtn.Click += new System.EventHandler(this.QRPhotomosaicBtn_Click);
            // 
            // SaveResultBtn
            // 
            this.SaveResultBtn.Location = new System.Drawing.Point(1219, 576);
            this.SaveResultBtn.Name = "SaveResultBtn";
            this.SaveResultBtn.Size = new System.Drawing.Size(124, 41);
            this.SaveResultBtn.TabIndex = 7;
            this.SaveResultBtn.Text = "SaveImage";
            this.SaveResultBtn.UseVisualStyleBackColor = true;
            this.SaveResultBtn.Click += new System.EventHandler(this.SaveResultBtn_Click);
            // 
            // LoadImageBtn
            // 
            this.LoadImageBtn.Location = new System.Drawing.Point(654, 576);
            this.LoadImageBtn.Name = "LoadImageBtn";
            this.LoadImageBtn.Size = new System.Drawing.Size(124, 41);
            this.LoadImageBtn.TabIndex = 6;
            this.LoadImageBtn.Text = "LoadImage";
            this.LoadImageBtn.UseVisualStyleBackColor = true;
            this.LoadImageBtn.Click += new System.EventHandler(this.LoadImageBtn_Click);
            // 
            // resultPicBox
            // 
            this.resultPicBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.resultPicBox.Location = new System.Drawing.Point(1000, 58);
            this.resultPicBox.Name = "resultPicBox";
            this.resultPicBox.Size = new System.Drawing.Size(500, 500);
            this.resultPicBox.TabIndex = 5;
            this.resultPicBox.TabStop = false;
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
            // PhotomosaicPicBox
            // 
            this.PhotomosaicPicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PhotomosaicPicBox.Location = new System.Drawing.Point(38, 553);
            this.PhotomosaicPicBox.Name = "PhotomosaicPicBox";
            this.PhotomosaicPicBox.Size = new System.Drawing.Size(250, 250);
            this.PhotomosaicPicBox.TabIndex = 3;
            this.PhotomosaicPicBox.TabStop = false;
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
            this.tabPage2.Size = new System.Drawing.Size(1529, 895);
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
            // TileWorker
            // 
            this.TileWorker.WorkerReportsProgress = true;
            this.TileWorker.WorkerSupportsCancellation = true;
            this.TileWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.TileWorker_DoWork);
            this.TileWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.TileWorker_ProgressChanged);
            this.TileWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.TileWorker_RunWorkerCompleted);
            // 
            // EmbeddingWorker
            // 
            this.EmbeddingWorker.WorkerReportsProgress = true;
            this.EmbeddingWorker.WorkerSupportsCancellation = true;
            // 
            // CreateWorker
            // 
            this.CreateWorker.WorkerReportsProgress = true;
            this.CreateWorker.WorkerSupportsCancellation = true;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.VersionLabel.Location = new System.Drawing.Point(301, 417);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(20, 21);
            this.VersionLabel.TabIndex = 30;
            this.VersionLabel.Text = "0";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1533, 924);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load_1);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RobustValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CenterSizenumDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resultPicBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputPicBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PhotomosaicPicBox)).EndInit();
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
        private System.Windows.Forms.PictureBox PhotomosaicPicBox;
        private System.Windows.Forms.Button LoadImageBtn;
        private System.Windows.Forms.PictureBox resultPicBox;
        private System.Windows.Forms.PictureBox InputPicBox;
        private System.Windows.Forms.Button SaveResultBtn;
        private System.ComponentModel.BackgroundWorker TileWorker;
        private System.Windows.Forms.Button QRPhotomosaicBtn;
        private System.Windows.Forms.Button TileFolderBtnAvg;
        private System.Windows.Forms.Button CalcAvgBtn;
        public System.Windows.Forms.Label StateLabel;
        public System.Windows.Forms.Label SrcPathLabel;
        private System.Windows.Forms.PictureBox TilePicBox;
        private System.Windows.Forms.Button QRAndPhotmosaicBtn;
        private System.Windows.Forms.ComboBox FolderComboBox;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.ComponentModel.BackgroundWorker EmbeddingWorker;
        private System.Windows.Forms.ComboBox BlockcomboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.ComponentModel.BackgroundWorker CreateWorker;
        private System.Windows.Forms.Button SaveMosaicBtn;
        private System.Windows.Forms.ComboBox LevelComboBox;
        private System.Windows.Forms.NumericUpDown CenterSizenumDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ColorSpaceComboBox;
        private System.Windows.Forms.Label ProcessTime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown RobustValue;
        private System.Windows.Forms.ComboBox CheckInputComboBox;
        private System.Windows.Forms.Label VersionLabel;
    }
}

