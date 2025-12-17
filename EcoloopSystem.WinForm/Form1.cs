using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Forms;

namespace EcoloopSystem.WinForm
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient;
        private readonly RFIDReader _rfidReader;
        private System.Windows.Forms.Timer _scanTimer;
        private string? _currentCardUid = null;
        private bool _isScanning = false;
        private int? _currentUserId = null;

        // 讀卡參數 (固定值)
        private const int SECTOR = 0;
        private const int BLOCK = 0;
        private const string KEY_TYPE = "A";
        private const string LOAD_KEY = "FFFFFFFFFFFF";

        public Form1()
        {
            InitializeComponent();
            
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5035");
            _rfidReader = new RFIDReader();

            // 初始化掃描計時器
            _scanTimer = new System.Windows.Forms.Timer();
            _scanTimer.Interval = 1000; // 每 1 秒掃描一次
            _scanTimer.Tick += ScanTimer_Tick;
        }

        #region 租借分頁 - 會員卡感應

        private void btnStartScan_Click(object? sender, EventArgs e)
        {
            if (_isScanning)
            {
                StopScanning();
            }
            else
            {
                StartScanning();
            }
        }

        private void StartScanning()
        {
            _isScanning = true;
            _currentCardUid = null;
            _currentUserId = null;
            btnStartScan.Text = "停止感應";
            btnStartScan.BackColor = Color.LightCoral;
            lblStatus.Text = "感應中...請放置會員卡";
            lblStatus.ForeColor = Color.Blue;
            pnlRegister.Visible = false;
            pnlBorrowReturn.Visible = false;
            _scanTimer.Start();
            Log("開始感應卡片...");
        }

        private void StopScanning()
        {
            _isScanning = false;
            _scanTimer.Stop();
            btnStartScan.Text = "開始感應";
            btnStartScan.BackColor = Color.LightGreen;
            lblStatus.Text = "已停止感應";
            lblStatus.ForeColor = Color.Gray;
            pnlBorrowReturn.Visible = false;
            Log("停止感應");
        }
        
        private async void ScanTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isScanning) return;

            try
            {
                // 使用原本的讀取邏輯
                string result = _rfidReader.ReadCardUID();
                
                if (!result.StartsWith("❌"))
                {
                    // 成功讀到卡片，暫停掃描
                    _scanTimer.Stop();
                    _currentCardUid = result;
                    lblCardUid.Text = result;
                    Log($"讀取到會員卡: {result}");
                    
                    // 查詢是否已註冊
                    await CheckCardRegistration(result);
                }
                else
                {
                    // 卡片離開或讀取失敗
                    if (_currentCardUid != null)
                    {
                        Log("會員卡已移開");
                        _currentCardUid = null;
                        _currentUserId = null;
                        lblCardUid.Text = "---";
                        lblStatus.Text = "感應中...請放置會員卡";
                        lblStatus.ForeColor = Color.Blue;
                        pnlRegister.Visible = false;
                        pnlBorrowReturn.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"掃描錯誤: {ex.Message}");
            }
        }

        private async Task CheckCardRegistration(string cardUid)
        {
            try
            {
                lblStatus.Text = "查詢中...";
                lblStatus.ForeColor = Color.Orange;

                var response = await _httpClient.GetAsync($"api/users/check/{cardUid}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CheckCardResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.IsRegistered == true)
                {
                    _currentUserId = result.UserId;
                    lblStatus.Text = $"✅ 歡迎！手機: {result.PhoneNumber}，請感應餐具進行借用/歸還";
                    lblStatus.ForeColor = Color.Green;
                    pnlRegister.Visible = false;
                    pnlBorrowReturn.Visible = true;
                    txtScanTableware.Clear();
                    txtScanTableware.Focus();
                    lblScanResult.Text = "";
                    Log($"已註冊使用者，ID: {result.UserId}，等待感應餐具");
                }
                else
                {
                    _currentUserId = null;
                    lblStatus.Text = "新卡片，請註冊";
                    lblStatus.ForeColor = Color.Orange;
                    pnlRegister.Visible = true;
                    pnlBorrowReturn.Visible = false;
                    txtPhone.Text = "";
                    txtPassword.Text = "";
                    txtPhone.Focus();
                    Log("卡片尚未註冊");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "查詢失敗";
                lblStatus.ForeColor = Color.Red;
                Log($"API 錯誤: {ex.Message}");
            }
        }

        private async void btnRegister_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentCardUid))
            {
                MessageBox.Show("請先放置卡片", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text;

            if (phone.Length < 10)
            {
                MessageBox.Show("請輸入正確的手機號碼（至少10碼）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("密碼至少需要4個字元", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                btnRegister.Enabled = false;
                lblStatus.Text = "註冊中...";

                var request = new { CardId = _currentCardUid, PhoneNumber = phone, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/users/register", request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    lblStatus.Text = "註冊成功！";
                    lblStatus.ForeColor = Color.Green;
                    Log($"註冊成功: {phone}");
                    MessageBox.Show("註冊成功！現在可以借用餐具了。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 註冊成功後重新查詢
                    await CheckCardRegistration(_currentCardUid);
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    lblStatus.Text = "註冊失敗";
                    lblStatus.ForeColor = Color.Red;
                    Log($"註冊失敗: {error?.Message}");
                    MessageBox.Show(error?.Message ?? "註冊失敗", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "註冊失敗";
                lblStatus.ForeColor = Color.Red;
                Log($"錯誤: {ex.Message}");
                MessageBox.Show($"錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
            }
        }

        #endregion

        #region 自動借/還功能

        /// <summary>
        /// 處理餐具感應輸入框的按鍵事件
        /// 當讀卡機輸入完 UID 並按下 Enter 時，自動執行借/還
        /// </summary>
        private async void txtScanTableware_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // 防止嗶聲

                string tablewareUid = txtScanTableware.Text.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(tablewareUid))
                    return;

                if (string.IsNullOrEmpty(_currentCardUid))
                {
                    lblScanResult.Text = "❌ 請先感應會員卡";
                    lblScanResult.ForeColor = Color.Red;
                    return;
                }

                await ProcessBorrowOrReturn(tablewareUid);
            }
        }

        /// <summary>
        /// 處理借用或歸還邏輯
        /// </summary>
        private async Task ProcessBorrowOrReturn(string tablewareUid)
        {
            try
            {
                lblScanResult.Text = "處理中...";
                lblScanResult.ForeColor = Color.Orange;
                Log($"感應餐具: {tablewareUid}");

                // 步驟1: 檢查餐具是否已註冊
                var checkResponse = await _httpClient.GetAsync($"api/tablewares/check/{tablewareUid}");
                var checkJson = await checkResponse.Content.ReadAsStringAsync();
                var checkResult = JsonSerializer.Deserialize<TablewareCheckResponse>(checkJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (checkResult?.IsRegistered != true)
                {
                    lblScanResult.Text = "❌ 此餐具尚未註冊，請先到「餐具管理」分頁註冊";
                    lblScanResult.ForeColor = Color.Red;
                    Log($"餐具 {tablewareUid} 尚未註冊");
                    ClearAndFocusScan();
                    return;
                }

                // 步驟2: 根據餐具狀態決定借用或歸還
                if (checkResult.Status == "Available")
                {
                    // 餐具可借用 → 執行租借
                    await DoBorrow(tablewareUid);
                }
                else if (checkResult.Status == "Rented")
                {
                    // 餐具已被借用 → 執行歸還
                    await DoReturn(tablewareUid);
                }
                else
                {
                    lblScanResult.Text = $"❌ 餐具狀態異常: {checkResult.Status}";
                    lblScanResult.ForeColor = Color.Red;
                    ClearAndFocusScan();
                }
            }
            catch (Exception ex)
            {
                lblScanResult.Text = $"❌ 錯誤: {ex.Message}";
                lblScanResult.ForeColor = Color.Red;
                Log($"錯誤: {ex.Message}");
                ClearAndFocusScan();
            }
        }

        private async Task DoBorrow(string tablewareUid)
        {
            try
            {
                var request = new { CardId = _currentCardUid, TablewareTagId = tablewareUid };
                var response = await _httpClient.PostAsJsonAsync("api/rentals/borrow", request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    lblScanResult.Text = $"✅ 借用成功！餐具: {tablewareUid}";
                    lblScanResult.ForeColor = Color.DarkGreen;
                    Log($"✅ 借用成功: {tablewareUid}");
                    
                    // 短暫顯示成功訊息後清除
                    await Task.Delay(1500);
                    ClearAndFocusScan();
                }
                else
                {
                    lblScanResult.Text = $"❌ 借用失敗: {json}";
                    lblScanResult.ForeColor = Color.Red;
                    Log($"借用失敗: {json}");
                    ClearAndFocusScan();
                }
            }
            catch (Exception ex)
            {
                lblScanResult.Text = $"❌ 借用錯誤: {ex.Message}";
                lblScanResult.ForeColor = Color.Red;
                Log($"借用錯誤: {ex.Message}");
                ClearAndFocusScan();
            }
        }

        private async Task DoReturn(string tablewareUid)
        {
            try
            {
                var request = new { TablewareTagId = tablewareUid };
                var response = await _httpClient.PostAsJsonAsync("api/rentals/return", request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    lblScanResult.Text = $"✅ 歸還成功！餐具: {tablewareUid}";
                    lblScanResult.ForeColor = Color.DarkBlue;
                    Log($"✅ 歸還成功: {tablewareUid}");
                    
                    // 短暫顯示成功訊息後清除
                    await Task.Delay(1500);
                    ClearAndFocusScan();
                }
                else
                {
                    lblScanResult.Text = $"❌ 歸還失敗: {json}";
                    lblScanResult.ForeColor = Color.Red;
                    Log($"歸還失敗: {json}");
                    ClearAndFocusScan();
                }
            }
            catch (Exception ex)
            {
                lblScanResult.Text = $"❌ 歸還錯誤: {ex.Message}";
                lblScanResult.ForeColor = Color.Red;
                Log($"歸還錯誤: {ex.Message}");
                ClearAndFocusScan();
            }
        }

        private void ClearAndFocusScan()
        {
            txtScanTableware.Clear();
            txtScanTableware.Focus();
        }

        private void Log(string message)
        {
            string logMsg = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lstLog.Items.Insert(0, logMsg);
            if (lstLog.Items.Count > 100) lstLog.Items.RemoveAt(100);
        }

        #endregion

        #region 餐具管理分頁 - 鍵盤輸入模式

        /// <summary>
        /// 處理餐具 UID 輸入框的按鍵事件
        /// 許多讀卡機會在輸出完 UID 後發送 Enter 鍵
        /// </summary>
        private void txtTablewareUid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 防止 Enter 鍵產生嗶聲
                e.SuppressKeyPress = true;

                string uid = txtTablewareUid.Text.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(uid))
                {
                    TablewareLog($"讀取到 UID: {uid}");
                }
            }
        }

        private async void btnRegisterTableware_Click(object? sender, EventArgs e)
        {
            string uid = txtTablewareUid.Text.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(uid))
            {
                MessageBox.Show("請先掃描餐具貼紙，或手動輸入 UID", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTablewareUid.Focus();
                return;
            }

            // 驗證 UID 格式 (應為 HEX 字串)
            if (!IsValidHexUid(uid))
            {
                MessageBox.Show("UID 格式不正確，應為 HEX 字串（例如：649B466C）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTablewareUid.Focus();
                return;
            }

            if (cmbTablewareType.SelectedItem == null)
            {
                MessageBox.Show("請選擇餐具類型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 解析類型
            string typeStr = cmbTablewareType.SelectedItem.ToString()!;
            string type = typeStr.Split(' ')[0]; // "Bowl (碗)" -> "Bowl"

            try
            {
                btnRegisterTableware.Enabled = false;
                TablewareLog($"正在註冊餐具: {uid}, 類型: {type}");

                var request = new { TagId = uid, Type = type };
                var response = await _httpClient.PostAsJsonAsync("api/tablewares/register", request);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TablewareLog($"✅ 註冊成功！");
                    MessageBox.Show($"餐具註冊成功！\nUID: {uid}\n類型: {type}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // 清除輸入框，準備下一個
                    txtTablewareUid.Clear();
                    txtTablewareUid.Focus();
                }
                else
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(json);
                    string message = result.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? json : json;
                    TablewareLog($"❌ 註冊失敗: {message}");
                    MessageBox.Show($"註冊失敗: {message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                TablewareLog($"❌ 錯誤: {ex.Message}");
                MessageBox.Show($"錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegisterTableware.Enabled = true;
            }
        }

        private bool IsValidHexUid(string uid)
        {
            if (string.IsNullOrEmpty(uid) || uid.Length < 4)
                return false;

            return uid.All(c => "0123456789ABCDEFabcdef".Contains(c));
        }

        private void TablewareLog(string message)
        {
            string logMsg = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lstTablewareLog.Items.Insert(0, logMsg);
            if (lstTablewareLog.Items.Count > 100) lstTablewareLog.Items.RemoveAt(100);
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _scanTimer?.Stop();
            _scanTimer?.Dispose();
            _rfidReader?.Disconnect();
            _httpClient?.Dispose();
            base.OnFormClosed(e);
        }
    }

    #region DTOs

    public class CheckCardResponse
    {
        public bool IsRegistered { get; set; }
        public int? UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string? Message { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class TablewareDto
    {
        public int Id { get; set; }
        public string TagId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class TablewareCheckResponse
    {
        public bool IsRegistered { get; set; }
        public int? TablewareId { get; set; }
        public string? TagId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TablewareItem
    {
        public int Id { get; set; }
        public string TagId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string DisplayName => $"{TagId} ({Type})";

        public override string ToString() => DisplayName;
    }

    #endregion
}
