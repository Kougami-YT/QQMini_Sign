namespace com.kougami.sign
{
    partial class Setting
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
            this.text_robotqq = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // text_robotqq
            // 
            this.text_robotqq.Font = new System.Drawing.Font("微软雅黑", 16F);
            this.text_robotqq.Location = new System.Drawing.Point(12, 12);
            this.text_robotqq.Name = "text_robotqq";
            this.text_robotqq.Size = new System.Drawing.Size(152, 36);
            this.text_robotqq.TabIndex = 0;
            this.text_robotqq.TextChanged += new System.EventHandler(this.text_robotqq_TextChanged);
            this.text_robotqq.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_robotqq_KeyPress);
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(176, 62);
            this.Controls.Add(this.text_robotqq);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Setting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "机器人QQ";
            this.Load += new System.EventHandler(this.Setting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox text_robotqq;
    }
}