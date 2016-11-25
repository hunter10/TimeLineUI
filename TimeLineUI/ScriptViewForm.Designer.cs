namespace TimeLineUI
{
    partial class ScriptViewForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.layer_dd = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(10, 10);
            this.textBox1.Margin = new System.Windows.Forms.Padding(1);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(1140, 442);
            this.textBox1.TabIndex = 0;
            // 
            // layer_dd
            // 
            this.layer_dd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layer_dd.FormattingEnabled = true;
            this.layer_dd.Items.AddRange(new object[] {
            "Layer 1",
            "Layer 2",
            "Layer 3",
            "Layer 4",
            "Layer-UI"});
            this.layer_dd.Location = new System.Drawing.Point(1006, 492);
            this.layer_dd.Name = "layer_dd";
            this.layer_dd.Size = new System.Drawing.Size(144, 20);
            this.layer_dd.TabIndex = 1;
            this.layer_dd.SelectedIndexChanged += new System.EventHandler(this.layer_dd_SelectedIndexChanged);
            // 
            // ScriptViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1164, 692);
            this.Controls.Add(this.layer_dd);
            this.Controls.Add(this.textBox1);
            this.Name = "ScriptViewForm";
            this.Text = "ScriptViewForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox layer_dd;
    }
}