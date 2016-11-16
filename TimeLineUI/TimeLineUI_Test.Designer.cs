namespace TimeLineUI_Test
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
            this.timeLineUI1 = new TimeLineUI.TimeLineUI();
            this.SuspendLayout();
            // 
            // timeLineUI1
            // 
            this.timeLineUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeLineUI1.Location = new System.Drawing.Point(0, 0);
            this.timeLineUI1.Name = "timeLineUI1";
            this.timeLineUI1.Size = new System.Drawing.Size(777, 267);
            this.timeLineUI1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(777, 267);
            this.Controls.Add(this.timeLineUI1);
            this.Name = "MainForm";
            this.Text = "TimeLineUI_Test";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);

        }

        private TimeLineUI.TimeLineUI timeLineUI1;

        #endregion

        //private TimeLineUI.TimeLineUI timeLineUI1;
    }
}

