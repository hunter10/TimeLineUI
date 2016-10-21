namespace TimeLineUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dGrid_TimeLineObj = new System.Windows.Forms.DataGridView();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Itemlock = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.itemView = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnGoFirst = new System.Windows.Forms.Button();
            this.btnOneStepPrev = new System.Windows.Forms.Button();
            this.btnPlayStop = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnOneStepNext = new System.Windows.Forms.Button();
            this.btnGoLast = new System.Windows.Forms.Button();
            this.btnGoReverse = new System.Windows.Forms.Button();
            this.picBox_Lock = new System.Windows.Forms.PictureBox();
            this.picBox_View = new System.Windows.Forms.PictureBox();
            this.panel_TimeEdit = new System.Windows.Forms.Panel();
            this.picBox_TimeEdit = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnObjAdd = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replayAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid_TimeLineObj)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_Lock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_View)).BeginInit();
            this.panel_TimeEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_TimeEdit)).BeginInit();
            this.panel2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dGrid_TimeLineObj
            // 
            this.dGrid_TimeLineObj.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dGrid_TimeLineObj.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGrid_TimeLineObj.ColumnHeadersVisible = false;
            this.dGrid_TimeLineObj.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.Itemlock,
            this.itemView});
            this.dGrid_TimeLineObj.Location = new System.Drawing.Point(0, 36);
            this.dGrid_TimeLineObj.MultiSelect = false;
            this.dGrid_TimeLineObj.Name = "dGrid_TimeLineObj";
            this.dGrid_TimeLineObj.RowHeadersVisible = false;
            this.dGrid_TimeLineObj.RowTemplate.Height = 23;
            this.dGrid_TimeLineObj.Size = new System.Drawing.Size(353, 202);
            this.dGrid_TimeLineObj.TabIndex = 0;
            this.dGrid_TimeLineObj.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dGrid_TimeLineObj_CellValueChanged);
            this.dGrid_TimeLineObj.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dGrid_TimeLineObj_Scroll);
            this.dGrid_TimeLineObj.SelectionChanged += new System.EventHandler(this.dGrid_TimeLineObj_SelectionChanged);
            // 
            // name
            // 
            this.name.HeaderText = "Name";
            this.name.Name = "name";
            this.name.Width = 250;
            // 
            // Itemlock
            // 
            this.Itemlock.HeaderText = "Lock";
            this.Itemlock.Name = "Itemlock";
            this.Itemlock.Width = 50;
            // 
            // itemView
            // 
            this.itemView.HeaderText = "View";
            this.itemView.Name = "itemView";
            this.itemView.Width = 50;
            // 
            // btnGoFirst
            // 
            this.btnGoFirst.Image = ((System.Drawing.Image)(resources.GetObject("btnGoFirst.Image")));
            this.btnGoFirst.Location = new System.Drawing.Point(0, 0);
            this.btnGoFirst.Name = "btnGoFirst";
            this.btnGoFirst.Size = new System.Drawing.Size(35, 25);
            this.btnGoFirst.TabIndex = 1;
            this.btnGoFirst.UseVisualStyleBackColor = true;
            this.btnGoFirst.Click += new System.EventHandler(this.btnGoFirst_Click);
            // 
            // btnOneStepPrev
            // 
            this.btnOneStepPrev.Image = ((System.Drawing.Image)(resources.GetObject("btnOneStepPrev.Image")));
            this.btnOneStepPrev.Location = new System.Drawing.Point(35, 0);
            this.btnOneStepPrev.Name = "btnOneStepPrev";
            this.btnOneStepPrev.Size = new System.Drawing.Size(35, 25);
            this.btnOneStepPrev.TabIndex = 2;
            this.btnOneStepPrev.UseVisualStyleBackColor = true;
            this.btnOneStepPrev.Click += new System.EventHandler(this.btnOneStepPrev_Click);
            // 
            // btnPlayStop
            // 
            this.btnPlayStop.Image = ((System.Drawing.Image)(resources.GetObject("btnPlayStop.Image")));
            this.btnPlayStop.Location = new System.Drawing.Point(70, 0);
            this.btnPlayStop.Name = "btnPlayStop";
            this.btnPlayStop.Size = new System.Drawing.Size(35, 25);
            this.btnPlayStop.TabIndex = 3;
            this.btnPlayStop.UseVisualStyleBackColor = true;
            this.btnPlayStop.Click += new System.EventHandler(this.btnPlayStop_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Image = ((System.Drawing.Image)(resources.GetObject("btnPlay.Image")));
            this.btnPlay.Location = new System.Drawing.Point(105, 0);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(35, 25);
            this.btnPlay.TabIndex = 4;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnOneStepNext
            // 
            this.btnOneStepNext.Image = ((System.Drawing.Image)(resources.GetObject("btnOneStepNext.Image")));
            this.btnOneStepNext.Location = new System.Drawing.Point(140, 0);
            this.btnOneStepNext.Name = "btnOneStepNext";
            this.btnOneStepNext.Size = new System.Drawing.Size(35, 25);
            this.btnOneStepNext.TabIndex = 5;
            this.btnOneStepNext.UseVisualStyleBackColor = true;
            this.btnOneStepNext.Click += new System.EventHandler(this.btnOneStepNext_Click);
            // 
            // btnGoLast
            // 
            this.btnGoLast.Image = ((System.Drawing.Image)(resources.GetObject("btnGoLast.Image")));
            this.btnGoLast.Location = new System.Drawing.Point(175, 0);
            this.btnGoLast.Name = "btnGoLast";
            this.btnGoLast.Size = new System.Drawing.Size(35, 25);
            this.btnGoLast.TabIndex = 6;
            this.btnGoLast.UseVisualStyleBackColor = true;
            this.btnGoLast.Click += new System.EventHandler(this.btnGoLast_Click);
            // 
            // btnGoReverse
            // 
            this.btnGoReverse.Image = ((System.Drawing.Image)(resources.GetObject("btnGoReverse.Image")));
            this.btnGoReverse.Location = new System.Drawing.Point(210, 0);
            this.btnGoReverse.Name = "btnGoReverse";
            this.btnGoReverse.Size = new System.Drawing.Size(35, 25);
            this.btnGoReverse.TabIndex = 7;
            this.btnGoReverse.UseVisualStyleBackColor = true;
            this.btnGoReverse.Click += new System.EventHandler(this.btnGoReverse_Click);
            // 
            // picBox_Lock
            // 
            this.picBox_Lock.Image = ((System.Drawing.Image)(resources.GetObject("picBox_Lock.Image")));
            this.picBox_Lock.Location = new System.Drawing.Point(271, 5);
            this.picBox_Lock.Name = "picBox_Lock";
            this.picBox_Lock.Size = new System.Drawing.Size(20, 20);
            this.picBox_Lock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBox_Lock.TabIndex = 8;
            this.picBox_Lock.TabStop = false;
            // 
            // picBox_View
            // 
            this.picBox_View.Image = ((System.Drawing.Image)(resources.GetObject("picBox_View.Image")));
            this.picBox_View.Location = new System.Drawing.Point(317, 5);
            this.picBox_View.Name = "picBox_View";
            this.picBox_View.Size = new System.Drawing.Size(20, 20);
            this.picBox_View.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBox_View.TabIndex = 9;
            this.picBox_View.TabStop = false;
            // 
            // panel_TimeEdit
            // 
            this.panel_TimeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_TimeEdit.AutoScroll = true;
            this.panel_TimeEdit.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panel_TimeEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_TimeEdit.Controls.Add(this.picBox_TimeEdit);
            this.panel_TimeEdit.Location = new System.Drawing.Point(401, 5);
            this.panel_TimeEdit.Name = "panel_TimeEdit";
            this.panel_TimeEdit.Size = new System.Drawing.Size(450, 233);
            this.panel_TimeEdit.TabIndex = 11;
            this.panel_TimeEdit.Scroll += new System.Windows.Forms.ScrollEventHandler(this.panel_TimeEdit_Scroll);
            // 
            // picBox_TimeEdit
            // 
            this.picBox_TimeEdit.BackColor = System.Drawing.SystemColors.ControlDark;
            this.picBox_TimeEdit.Location = new System.Drawing.Point(0, 0);
            this.picBox_TimeEdit.Name = "picBox_TimeEdit";
            this.picBox_TimeEdit.Size = new System.Drawing.Size(414, 230);
            this.picBox_TimeEdit.TabIndex = 0;
            this.picBox_TimeEdit.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnGoFirst);
            this.panel2.Controls.Add(this.btnOneStepPrev);
            this.panel2.Controls.Add(this.btnPlayStop);
            this.panel2.Controls.Add(this.picBox_View);
            this.panel2.Controls.Add(this.btnPlay);
            this.panel2.Controls.Add(this.picBox_Lock);
            this.panel2.Controls.Add(this.btnOneStepNext);
            this.panel2.Controls.Add(this.btnGoReverse);
            this.panel2.Controls.Add(this.btnGoLast);
            this.panel2.Location = new System.Drawing.Point(0, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(345, 27);
            this.panel2.TabIndex = 12;
            // 
            // btnObjAdd
            // 
            this.btnObjAdd.Location = new System.Drawing.Point(359, 90);
            this.btnObjAdd.Name = "btnObjAdd";
            this.btnObjAdd.Size = new System.Drawing.Size(36, 62);
            this.btnObjAdd.TabIndex = 13;
            this.btnObjAdd.Text = "Object Add";
            this.btnObjAdd.UseVisualStyleBackColor = true;
            this.btnObjAdd.Click += new System.EventHandler(this.btnObjAdd_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteGroupToolStripMenuItem,
            this.stopAnimationToolStripMenuItem,
            this.replayAnimationToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(170, 70);
            // 
            // deleteGroupToolStripMenuItem
            // 
            this.deleteGroupToolStripMenuItem.Name = "deleteGroupToolStripMenuItem";
            this.deleteGroupToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.deleteGroupToolStripMenuItem.Text = "Delete Group";
            this.deleteGroupToolStripMenuItem.Click += new System.EventHandler(this.deleteGroupToolStripMenuItem_Click);
            // 
            // stopAnimationToolStripMenuItem
            // 
            this.stopAnimationToolStripMenuItem.Name = "stopAnimationToolStripMenuItem";
            this.stopAnimationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.stopAnimationToolStripMenuItem.Text = "Stop Animation";
            this.stopAnimationToolStripMenuItem.Click += new System.EventHandler(this.stopAnimationToolStripMenuItem_Click);
            // 
            // replayAnimationToolStripMenuItem
            // 
            this.replayAnimationToolStripMenuItem.Name = "replayAnimationToolStripMenuItem";
            this.replayAnimationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.replayAnimationToolStripMenuItem.Text = "Replay Animation";
            this.replayAnimationToolStripMenuItem.Click += new System.EventHandler(this.replayAnimationToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 239);
            this.Controls.Add(this.btnObjAdd);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel_TimeEdit);
            this.Controls.Add(this.dGrid_TimeLineObj);
            this.Name = "MainForm";
            this.Text = "TimeLine";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dGrid_TimeLineObj)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_Lock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_View)).EndInit();
            this.panel_TimeEdit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBox_TimeEdit)).EndInit();
            this.panel2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dGrid_TimeLineObj;
        private System.Windows.Forms.Button btnGoFirst;
        private System.Windows.Forms.Button btnOneStepPrev;
        private System.Windows.Forms.Button btnPlayStop;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnOneStepNext;
        private System.Windows.Forms.Button btnGoLast;
        private System.Windows.Forms.Button btnGoReverse;
        private System.Windows.Forms.PictureBox picBox_Lock;
        private System.Windows.Forms.PictureBox picBox_View;
        private System.Windows.Forms.Panel panel_TimeEdit;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnObjAdd;
        private System.Windows.Forms.PictureBox picBox_TimeEdit;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Itemlock;
        private System.Windows.Forms.DataGridViewCheckBoxColumn itemView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replayAnimationToolStripMenuItem;
    }
}

