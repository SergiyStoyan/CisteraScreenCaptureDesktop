namespace Cliver.CisteraScreenCaptureTestServer
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
            this.start = new System.Windows.Forms.Button();
            this.stop = new System.Windows.Forms.Button();
            this.localTcpPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.mpegCommandLine = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.state = new System.Windows.Forms.TextBox();
            this.bSsl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // start
            // 
            this.start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.start.Location = new System.Drawing.Point(369, 312);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(75, 23);
            this.start.TabIndex = 0;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // stop
            // 
            this.stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stop.Location = new System.Drawing.Point(450, 312);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(75, 23);
            this.stop.TabIndex = 1;
            this.stop.Text = "Stop";
            this.stop.UseVisualStyleBackColor = true;
            this.stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // localTcpPort
            // 
            this.localTcpPort.Location = new System.Drawing.Point(112, 17);
            this.localTcpPort.Name = "localTcpPort";
            this.localTcpPort.Size = new System.Drawing.Size(67, 20);
            this.localTcpPort.TabIndex = 2;
            this.localTcpPort.Text = "5700";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Local TCP Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 164);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "State:";
            // 
            // mpegCommandLine
            // 
            this.mpegCommandLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mpegCommandLine.Location = new System.Drawing.Point(27, 89);
            this.mpegCommandLine.Multiline = true;
            this.mpegCommandLine.Name = "mpegCommandLine";
            this.mpegCommandLine.Size = new System.Drawing.Size(498, 62);
            this.mpegCommandLine.TabIndex = 8;
            this.mpegCommandLine.Text = "-f gdigrab -framerate 10 -f rtp_mpegts -srtp_out_suite AES_CM_128_HMAC_SHA1_80 -s" +
    "rtp_out_params aMg7BqN047lFN72szkezmPyN1qSMilYCXbqP/sCt srtp://127.0.0.1:5920";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Mpeg Command Line:";
            // 
            // state
            // 
            this.state.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.state.Location = new System.Drawing.Point(27, 180);
            this.state.Multiline = true;
            this.state.Name = "state";
            this.state.ReadOnly = true;
            this.state.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.state.Size = new System.Drawing.Size(498, 117);
            this.state.TabIndex = 10;
            this.state.Text = "gddgrdfg";
            // 
            // bSsl
            // 
            this.bSsl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bSsl.Location = new System.Drawing.Point(288, 312);
            this.bSsl.Name = "bSsl";
            this.bSsl.Size = new System.Drawing.Size(75, 23);
            this.bSsl.TabIndex = 11;
            this.bSsl.Text = "SSL";
            this.bSsl.UseVisualStyleBackColor = true;
            this.bSsl.Click += new System.EventHandler(this.bSsl_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 366);
            this.Controls.Add(this.bSsl);
            this.Controls.Add(this.state);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.mpegCommandLine);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.localTcpPort);
            this.Controls.Add(this.stop);
            this.Controls.Add(this.start);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Button stop;
        private System.Windows.Forms.TextBox localTcpPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox mpegCommandLine;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox state;
        private System.Windows.Forms.Button bSsl;
    }
}