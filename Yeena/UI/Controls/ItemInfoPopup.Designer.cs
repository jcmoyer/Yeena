namespace Yeena.UI.Controls {
    partial class ItemInfoPopup {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lblItemName1 = new System.Windows.Forms.Label();
            this.lblItemName2 = new System.Windows.Forms.Label();
            this.lblProps = new System.Windows.Forms.Label();
            this.lblUnidentified = new System.Windows.Forms.Label();
            this.lblFlavorText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblItemName1
            // 
            this.lblItemName1.AutoSize = true;
            this.lblItemName1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemName1.Location = new System.Drawing.Point(0, 0);
            this.lblItemName1.Name = "lblItemName1";
            this.lblItemName1.Size = new System.Drawing.Size(123, 25);
            this.lblItemName1.TabIndex = 0;
            this.lblItemName1.Text = "Item Name";
            this.lblItemName1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblItemName2
            // 
            this.lblItemName2.AutoSize = true;
            this.lblItemName2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemName2.Location = new System.Drawing.Point(0, 32);
            this.lblItemName2.Name = "lblItemName2";
            this.lblItemName2.Size = new System.Drawing.Size(77, 16);
            this.lblItemName2.TabIndex = 1;
            this.lblItemName2.Text = "Item Type";
            this.lblItemName2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblProps
            // 
            this.lblProps.AutoSize = true;
            this.lblProps.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProps.ForeColor = System.Drawing.Color.White;
            this.lblProps.Location = new System.Drawing.Point(2, 64);
            this.lblProps.Name = "lblProps";
            this.lblProps.Size = new System.Drawing.Size(140, 48);
            this.lblProps.TabIndex = 2;
            this.lblProps.Text = "Property 1 kind of long\r\nProperty 2\r\nProperty 3";
            this.lblProps.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblUnidentified
            // 
            this.lblUnidentified.AutoSize = true;
            this.lblUnidentified.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnidentified.ForeColor = System.Drawing.Color.Red;
            this.lblUnidentified.Location = new System.Drawing.Point(160, 88);
            this.lblUnidentified.Name = "lblUnidentified";
            this.lblUnidentified.Size = new System.Drawing.Size(75, 13);
            this.lblUnidentified.TabIndex = 3;
            this.lblUnidentified.Text = "Unidentified";
            // 
            // lblFlavorText
            // 
            this.lblFlavorText.AutoSize = true;
            this.lblFlavorText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFlavorText.ForeColor = System.Drawing.Color.SaddleBrown;
            this.lblFlavorText.Location = new System.Drawing.Point(129, 32);
            this.lblFlavorText.Name = "lblFlavorText";
            this.lblFlavorText.Size = new System.Drawing.Size(69, 16);
            this.lblFlavorText.TabIndex = 4;
            this.lblFlavorText.Text = "Flavor text";
            this.lblFlavorText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ItemInfoPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(247, 110);
            this.Controls.Add(this.lblFlavorText);
            this.Controls.Add(this.lblUnidentified);
            this.Controls.Add(this.lblProps);
            this.Controls.Add(this.lblItemName2);
            this.Controls.Add(this.lblItemName1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ItemInfoPopup";
            this.Opacity = 0.8D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblItemName1;
        private System.Windows.Forms.Label lblItemName2;
        private System.Windows.Forms.Label lblProps;
        private System.Windows.Forms.Label lblUnidentified;
        private System.Windows.Forms.Label lblFlavorText;
    }
}
