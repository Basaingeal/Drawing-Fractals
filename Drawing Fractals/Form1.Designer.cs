namespace Drawing_Fractals
{
    partial class Form1
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.zoomSlider = new System.Windows.Forms.TrackBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.button1 = new System.Windows.Forms.Button();
            this.lblLayerCount = new System.Windows.Forms.Label();
            this.lblLastLayerDuration = new System.Windows.Forms.Label();
            this.lblAverageLayerDuration = new System.Windows.Forms.Label();
            this.lblLastPointDuration = new System.Windows.Forms.Label();
            this.lblAveragePointDuration = new System.Windows.Forms.Label();
            this.lblTotalDuration = new System.Windows.Forms.Label();
            this.lblPointCount = new System.Windows.Forms.Label();
            this.lblDirectionRatio = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblDirectionRatio);
            this.panel1.Controls.Add(this.lblPointCount);
            this.panel1.Controls.Add(this.lblTotalDuration);
            this.panel1.Controls.Add(this.lblAveragePointDuration);
            this.panel1.Controls.Add(this.lblLastPointDuration);
            this.panel1.Controls.Add(this.lblAverageLayerDuration);
            this.panel1.Controls.Add(this.lblLastLayerDuration);
            this.panel1.Controls.Add(this.lblLayerCount);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.zoomSlider);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.trackBar1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(284, 261);
            this.panel1.TabIndex = 0;
            // 
            // zoomSlider
            // 
            this.zoomSlider.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zoomSlider.Location = new System.Drawing.Point(0, 216);
            this.zoomSlider.Name = "zoomSlider";
            this.zoomSlider.Size = new System.Drawing.Size(284, 45);
            this.zoomSlider.TabIndex = 3;
            this.zoomSlider.Visible = false;
            this.zoomSlider.Scroll += new System.EventHandler(this.zoomSlider_Scroll);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(284, 261);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(284, 261);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(-19, 215);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(303, 45);
            this.trackBar1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(209, 238);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Save Image";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SaveBitmap);
            // 
            // lblLayerCount
            // 
            this.lblLayerCount.AutoSize = true;
            this.lblLayerCount.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblLayerCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblLayerCount.Location = new System.Drawing.Point(0, 0);
            this.lblLayerCount.Name = "lblLayerCount";
            this.lblLayerCount.Size = new System.Drawing.Size(76, 13);
            this.lblLayerCount.TabIndex = 5;
            this.lblLayerCount.Text = "Layer Count: 0";
            // 
            // lblLastLayerDuration
            // 
            this.lblLastLayerDuration.AutoSize = true;
            this.lblLastLayerDuration.Location = new System.Drawing.Point(4, 44);
            this.lblLastLayerDuration.Name = "lblLastLayerDuration";
            this.lblLastLayerDuration.Size = new System.Drawing.Size(105, 13);
            this.lblLastLayerDuration.TabIndex = 6;
            this.lblLastLayerDuration.Text = "Last Layer Duration: ";
            // 
            // lblAverageLayerDuration
            // 
            this.lblAverageLayerDuration.AutoSize = true;
            this.lblAverageLayerDuration.Location = new System.Drawing.Point(3, 57);
            this.lblAverageLayerDuration.Name = "lblAverageLayerDuration";
            this.lblAverageLayerDuration.Size = new System.Drawing.Size(119, 13);
            this.lblAverageLayerDuration.TabIndex = 7;
            this.lblAverageLayerDuration.Text = "Average Layer Duration";
            // 
            // lblLastPointDuration
            // 
            this.lblLastPointDuration.AutoSize = true;
            this.lblLastPointDuration.Location = new System.Drawing.Point(4, 70);
            this.lblLastPointDuration.Name = "lblLastPointDuration";
            this.lblLastPointDuration.Size = new System.Drawing.Size(103, 13);
            this.lblLastPointDuration.TabIndex = 8;
            this.lblLastPointDuration.Text = "Last Point Duration: ";
            // 
            // lblAveragePointDuration
            // 
            this.lblAveragePointDuration.AutoSize = true;
            this.lblAveragePointDuration.Location = new System.Drawing.Point(4, 83);
            this.lblAveragePointDuration.Name = "lblAveragePointDuration";
            this.lblAveragePointDuration.Size = new System.Drawing.Size(117, 13);
            this.lblAveragePointDuration.TabIndex = 9;
            this.lblAveragePointDuration.Text = "Average Point Duration";
            // 
            // lblTotalDuration
            // 
            this.lblTotalDuration.AutoSize = true;
            this.lblTotalDuration.Location = new System.Drawing.Point(4, 96);
            this.lblTotalDuration.Name = "lblTotalDuration";
            this.lblTotalDuration.Size = new System.Drawing.Size(80, 13);
            this.lblTotalDuration.TabIndex = 10;
            this.lblTotalDuration.Text = "Total Duration: ";
            // 
            // lblPointCount
            // 
            this.lblPointCount.AutoSize = true;
            this.lblPointCount.Location = new System.Drawing.Point(7, 28);
            this.lblPointCount.Name = "lblPointCount";
            this.lblPointCount.Size = new System.Drawing.Size(68, 13);
            this.lblPointCount.TabIndex = 11;
            this.lblPointCount.Text = "Point Count: ";
            // 
            // lblDirectionRatio
            // 
            this.lblDirectionRatio.AutoSize = true;
            this.lblDirectionRatio.Location = new System.Drawing.Point(4, 113);
            this.lblDirectionRatio.Name = "lblDirectionRatio";
            this.lblDirectionRatio.Size = new System.Drawing.Size(83, 13);
            this.lblDirectionRatio.TabIndex = 12;
            this.lblDirectionRatio.Text = "Direction Ratio: ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TrackBar zoomSlider;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblLayerCount;
        private System.Windows.Forms.Label lblAverageLayerDuration;
        private System.Windows.Forms.Label lblLastLayerDuration;
        private System.Windows.Forms.Label lblTotalDuration;
        private System.Windows.Forms.Label lblAveragePointDuration;
        private System.Windows.Forms.Label lblLastPointDuration;
        private System.Windows.Forms.Label lblPointCount;
        private System.Windows.Forms.Label lblDirectionRatio;
    }
}

