namespace GenericExcelMapper
{
    partial class ExcelTemplate
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstIssueList = new System.Windows.Forms.ListBox();
            this.IssuedFileslbl = new System.Windows.Forms.Label();
            this.DataConfigGrpBox = new System.Windows.Forms.GroupBox();
            this.lblSheetName = new System.Windows.Forms.Label();
            this.txtBoxSheetName = new System.Windows.Forms.TextBox();
            this.lblDataStartIndex = new System.Windows.Forms.Label();
            this.txtBoxDataStartIndex = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.DataConfigGrpBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstIssueList
            // 
            this.lstIssueList.FormattingEnabled = true;
            this.lstIssueList.Location = new System.Drawing.Point(3, 28);
            this.lstIssueList.Name = "lstIssueList";
            this.lstIssueList.Size = new System.Drawing.Size(291, 95);
            this.lstIssueList.TabIndex = 140;
            this.lstIssueList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstIssueList_MouseDoubleClick);
            // 
            // IssuedFileslbl
            // 
            this.IssuedFileslbl.AutoSize = true;
            this.IssuedFileslbl.Location = new System.Drawing.Point(2, 4);
            this.IssuedFileslbl.Name = "IssuedFileslbl";
            this.IssuedFileslbl.Size = new System.Drawing.Size(65, 13);
            this.IssuedFileslbl.TabIndex = 141;
            this.IssuedFileslbl.Text = "Issued Files:";
            // 
            // DataConfigGrpBox
            // 
            this.DataConfigGrpBox.Controls.Add(this.lblSheetName);
            this.DataConfigGrpBox.Controls.Add(this.txtBoxSheetName);
            this.DataConfigGrpBox.Controls.Add(this.lblDataStartIndex);
            this.DataConfigGrpBox.Controls.Add(this.txtBoxDataStartIndex);
            this.DataConfigGrpBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DataConfigGrpBox.Location = new System.Drawing.Point(300, 28);
            this.DataConfigGrpBox.Name = "DataConfigGrpBox";
            this.DataConfigGrpBox.Size = new System.Drawing.Size(469, 95);
            this.DataConfigGrpBox.TabIndex = 142;
            this.DataConfigGrpBox.TabStop = false;
            this.DataConfigGrpBox.Text = "Data Configuration";
            // 
            // lblSheetName
            // 
            this.lblSheetName.AutoSize = true;
            this.lblSheetName.Location = new System.Drawing.Point(7, 51);
            this.lblSheetName.Name = "lblSheetName";
            this.lblSheetName.Size = new System.Drawing.Size(69, 13);
            this.lblSheetName.TabIndex = 3;
            this.lblSheetName.Text = "Sheet Name:";
            // 
            // txtBoxSheetName
            // 
            this.txtBoxSheetName.Location = new System.Drawing.Point(97, 48);
            this.txtBoxSheetName.Name = "txtBoxSheetName";
            this.txtBoxSheetName.Size = new System.Drawing.Size(117, 20);
            this.txtBoxSheetName.TabIndex = 2;
            // 
            // lblDataStartIndex
            // 
            this.lblDataStartIndex.AutoSize = true;
            this.lblDataStartIndex.Location = new System.Drawing.Point(7, 25);
            this.lblDataStartIndex.Name = "lblDataStartIndex";
            this.lblDataStartIndex.Size = new System.Drawing.Size(84, 13);
            this.lblDataStartIndex.TabIndex = 1;
            this.lblDataStartIndex.Text = "Data StartIndex:";
            // 
            // txtBoxDataStartIndex
            // 
            this.txtBoxDataStartIndex.Location = new System.Drawing.Point(97, 22);
            this.txtBoxDataStartIndex.Name = "txtBoxDataStartIndex";
            this.txtBoxDataStartIndex.Size = new System.Drawing.Size(117, 20);
            this.txtBoxDataStartIndex.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(464, 547);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 143;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(385, 547);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(73, 23);
            this.btnSave.TabIndex = 144;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Location = new System.Drawing.Point(306, 547);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(73, 23);
            this.btnPaste.TabIndex = 145;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // ExcelTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnPaste);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.DataConfigGrpBox);
            this.Controls.Add(this.IssuedFileslbl);
            this.Controls.Add(this.lstIssueList);
            this.Name = "ExcelTemplate";
            this.Size = new System.Drawing.Size(780, 573);
            this.DataConfigGrpBox.ResumeLayout(false);
            this.DataConfigGrpBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxOWC11.AxSpreadsheet axSpreadsheet;
        private AxOWC11.AxSpreadsheet axSpreadsheetMap;
        private System.Windows.Forms.ListBox lstIssueList;
        private System.Windows.Forms.Label IssuedFileslbl;
        private System.Windows.Forms.GroupBox DataConfigGrpBox;
        private System.Windows.Forms.Label lblDataStartIndex;
        private System.Windows.Forms.TextBox txtBoxDataStartIndex;
        private System.Windows.Forms.Label lblSheetName;
        private System.Windows.Forms.TextBox txtBoxSheetName;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnPaste;
    }
}
