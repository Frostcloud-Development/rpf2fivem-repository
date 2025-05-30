namespace rpf2fivem
{
    partial class HelperWindow
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
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.InputVehicleName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InputVehicleBrand = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.InputVehiclePrice = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FinishButton = new System.Windows.Forms.Button();
            this.InputVehicleType = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.InputVehicleCategory = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(9, 132);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Vehicle Name";
            // 
            // InputVehicleName
            // 
            this.InputVehicleName.Location = new System.Drawing.Point(10, 44);
            this.InputVehicleName.Name = "InputVehicleName";
            this.InputVehicleName.Size = new System.Drawing.Size(442, 20);
            this.InputVehicleName.TabIndex = 15;
            this.InputVehicleName.Text = "default";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Vehicle Brand";
            // 
            // InputVehicleBrand
            // 
            this.InputVehicleBrand.Location = new System.Drawing.Point(10, 93);
            this.InputVehicleBrand.Name = "InputVehicleBrand";
            this.InputVehicleBrand.Size = new System.Drawing.Size(442, 20);
            this.InputVehicleBrand.TabIndex = 17;
            this.InputVehicleBrand.Text = "default";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(174, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Vehicle Price (Used for dealerships)";
            // 
            // InputVehiclePrice
            // 
            this.InputVehiclePrice.Location = new System.Drawing.Point(10, 148);
            this.InputVehiclePrice.Name = "InputVehiclePrice";
            this.InputVehiclePrice.Size = new System.Drawing.Size(442, 20);
            this.InputVehiclePrice.TabIndex = 21;
            this.InputVehiclePrice.Text = "default";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 187);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(239, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Vehicle Category (Compacts, Sedans, Sports etc)";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.FinishButton);
            this.groupBox1.Controls.Add(this.InputVehicleType);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.InputVehicleCategory);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.InputVehiclePrice);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.InputVehicleBrand);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.InputVehicleName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 360);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configuration Helper";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // FinishButton
            // 
            this.FinishButton.Enabled = false;
            this.FinishButton.Location = new System.Drawing.Point(10, 328);
            this.FinishButton.Name = "FinishButton";
            this.FinishButton.Size = new System.Drawing.Size(85, 26);
            this.FinishButton.TabIndex = 27;
            this.FinishButton.Text = "Finished";
            this.FinishButton.UseVisualStyleBackColor = true;
            // 
            // InputVehicleType
            // 
            this.InputVehicleType.Location = new System.Drawing.Point(10, 257);
            this.InputVehicleType.Name = "InputVehicleType";
            this.InputVehicleType.Size = new System.Drawing.Size(442, 20);
            this.InputVehicleType.TabIndex = 26;
            this.InputVehicleType.Text = "default";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 241);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(173, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Vehicle Type (automobile, bike etc)";
            // 
            // InputVehicleCategory
            // 
            this.InputVehicleCategory.Location = new System.Drawing.Point(10, 203);
            this.InputVehicleCategory.Name = "InputVehicleCategory";
            this.InputVehicleCategory.Size = new System.Drawing.Size(442, 20);
            this.InputVehicleCategory.TabIndex = 24;
            this.InputVehicleCategory.Text = "default";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 295);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "label2";
            // 
            // HelperWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 384);
            this.Controls.Add(this.groupBox1);
            this.Name = "HelperWindow";
            this.Text = "Configuration Helper";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox InputVehicleName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox InputVehicleBrand;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox InputVehiclePrice;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox InputVehicleType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox InputVehicleCategory;
        private System.Windows.Forms.Button FinishButton;
        private System.Windows.Forms.Label label2;
    }
}