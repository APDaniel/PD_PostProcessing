namespace PluginTester
{
    partial class PluginTesterForm
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
            this.Run = new System.Windows.Forms.Button();
            this.textBox_PID = new System.Windows.Forms.TextBox();
            this.textBox_SSID = new System.Windows.Forms.TextBox();
            this.label_PatientID = new System.Windows.Forms.Label();
            this.label_PlanID = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_CourseID = new System.Windows.Forms.TextBox();
            this.checkBox_save = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_planId = new System.Windows.Forms.TextBox();
            this.and = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Run
            // 
            this.Run.Location = new System.Drawing.Point(100, 210);
            this.Run.Name = "Run";
            this.Run.Size = new System.Drawing.Size(164, 21);
            this.Run.TabIndex = 4;
            this.Run.Text = "Run Script";
            this.Run.UseVisualStyleBackColor = true;
            this.Run.Click += new System.EventHandler(this.Run_Script_Click);
            // 
            // textBox_PID
            // 
            this.textBox_PID.Location = new System.Drawing.Point(102, 24);
            this.textBox_PID.Name = "textBox_PID";
            this.textBox_PID.Size = new System.Drawing.Size(164, 20);
            this.textBox_PID.TabIndex = 1;
            // 
            // textBox_SSID
            // 
            this.textBox_SSID.Location = new System.Drawing.Point(100, 150);
            this.textBox_SSID.Name = "textBox_SSID";
            this.textBox_SSID.Size = new System.Drawing.Size(164, 20);
            this.textBox_SSID.TabIndex = 3;
            // 
            // label_PatientID
            // 
            this.label_PatientID.AutoSize = true;
            this.label_PatientID.Location = new System.Drawing.Point(42, 27);
            this.label_PatientID.Name = "label_PatientID";
            this.label_PatientID.Size = new System.Drawing.Size(54, 13);
            this.label_PatientID.TabIndex = 4;
            this.label_PatientID.Text = "Patient ID";
            // 
            // label_PlanID
            // 
            this.label_PlanID.AutoSize = true;
            this.label_PlanID.Location = new System.Drawing.Point(12, 153);
            this.label_PlanID.Name = "label_PlanID";
            this.label_PlanID.Size = new System.Drawing.Size(81, 13);
            this.label_PlanID.TabIndex = 5;
            this.label_PlanID.Text = "Structure set ID";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Course ID";
            // 
            // textBox_CourseID
            // 
            this.textBox_CourseID.Location = new System.Drawing.Point(100, 72);
            this.textBox_CourseID.Name = "textBox_CourseID";
            this.textBox_CourseID.Size = new System.Drawing.Size(164, 20);
            this.textBox_CourseID.TabIndex = 2;
            // 
            // checkBox_save
            // 
            this.checkBox_save.AutoSize = true;
            this.checkBox_save.Location = new System.Drawing.Point(100, 185);
            this.checkBox_save.Name = "checkBox_save";
            this.checkBox_save.Size = new System.Drawing.Size(120, 17);
            this.checkBox_save.TabIndex = 9;
            this.checkBox_save.Text = "Save on completion";
            this.checkBox_save.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(43, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Plan ID";
            // 
            // textBox_planId
            // 
            this.textBox_planId.Location = new System.Drawing.Point(100, 98);
            this.textBox_planId.Name = "textBox_planId";
            this.textBox_planId.Size = new System.Drawing.Size(164, 20);
            this.textBox_planId.TabIndex = 10;
            // 
            // and
            // 
            this.and.AutoSize = true;
            this.and.Location = new System.Drawing.Point(171, 51);
            this.and.Name = "and";
            this.and.Size = new System.Drawing.Size(25, 13);
            this.and.TabIndex = 12;
            this.and.Text = "and";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(171, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "or";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 251);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(324, 99);
            this.label2.TabIndex = 8;
            this.label2.Text = "Enter in the patient, and either the couse and plan, or structure set that you wa" +
    "nt to send to your script.  ";
            // 
            // PluginTesterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 357);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.and);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_planId);
            this.Controls.Add(this.checkBox_save);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_CourseID);
            this.Controls.Add(this.label_PlanID);
            this.Controls.Add(this.label_PatientID);
            this.Controls.Add(this.textBox_SSID);
            this.Controls.Add(this.textBox_PID);
            this.Controls.Add(this.Run);
            this.Name = "PluginTesterForm";
            this.Text = "Plugin Tester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button Run;
        private System.Windows.Forms.TextBox textBox_PID;
        private System.Windows.Forms.TextBox textBox_SSID;
        private System.Windows.Forms.Label label_PatientID;
        private System.Windows.Forms.Label label_PlanID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_CourseID;
        private System.Windows.Forms.CheckBox checkBox_save;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_planId;
        private System.Windows.Forms.Label and;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
    }
}

