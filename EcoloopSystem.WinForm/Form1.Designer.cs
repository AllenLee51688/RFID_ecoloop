namespace EcoloopSystem.WinForm
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // 主控制項
        private Button btnStartScan;
        private Label lblCardUid;
        private Label lblStatus;
        private ListBox lstLog;

        // 註冊面板
        private Panel pnlRegister;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnRegister;

        // 租借面板
        private Panel pnlBorrow;
        private Label lblTableware;
        private ComboBox cmbTestTableware;
        private Button btnBorrow;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            btnStartScan = new Button();
            lblCardUid = new Label();
            lblStatus = new Label();
            lstLog = new ListBox();

            pnlRegister = new Panel();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnRegister = new Button();

            pnlBorrow = new Panel();
            lblTableware = new Label();
            cmbTestTableware = new ComboBox();
            btnBorrow = new Button();

            pnlRegister.SuspendLayout();
            pnlBorrow.SuspendLayout();
            SuspendLayout();

            // ========== 開始感應按鈕 ==========
            btnStartScan.Text = "開始感應";
            btnStartScan.Location = new Point(20, 20);
            btnStartScan.Size = new Size(150, 50);
            btnStartScan.BackColor = Color.LightGreen;
            btnStartScan.FlatStyle = FlatStyle.Flat;
            btnStartScan.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            btnStartScan.Click += btnStartScan_Click;

            // ========== 卡片 UID 顯示 ==========
            Label lblUidTitle = new Label();
            lblUidTitle.Text = "卡片 UID:";
            lblUidTitle.Location = new Point(200, 25);
            lblUidTitle.AutoSize = true;
            lblUidTitle.Font = new Font("Microsoft JhengHei", 11F);

            lblCardUid.Text = "---";
            lblCardUid.Location = new Point(290, 25);
            lblCardUid.Size = new Size(280, 25);
            lblCardUid.Font = new Font("Consolas", 14F, FontStyle.Bold);
            lblCardUid.ForeColor = Color.DarkBlue;

            // ========== 狀態標籤 ==========
            lblStatus.Text = "請按「開始感應」";
            lblStatus.Location = new Point(200, 55);
            lblStatus.Size = new Size(380, 25);
            lblStatus.Font = new Font("Microsoft JhengHei", 10F);
            lblStatus.ForeColor = Color.Gray;

            // ========== 註冊面板 ==========
            pnlRegister.Location = new Point(20, 90);
            pnlRegister.Size = new Size(560, 120);
            pnlRegister.BorderStyle = BorderStyle.FixedSingle;
            pnlRegister.BackColor = Color.LightYellow;
            pnlRegister.Visible = false;

            Label lblRegTitle = new Label();
            lblRegTitle.Text = "新使用者註冊";
            lblRegTitle.Location = new Point(10, 10);
            lblRegTitle.AutoSize = true;
            lblRegTitle.Font = new Font("Microsoft JhengHei", 11F, FontStyle.Bold);
            lblRegTitle.ForeColor = Color.DarkOrange;

            lblPhone.Text = "手機號碼:";
            lblPhone.Location = new Point(10, 45);
            lblPhone.AutoSize = true;
            lblPhone.Font = new Font("Microsoft JhengHei", 10F);

            txtPhone.Location = new Point(90, 42);
            txtPhone.Size = new Size(150, 25);
            txtPhone.MaxLength = 15;
            txtPhone.Font = new Font("Microsoft JhengHei", 10F);

            lblPassword.Text = "密碼:";
            lblPassword.Location = new Point(260, 45);
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Microsoft JhengHei", 10F);

            txtPassword.Location = new Point(310, 42);
            txtPassword.Size = new Size(120, 25);
            txtPassword.MaxLength = 20;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Font = new Font("Microsoft JhengHei", 10F);

            btnRegister.Text = "確認註冊";
            btnRegister.Location = new Point(450, 40);
            btnRegister.Size = new Size(100, 30);
            btnRegister.BackColor = Color.Orange;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Bold);
            btnRegister.Click += btnRegister_Click;

            Label lblRegHint = new Label();
            lblRegHint.Text = "※ 請輸入手機號碼(10碼)與密碼(至少4字元)完成註冊";
            lblRegHint.Location = new Point(10, 85);
            lblRegHint.AutoSize = true;
            lblRegHint.Font = new Font("Microsoft JhengHei", 9F);
            lblRegHint.ForeColor = Color.Gray;

            pnlRegister.Controls.Add(lblRegTitle);
            pnlRegister.Controls.Add(lblPhone);
            pnlRegister.Controls.Add(txtPhone);
            pnlRegister.Controls.Add(lblPassword);
            pnlRegister.Controls.Add(txtPassword);
            pnlRegister.Controls.Add(btnRegister);
            pnlRegister.Controls.Add(lblRegHint);

            // ========== 租借面板 ==========
            pnlBorrow.Location = new Point(20, 90);
            pnlBorrow.Size = new Size(560, 120);
            pnlBorrow.BorderStyle = BorderStyle.FixedSingle;
            pnlBorrow.BackColor = Color.LightCyan;
            pnlBorrow.Visible = false;

            Label lblBorrowTitle = new Label();
            lblBorrowTitle.Text = "餐具租借";
            lblBorrowTitle.Location = new Point(10, 10);
            lblBorrowTitle.AutoSize = true;
            lblBorrowTitle.Font = new Font("Microsoft JhengHei", 11F, FontStyle.Bold);
            lblBorrowTitle.ForeColor = Color.DarkCyan;

            lblTableware.Text = "選擇餐具 (測試):";
            lblTableware.Location = new Point(10, 50);
            lblTableware.AutoSize = true;
            lblTableware.Font = new Font("Microsoft JhengHei", 10F);

            cmbTestTableware.Location = new Point(130, 47);
            cmbTestTableware.Size = new Size(200, 25);
            cmbTestTableware.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTestTableware.Font = new Font("Microsoft JhengHei", 10F);

            btnBorrow.Text = "確認租借";
            btnBorrow.Location = new Point(350, 45);
            btnBorrow.Size = new Size(100, 35);
            btnBorrow.BackColor = Color.DeepSkyBlue;
            btnBorrow.FlatStyle = FlatStyle.Flat;
            btnBorrow.Font = new Font("Microsoft JhengHei", 10F, FontStyle.Bold);
            btnBorrow.ForeColor = Color.White;
            btnBorrow.Click += btnBorrow_Click;

            Label lblBorrowHint = new Label();
            lblBorrowHint.Text = "※ 實際使用時會由第二台讀卡機感應餐具";
            lblBorrowHint.Location = new Point(10, 90);
            lblBorrowHint.AutoSize = true;
            lblBorrowHint.Font = new Font("Microsoft JhengHei", 9F);
            lblBorrowHint.ForeColor = Color.Gray;

            pnlBorrow.Controls.Add(lblBorrowTitle);
            pnlBorrow.Controls.Add(lblTableware);
            pnlBorrow.Controls.Add(cmbTestTableware);
            pnlBorrow.Controls.Add(btnBorrow);
            pnlBorrow.Controls.Add(lblBorrowHint);

            // ========== 日誌區 ==========
            Label lblLogTitle = new Label();
            lblLogTitle.Text = "操作記錄";
            lblLogTitle.Location = new Point(20, 220);
            lblLogTitle.AutoSize = true;
            lblLogTitle.Font = new Font("Microsoft JhengHei", 9F);
            lblLogTitle.ForeColor = Color.Gray;

            lstLog.Location = new Point(20, 240);
            lstLog.Size = new Size(560, 120);
            lstLog.Font = new Font("Consolas", 9F);

            // ========== Form 設定 ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 375);
            Controls.Add(btnStartScan);
            Controls.Add(lblUidTitle);
            Controls.Add(lblCardUid);
            Controls.Add(lblStatus);
            Controls.Add(pnlRegister);
            Controls.Add(pnlBorrow);
            Controls.Add(lblLogTitle);
            Controls.Add(lstLog);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ecoloop 環保餐具租借系統";
            Font = new Font("Microsoft JhengHei", 9F);
            BackColor = Color.WhiteSmoke;

            pnlRegister.ResumeLayout(false);
            pnlRegister.PerformLayout();
            pnlBorrow.ResumeLayout(false);
            pnlBorrow.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
