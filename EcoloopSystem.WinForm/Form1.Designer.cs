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

        // 分頁控制
        private TabControl tabMain;
        private TabPage tabBorrow;
        private TabPage tabTablewareManage;

        // 租借分頁 - 主控制項
        private Button btnStartScan;
        private Label lblCardUid;
        private Label lblStatus;
        private ListBox lstLog;

        // 租借分頁 - 註冊面板
        private Panel pnlRegister;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnRegister;

        // 租借分頁 - 借/還面板（新設計）
        private Panel pnlBorrowReturn;
        private TextBox txtScanTableware;
        private Label lblScanResult;

        // 餐具管理分頁 - 鍵盤輸入模式
        private GroupBox grpTablewareScan;
        private Label lblScanHint;
        private TextBox txtTablewareUid;
        private Label lblTablewareType;
        private ComboBox cmbTablewareType;
        private Button btnRegisterTableware;
        private ListBox lstTablewareLog;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // 初始化控制項
            tabMain = new TabControl();
            tabBorrow = new TabPage();
            tabTablewareManage = new TabPage();

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

            pnlBorrowReturn = new Panel();
            txtScanTableware = new TextBox();
            lblScanResult = new Label();

            grpTablewareScan = new GroupBox();
            lblScanHint = new Label();
            txtTablewareUid = new TextBox();
            lblTablewareType = new Label();
            cmbTablewareType = new ComboBox();
            btnRegisterTableware = new Button();
            lstTablewareLog = new ListBox();

            tabMain.SuspendLayout();
            tabBorrow.SuspendLayout();
            tabTablewareManage.SuspendLayout();
            pnlRegister.SuspendLayout();
            pnlBorrowReturn.SuspendLayout();
            grpTablewareScan.SuspendLayout();
            SuspendLayout();

            // ========== TabControl ==========
            tabMain.Location = new Point(10, 10);
            tabMain.Size = new Size(680, 530);
            tabMain.Font = new Font("Microsoft JhengHei", 10F);
            tabMain.TabPages.Add(tabBorrow);
            tabMain.TabPages.Add(tabTablewareManage);

            // ========== 租借分頁 ==========
            tabBorrow.Text = "借用 / 歸還";
            tabBorrow.Padding = new Padding(10);
            tabBorrow.BackColor = Color.WhiteSmoke;

            // 開始感應按鈕
            btnStartScan.Text = "開始感應";
            btnStartScan.Location = new Point(15, 15);
            btnStartScan.Size = new Size(150, 50);
            btnStartScan.BackColor = Color.LightGreen;
            btnStartScan.FlatStyle = FlatStyle.Flat;
            btnStartScan.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            btnStartScan.Click += btnStartScan_Click;

            // 卡片 UID 顯示
            Label lblUidTitle = new Label();
            lblUidTitle.Text = "會員卡 UID:";
            lblUidTitle.Location = new Point(185, 20);
            lblUidTitle.AutoSize = true;
            lblUidTitle.Font = new Font("Microsoft JhengHei", 11F);

            lblCardUid.Text = "---";
            lblCardUid.Location = new Point(290, 20);
            lblCardUid.Size = new Size(280, 25);
            lblCardUid.Font = new Font("Consolas", 14F, FontStyle.Bold);
            lblCardUid.ForeColor = Color.DarkBlue;

            // 狀態標籤
            lblStatus.Text = "請按「開始感應」";
            lblStatus.Location = new Point(185, 50);
            lblStatus.Size = new Size(450, 25);
            lblStatus.Font = new Font("Microsoft JhengHei", 10F);
            lblStatus.ForeColor = Color.Gray;

            // ========== 註冊面板 ==========
            pnlRegister.Location = new Point(15, 80);
            pnlRegister.Size = new Size(635, 100);
            pnlRegister.BorderStyle = BorderStyle.FixedSingle;
            pnlRegister.BackColor = Color.LightYellow;
            pnlRegister.Visible = false;

            Label lblRegTitle = new Label();
            lblRegTitle.Text = "📝 新使用者註冊";
            lblRegTitle.Location = new Point(10, 8);
            lblRegTitle.AutoSize = true;
            lblRegTitle.Font = new Font("Microsoft JhengHei", 10F, FontStyle.Bold);
            lblRegTitle.ForeColor = Color.DarkOrange;

            lblPhone.Text = "手機號碼:";
            lblPhone.Location = new Point(10, 40);
            lblPhone.AutoSize = true;
            lblPhone.Font = new Font("Microsoft JhengHei", 10F);

            txtPhone.Location = new Point(90, 37);
            txtPhone.Size = new Size(150, 25);
            txtPhone.MaxLength = 15;
            txtPhone.Font = new Font("Microsoft JhengHei", 10F);

            lblPassword.Text = "密碼:";
            lblPassword.Location = new Point(260, 40);
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Microsoft JhengHei", 10F);

            txtPassword.Location = new Point(310, 37);
            txtPassword.Size = new Size(120, 25);
            txtPassword.MaxLength = 20;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.Font = new Font("Microsoft JhengHei", 10F);

            btnRegister.Text = "確認註冊";
            btnRegister.Location = new Point(450, 35);
            btnRegister.Size = new Size(100, 30);
            btnRegister.BackColor = Color.Orange;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Bold);
            btnRegister.Click += btnRegister_Click;

            Label lblRegHint = new Label();
            lblRegHint.Text = "※ 請輸入手機號碼(10碼)與密碼(至少4字元)完成註冊";
            lblRegHint.Location = new Point(10, 72);
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

            // ========== 借/還面板（新設計）==========
            pnlBorrowReturn.Location = new Point(15, 80);
            pnlBorrowReturn.Size = new Size(635, 130);
            pnlBorrowReturn.BorderStyle = BorderStyle.FixedSingle;
            pnlBorrowReturn.BackColor = Color.LightCyan;
            pnlBorrowReturn.Visible = false;

            Label lblBorrowTitle = new Label();
            lblBorrowTitle.Text = "🍽️ 餐具借用 / 歸還";
            lblBorrowTitle.Location = new Point(10, 8);
            lblBorrowTitle.AutoSize = true;
            lblBorrowTitle.Font = new Font("Microsoft JhengHei", 11F, FontStyle.Bold);
            lblBorrowTitle.ForeColor = Color.DarkCyan;

            Label lblScanInstr = new Label();
            lblScanInstr.Text = "請將餐具靠近第二台讀卡機掃描：";
            lblScanInstr.Location = new Point(10, 40);
            lblScanInstr.AutoSize = true;
            lblScanInstr.Font = new Font("Microsoft JhengHei", 10F);

            txtScanTableware.Location = new Point(10, 65);
            txtScanTableware.Size = new Size(280, 30);
            txtScanTableware.Font = new Font("Consolas", 14F, FontStyle.Bold);
            txtScanTableware.ForeColor = Color.DarkGreen;
            txtScanTableware.MaxLength = 20;
            txtScanTableware.KeyDown += txtScanTableware_KeyDown;

            Button btnClearScan = new Button();
            btnClearScan.Text = "清除";
            btnClearScan.Location = new Point(300, 65);
            btnClearScan.Size = new Size(60, 30);
            btnClearScan.BackColor = Color.LightGray;
            btnClearScan.FlatStyle = FlatStyle.Flat;
            btnClearScan.Font = new Font("Microsoft JhengHei", 9F);
            btnClearScan.Click += (s, e) => { txtScanTableware.Clear(); txtScanTableware.Focus(); lblScanResult.Text = ""; };

            lblScanResult.Text = "";
            lblScanResult.Location = new Point(10, 100);
            lblScanResult.Size = new Size(600, 25);
            lblScanResult.Font = new Font("Microsoft JhengHei", 10F, FontStyle.Bold);
            lblScanResult.ForeColor = Color.DarkGreen;

            pnlBorrowReturn.Controls.Add(lblBorrowTitle);
            pnlBorrowReturn.Controls.Add(lblScanInstr);
            pnlBorrowReturn.Controls.Add(txtScanTableware);
            pnlBorrowReturn.Controls.Add(btnClearScan);
            pnlBorrowReturn.Controls.Add(lblScanResult);

            // 日誌區
            Label lblLogTitle = new Label();
            lblLogTitle.Text = "操作記錄";
            lblLogTitle.Location = new Point(15, 225);
            lblLogTitle.AutoSize = true;
            lblLogTitle.Font = new Font("Microsoft JhengHei", 9F);
            lblLogTitle.ForeColor = Color.Gray;

            lstLog.Location = new Point(15, 245);
            lstLog.Size = new Size(635, 245);
            lstLog.Font = new Font("Consolas", 9F);

            tabBorrow.Controls.Add(btnStartScan);
            tabBorrow.Controls.Add(lblUidTitle);
            tabBorrow.Controls.Add(lblCardUid);
            tabBorrow.Controls.Add(lblStatus);
            tabBorrow.Controls.Add(pnlRegister);
            tabBorrow.Controls.Add(pnlBorrowReturn);
            tabBorrow.Controls.Add(lblLogTitle);
            tabBorrow.Controls.Add(lstLog);

            // ========== 餐具管理分頁 (鍵盤輸入模式) ==========
            tabTablewareManage.Text = "餐具管理 (RFID 註冊)";
            tabTablewareManage.BackColor = Color.WhiteSmoke;

            // 餐具掃描群組
            grpTablewareScan.Text = "餐具 RFID 掃描與註冊";
            grpTablewareScan.Location = new Point(15, 15);
            grpTablewareScan.Size = new Size(635, 200);
            grpTablewareScan.Font = new Font("Microsoft JhengHei", 10F);

            // 提示標籤
            lblScanHint.Text = "📡 將餐具貼紙靠近讀卡機，UID 會自動輸入到下方輸入框：";
            lblScanHint.Location = new Point(15, 30);
            lblScanHint.AutoSize = true;
            lblScanHint.Font = new Font("Microsoft JhengHei", 10F);
            lblScanHint.ForeColor = Color.DarkBlue;

            // UID 輸入框 (接收讀卡機鍵盤輸入)
            Label lblUidInput = new Label();
            lblUidInput.Text = "餐具 UID:";
            lblUidInput.Location = new Point(15, 65);
            lblUidInput.AutoSize = true;
            lblUidInput.Font = new Font("Microsoft JhengHei", 10F);

            txtTablewareUid.Location = new Point(100, 62);
            txtTablewareUid.Size = new Size(250, 30);
            txtTablewareUid.Font = new Font("Consolas", 14F, FontStyle.Bold);
            txtTablewareUid.ForeColor = Color.DarkGreen;
            txtTablewareUid.MaxLength = 20;
            txtTablewareUid.KeyDown += txtTablewareUid_KeyDown;

            Button btnClearUid = new Button();
            btnClearUid.Text = "清除";
            btnClearUid.Location = new Point(360, 60);
            btnClearUid.Size = new Size(70, 30);
            btnClearUid.BackColor = Color.LightGray;
            btnClearUid.FlatStyle = FlatStyle.Flat;
            btnClearUid.Font = new Font("Microsoft JhengHei", 9F);
            btnClearUid.Click += (s, e) => { txtTablewareUid.Clear(); txtTablewareUid.Focus(); };

            // 餐具類型選擇
            lblTablewareType.Text = "餐具類型:";
            lblTablewareType.Location = new Point(15, 110);
            lblTablewareType.AutoSize = true;
            lblTablewareType.Font = new Font("Microsoft JhengHei", 10F);

            cmbTablewareType.Location = new Point(100, 107);
            cmbTablewareType.Size = new Size(150, 25);
            cmbTablewareType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTablewareType.Font = new Font("Microsoft JhengHei", 10F);
            cmbTablewareType.Items.AddRange(new object[] { "Bowl (碗)", "Cup (杯)", "Chopsticks (筷)" });
            cmbTablewareType.SelectedIndex = 0;

            // 註冊按鈕
            btnRegisterTableware.Text = "註冊餐具";
            btnRegisterTableware.Location = new Point(270, 105);
            btnRegisterTableware.Size = new Size(130, 35);
            btnRegisterTableware.BackColor = Color.MediumSeaGreen;
            btnRegisterTableware.FlatStyle = FlatStyle.Flat;
            btnRegisterTableware.Font = new Font("Microsoft JhengHei", 11F, FontStyle.Bold);
            btnRegisterTableware.ForeColor = Color.White;
            btnRegisterTableware.Click += btnRegisterTableware_Click;

            // 提示文字
            Label lblRegHint2 = new Label();
            lblRegHint2.Text = "💡 讀卡機會模擬鍵盤輸入 UID。請點擊上方輸入框，再感應貼紙。";
            lblRegHint2.Location = new Point(15, 155);
            lblRegHint2.AutoSize = true;
            lblRegHint2.Font = new Font("Microsoft JhengHei", 9F);
            lblRegHint2.ForeColor = Color.Gray;

            Label lblRegHint3 = new Label();
            lblRegHint3.Text = "💡 如果沒有自動輸入，請確認讀卡機 USB 有正確連接。";
            lblRegHint3.Location = new Point(15, 175);
            lblRegHint3.AutoSize = true;
            lblRegHint3.Font = new Font("Microsoft JhengHei", 9F);
            lblRegHint3.ForeColor = Color.Gray;

            grpTablewareScan.Controls.Add(lblScanHint);
            grpTablewareScan.Controls.Add(lblUidInput);
            grpTablewareScan.Controls.Add(txtTablewareUid);
            grpTablewareScan.Controls.Add(btnClearUid);
            grpTablewareScan.Controls.Add(lblTablewareType);
            grpTablewareScan.Controls.Add(cmbTablewareType);
            grpTablewareScan.Controls.Add(btnRegisterTableware);
            grpTablewareScan.Controls.Add(lblRegHint2);
            grpTablewareScan.Controls.Add(lblRegHint3);

            // 餐具管理日誌
            Label lblTablewareLogTitle = new Label();
            lblTablewareLogTitle.Text = "餐具管理記錄";
            lblTablewareLogTitle.Location = new Point(15, 225);
            lblTablewareLogTitle.AutoSize = true;
            lblTablewareLogTitle.Font = new Font("Microsoft JhengHei", 9F);
            lblTablewareLogTitle.ForeColor = Color.Gray;

            lstTablewareLog.Location = new Point(15, 245);
            lstTablewareLog.Size = new Size(635, 245);
            lstTablewareLog.Font = new Font("Consolas", 9F);

            tabTablewareManage.Controls.Add(grpTablewareScan);
            tabTablewareManage.Controls.Add(lblTablewareLogTitle);
            tabTablewareManage.Controls.Add(lstTablewareLog);

            // ========== Form 設定 ==========
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 560);
            Controls.Add(tabMain);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Ecoloop 環保餐具租借系統";
            Font = new Font("Microsoft JhengHei", 9F);
            BackColor = Color.WhiteSmoke;

            tabMain.ResumeLayout(false);
            tabBorrow.ResumeLayout(false);
            tabBorrow.PerformLayout();
            tabTablewareManage.ResumeLayout(false);
            tabTablewareManage.PerformLayout();
            pnlRegister.ResumeLayout(false);
            pnlRegister.PerformLayout();
            pnlBorrowReturn.ResumeLayout(false);
            pnlBorrowReturn.PerformLayout();
            grpTablewareScan.ResumeLayout(false);
            grpTablewareScan.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
