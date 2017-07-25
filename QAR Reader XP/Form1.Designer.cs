namespace QAR_Reader_XP
{
    partial class FormMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.labelAction = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.fileButt = new System.Windows.Forms.ToolStripDropDownButton();
            this.openFile = new System.Windows.Forms.ToolStripMenuItem();
            this.export = new System.Windows.Forms.ToolStripMenuItem();
            this.exportPartition = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportPartitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eraseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forciblyToolStripSplitButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.noForciblyStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.A320BitAppendWithRotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.a320CFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saab340ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.flightNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sectorNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.startCopyTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.endCopyTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.flightLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.exportWorker = new System.ComponentModel.BackgroundWorker();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.exportPartitionWorker = new System.ComponentModel.BackgroundWorker();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.erasePartitionWorker = new System.ComponentModel.BackgroundWorker();
            this.erasePartitionBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.statusStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.labelAction});
            this.statusStrip.Location = new System.Drawing.Point(0, 431);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(632, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // labelAction
            // 
            this.labelAction.Name = "labelAction";
            this.labelAction.Size = new System.Drawing.Size(74, 17);
            this.labelAction.Text = "current action";
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileButt,
            this.exportPartition,
            this.forciblyToolStripSplitButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(632, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip1";
            // 
            // fileButt
            // 
            this.fileButt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileButt.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFile,
            this.export});
            this.fileButt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileButt.Name = "fileButt";
            this.fileButt.Size = new System.Drawing.Size(36, 22);
            this.fileButt.Text = "File";
            // 
            // openFile
            // 
            this.openFile.Name = "openFile";
            this.openFile.Size = new System.Drawing.Size(133, 22);
            this.openFile.Text = "Open";
            this.openFile.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // export
            // 
            this.export.Name = "export";
            this.export.Size = new System.Drawing.Size(133, 22);
            this.export.Text = "Export flight";
            this.export.Click += new System.EventHandler(this.exportFlightToolStripMenuItem_Click);
            // 
            // exportPartition
            // 
            this.exportPartition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exportPartition.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportPartitionToolStripMenuItem,
            this.eraseToolStripMenuItem});
            this.exportPartition.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportPartition.Name = "exportPartition";
            this.exportPartition.Size = new System.Drawing.Size(60, 22);
            this.exportPartition.Text = "Partition";
            this.exportPartition.ToolTipText = "Partition";
            this.exportPartition.Visible = false;
            // 
            // exportPartitionToolStripMenuItem
            // 
            this.exportPartitionToolStripMenuItem.Name = "exportPartitionToolStripMenuItem";
            this.exportPartitionToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.exportPartitionToolStripMenuItem.Text = "Export";
            this.exportPartitionToolStripMenuItem.Click += new System.EventHandler(this.exportPartitionToolStripMenuItem_Click);
            // 
            // eraseToolStripMenuItem
            // 
            this.eraseToolStripMenuItem.Name = "eraseToolStripMenuItem";
            this.eraseToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.eraseToolStripMenuItem.Text = "Erase";
            this.eraseToolStripMenuItem.Click += new System.EventHandler(this.eraseToolStripMenuItem_Click);
            // 
            // forciblyToolStripSplitButton
            // 
            this.forciblyToolStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.forciblyToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noForciblyStripMenuItem,
            this.rawToolStripMenuItem,
            this.A320BitAppendWithRotationToolStripMenuItem,
            this.a320CFToolStripMenuItem,
            this.saab340ToolStripMenuItem});
            this.forciblyToolStripSplitButton.Image = ((System.Drawing.Image)(resources.GetObject("forciblyToolStripSplitButton.Image")));
            this.forciblyToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.forciblyToolStripSplitButton.Name = "forciblyToolStripSplitButton";
            this.forciblyToolStripSplitButton.Size = new System.Drawing.Size(57, 22);
            this.forciblyToolStripSplitButton.Text = "Forcibly";
            this.forciblyToolStripSplitButton.ToolTipText = "Raw transform";
            this.forciblyToolStripSplitButton.Visible = false;
            // 
            // noForciblyStripMenuItem
            // 
            this.noForciblyStripMenuItem.Checked = true;
            this.noForciblyStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.noForciblyStripMenuItem.Name = "noForciblyStripMenuItem";
            this.noForciblyStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.noForciblyStripMenuItem.Text = "No";
            this.noForciblyStripMenuItem.Click += new System.EventHandler(this.noForciblyStripMenuItem_Click);
            // 
            // rawToolStripMenuItem
            // 
            this.rawToolStripMenuItem.Name = "rawToolStripMenuItem";
            this.rawToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rawToolStripMenuItem.Text = "Raw";
            this.rawToolStripMenuItem.Click += new System.EventHandler(this.rawToolStripMenuItem_Click);
            // 
            // A320BitAppendWithRotationToolStripMenuItem
            // 
            this.A320BitAppendWithRotationToolStripMenuItem.Name = "A320BitAppendWithRotationToolStripMenuItem";
            this.A320BitAppendWithRotationToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.A320BitAppendWithRotationToolStripMenuItem.Text = "A320";
            this.A320BitAppendWithRotationToolStripMenuItem.Click += new System.EventHandler(this.A320BitAppendWithRotationToolStripMenuItem_Click);
            // 
            // a320CFToolStripMenuItem
            // 
            this.a320CFToolStripMenuItem.Name = "a320CFToolStripMenuItem";
            this.a320CFToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.a320CFToolStripMenuItem.Text = "A320CF";
            this.a320CFToolStripMenuItem.Click += new System.EventHandler(this.a320CFToolStripMenuItem_Click);
            // 
            // saab340ToolStripMenuItem
            // 
            this.saab340ToolStripMenuItem.Name = "saab340ToolStripMenuItem";
            this.saab340ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saab340ToolStripMenuItem.Text = "Saab340";
            this.saab340ToolStripMenuItem.Click += new System.EventHandler(this.saab340ToolStripMenuItem_Click);
            // 
            // dataGridView
            // 
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.flightNum,
            this.sectorNum,
            this.startCopyTime,
            this.endCopyTime,
            this.flightLength});
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 25);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.Size = new System.Drawing.Size(632, 406);
            this.dataGridView.TabIndex = 2;
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            // 
            // flightNum
            // 
            this.flightNum.HeaderText = "Flight num";
            this.flightNum.Name = "flightNum";
            this.flightNum.ReadOnly = true;
            this.flightNum.Width = 80;
            // 
            // sectorNum
            // 
            this.sectorNum.HeaderText = "Sector num";
            this.sectorNum.Name = "sectorNum";
            this.sectorNum.ReadOnly = true;
            // 
            // startCopyTime
            // 
            this.startCopyTime.HeaderText = "Start copy time";
            this.startCopyTime.Name = "startCopyTime";
            this.startCopyTime.ReadOnly = true;
            this.startCopyTime.Width = 150;
            // 
            // endCopyTime
            // 
            this.endCopyTime.HeaderText = "End copy time";
            this.endCopyTime.Name = "endCopyTime";
            this.endCopyTime.ReadOnly = true;
            this.endCopyTime.Width = 150;
            // 
            // flightLength
            // 
            this.flightLength.HeaderText = "Estimated flight length";
            this.flightLength.Name = "flightLength";
            this.flightLength.ReadOnly = true;
            // 
            // worker
            // 
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.worker_ProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
            // 
            // exportWorker
            // 
            this.exportWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.exportWorker_DoWork);
            this.exportWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.exportWorker_ProgressChanged);
            this.exportWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.exportWorker_RunWorkerCompleted);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // exportPartitionWorker
            // 
            this.exportPartitionWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.exportPartitionWorker_DoWork);
            this.exportPartitionWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.exportPartitionWorker_ProgressChanged);
            this.exportPartitionWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.exportPartitionWorker_RunWorkerCompleted);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // erasePartitionWorker
            // 
            this.erasePartitionWorker.WorkerReportsProgress = true;
            this.erasePartitionWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.erasePartitionWorker_DoWork);
            this.erasePartitionWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.erasePartitionWorker_ProgressChanged);
            this.erasePartitionWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.erasePartitionWorker_RunWorkerCompleted);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 453);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "QAR Reader";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.ToolStripStatusLabel labelAction;
        private System.ComponentModel.BackgroundWorker exportWorker;
        private System.Windows.Forms.DataGridViewTextBoxColumn flightNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn sectorNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn startCopyTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn endCopyTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn flightLength;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.ComponentModel.BackgroundWorker exportPartitionWorker;
        private System.Windows.Forms.ToolStripDropDownButton forciblyToolStripSplitButton;
        private System.Windows.Forms.ToolStripMenuItem noForciblyStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem A320BitAppendWithRotationToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton exportPartition;
        private System.Windows.Forms.ToolStripMenuItem exportPartitionToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton fileButt;
        private System.Windows.Forms.ToolStripMenuItem openFile;
        private System.Windows.Forms.ToolStripMenuItem export;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem eraseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem a320CFToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker erasePartitionWorker;
        private System.Windows.Forms.FolderBrowserDialog erasePartitionBrowserDialog;
        private System.Windows.Forms.ToolStripMenuItem saab340ToolStripMenuItem;
    }
}

