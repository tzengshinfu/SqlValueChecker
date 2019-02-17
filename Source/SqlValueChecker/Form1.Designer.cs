namespace SqlSyntaxChecker {
    partial class Form1 {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.txtSqlSyntax = new System.Windows.Forms.TextBox();
            this.btnCheck = new System.Windows.Forms.Button();
            this.cbxDatabaseName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtSqlSyntax
            // 
            this.txtSqlSyntax.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSqlSyntax.HideSelection = false;
            this.txtSqlSyntax.Location = new System.Drawing.Point(21, 78);
            this.txtSqlSyntax.Margin = new System.Windows.Forms.Padding(4);
            this.txtSqlSyntax.MaxLength = 2147483647;
            this.txtSqlSyntax.Multiline = true;
            this.txtSqlSyntax.Name = "txtSqlSyntax";
            this.txtSqlSyntax.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSqlSyntax.Size = new System.Drawing.Size(845, 638);
            this.txtSqlSyntax.TabIndex = 0;
            this.txtSqlSyntax.TextChanged += new System.EventHandler(this.txtSqlSyntax_TextChanged);
            this.txtSqlSyntax.DoubleClick += new System.EventHandler(this.txtSqlSyntax_DoubleClick);
            // 
            // btnCheck
            // 
            this.btnCheck.Enabled = false;
            this.btnCheck.Location = new System.Drawing.Point(279, 16);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(4);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(112, 31);
            this.btnCheck.TabIndex = 1;
            this.btnCheck.Text = "檢查";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // cbxDatabaseName
            // 
            this.cbxDatabaseName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDatabaseName.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbxDatabaseName.FormattingEnabled = true;
            this.cbxDatabaseName.Location = new System.Drawing.Point(82, 16);
            this.cbxDatabaseName.Margin = new System.Windows.Forms.Padding(4);
            this.cbxDatabaseName.Name = "cbxDatabaseName";
            this.cbxDatabaseName.Size = new System.Drawing.Size(180, 24);
            this.cbxDatabaseName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "資料庫";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(242, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "SQL指令,用分號(；)符號分隔多筆";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 737);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxDatabaseName);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.txtSqlSyntax);
            this.Font = new System.Drawing.Font("PMingLiU", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "SQL欄位值長度檢查";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSqlSyntax;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.ComboBox cbxDatabaseName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

