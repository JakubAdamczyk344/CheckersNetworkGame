namespace CheckersNetworkGame
{
    partial class ClientGame
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
            this.whoseTurnLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // whoseTurnLabel
            // 
            this.whoseTurnLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.whoseTurnLabel.Location = new System.Drawing.Point(50, 9);
            this.whoseTurnLabel.Name = "whoseTurnLabel";
            this.whoseTurnLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.whoseTurnLabel.Size = new System.Drawing.Size(400, 29);
            this.whoseTurnLabel.TabIndex = 6;
            this.whoseTurnLabel.Text = "Czekaj na ruch przeciwnika";
            this.whoseTurnLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(50, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(400, 13);
            this.label3.TabIndex = 7;
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ClientGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 542);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.whoseTurnLabel);
            this.Name = "ClientGame";
            this.Text = "ClientGame";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label whoseTurnLabel;
        private System.Windows.Forms.Label label3;
    }
}