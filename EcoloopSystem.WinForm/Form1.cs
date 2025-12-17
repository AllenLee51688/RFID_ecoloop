using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace EcoloopSystem.WinForm
{
    public partial class Form1 : Form
    {
        // 強制輸入法為英文
        [DllImport("imm32.dll")]
        private static extern bool ImmDisableIME(int idThread);
        
        private readonly HttpClient _httpClient;
        private readonly RFIDReader _rfidReader;
        private System.Windows.Forms.Timer _scanTimer;
        private string? _currentCardUid = null;
        private bool _isScanning = false;
        private int? _currentUserId = null;

        // 餐具讀卡機緩衝區（鍵盤模擬輸入）
        private readonly StringBuilder _tablewareInputBuffer = new StringBuilder();
        private DateTime _lastKeyTime = DateTime.MinValue;
        private const int KEY_INPUT_TIMEOUT_MS = 100; // 按鍵間隔超時（毫秒）
        
        // 餐具輸入延時計時器（偵測輸入完成）
        private System.Windows.Forms.Timer? _tablewareInputTimer;
        
        // 冷卻機制 - 防止同一餐具被連續處理兩次
        private string? _lastProcessedUid = null;
        private DateTime _lastProcessedTime = DateTime.MinValue;
        private const int COOLDOWN_MS = 3000; // 3 秒冷卻時間

        // 讀卡參數 (固定值)
        private const int SECTOR = 0;
        private const int BLOCK = 0;
        private const string KEY_TYPE = "A";
        private const string LOAD_KEY = "FFFFFFFFFFFF";

        public Form1()
        {
            // 強制當前執行緒停用 IME（輸入法），確保鍵盤輸入為英文
            ImmDisableIME(0);
            
            InitializeComponent();
            
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5035");
            _rfidReader = new RFIDReader();

            // 初始化掃描計時器（會員卡）
            _scanTimer = new System.Windows.Forms.Timer();
            _scanTimer.Interval = 1000; // 每 1 秒掃描一次
            _scanTimer.Tick += ScanTimer_Tick;
            
            // 初始化餐具輸入延時計時器（偵測輸入完成）
            _tablewareInputTimer = new System.Windows.Forms.Timer();
            _tablewareInputTimer.Interval = 200; // 200ms 無輸入視為完成
            _tablewareInputTimer.Tick += TablewareInputTimer_Tick;

            // 訂閱全局鍵盤事件
            this.KeyPress += Form1_KeyPress;

            // 程式啟動時自動開始感應
            this.Load += (s, e) => { StartScanning(); FocusTablewareInput(); };
            
            // 當表單獲得焦點時，確保餐具輸入框有焦點
            this.Activated += (s, e) => FocusTablewareInput();
            
            // 當用戶點擊表單時，也聚焦到餐具輸入框
            this.Click += (s, e) => FocusTablewareInput();
        }
        
        /// <summary>
        /// 聚焦餐具輸入框（確保鍵盤輸入正確接收）
        /// </summary>
        private void FocusTablewareInput()
        {
            // 如果用戶不在輸入電話或密碼
            if (ActiveControl != txtPhone && ActiveControl != txtPassword && ActiveControl != txtTablewareUid)
            {
                txtScanTableware.Select();
            }
        }

        #region 全局餐具讀卡機輸入處理

        /// <summary>
        /// 處理全局鍵盤輸入（捕捉餐具讀卡機的鍵盤模擬輸入）
        /// 餐具讀卡機永遠可用，只有在用戶打字時（如註冊表單）才忽略
        /// </summary>
        private void Form1_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // 只有在這些特定輸入框焦點時忽略（用戶正在打字）
            // txtPhone, txtPassword, txtTablewareUid 需要用戶手動輸入
            if (ActiveControl == txtPhone || ActiveControl == txtPassword || ActiveControl == txtTablewareUid)
            {
                return;
            }

            // 檢查是否超時，如果超時則清空緩衝區
            if ((DateTime.Now - _lastKeyTime).TotalMilliseconds > KEY_INPUT_TIMEOUT_MS && _tablewareInputBuffer.Length > 0)
            {
                _tablewareInputBuffer.Clear();
            }
            _lastKeyTime = DateTime.Now;

            // Enter 鍵表示輸入完成
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                string uid = _tablewareInputBuffer.ToString().Trim().ToUpperInvariant();
                _tablewareInputBuffer.Clear();

                if (IsValidHexUid(uid))
                {
                    e.Handled = true;
                    Log($"🔖 感應到餐具: {uid}");
                    
                    // 處理借用或歸還
                    _ = ProcessTablewareScan(uid);
                }
                return;
            }

            // 收集 HEX 字元
            if (char.IsLetterOrDigit(e.KeyChar) && "0123456789ABCDEFabcdef".Contains(e.KeyChar))
            {
                _tablewareInputBuffer.Append(e.KeyChar);
                
                // 同時更新 txtScanTableware（如果可見）
                if (pnlBorrowReturn.Visible)
                {
                    txtScanTableware.Text = _tablewareInputBuffer.ToString();
                }
                
                e.Handled = true;
            }
        }

        /// <summary>
        /// 處理餐具掃描（自動判斷借用或歸還）
        /// </summary>
        private async Task ProcessTablewareScan(string tablewareUid)
        {
            try
            {
                // 冷卻檢查 - 防止同一餐具在短時間內被重複處理
                if (_lastProcessedUid == tablewareUid && 
                    (DateTime.Now - _lastProcessedTime).TotalMilliseconds < COOLDOWN_MS)
                {
                    Log($"跳過重複處理: {tablewareUid} (冷卻中)");
                    txtScanTableware.Clear(); // 清除輸入框
                    return;
                }

                // 更新 UI 顯示
                if (pnlBorrowReturn.Visible)
                {
                    txtScanTableware.Text = tablewareUid;
                }

                // 步驟1: 檢查餐具是否已註冊
                var checkResponse = await _httpClient.GetAsync($"api/tablewares/check/{tablewareUid}");
                var checkJson = await checkResponse.Content.ReadAsStringAsync();
                var checkResult = JsonSerializer.Deserialize<TablewareCheckResponse>(checkJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (checkResult?.IsRegistered != true)
                {
                    ShowResult($"❌ 餐具 {tablewareUid} 尚未註冊", Color.Red);
                    Log($"餐具 {tablewareUid} 尚未註冊");
                    return;
                }

                // 步驟2: 根據餐具狀態決定借用或歸還
                if (checkResult.Status == "Available")
                {
                    // 餐具可借用 → 需要會員卡
                    if (string.IsNullOrEmpty(_currentCardUid))
                    {
                        ShowResult($"⚠️ 借用需要先感應會員卡！餐具: {tablewareUid}", Color.Orange);
                        Log($"借用失敗: 尚未感應會員卡");
                        return;
                    }
                    await DoBorrow(tablewareUid);
                }
                else if (checkResult.Status == "Rented")
                {
                    // 餐具已被借用 → 直接歸還（不需要會員卡）
                    await DoReturn(tablewareUid);
                }
                else
                {
                    ShowResult($"❌ 餐具狀態異常: {checkResult.Status}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                ShowResult($"❌ 錯誤: {ex.Message}", Color.Red);
                Log($"錯誤: {ex.Message}");
            }
        }

        private void ShowResult(string message, Color color)
        {
            if (pnlBorrowReturn.Visible)
            {
                lblScanResult.Text = message;
                lblScanResult.ForeColor = color;
            }
            else
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        #endregion

        #region 租借分頁 - 會員卡感應

        /// <summary>
        /// 清除會員按鈕
        /// </summary>
        private void btnClearMember_Click(object? sender, EventArgs e)
        {
            ClearMember();
            Log("已清除會員，等待下一位...");
        }

        /// <summary>
        /// 清除當前會員狀態（重新啟動讀卡機感應）
        /// </summary>
        private void ClearMember()
        {
            _currentCardUid = null;
            _currentUserId = null;
            lblCardUid.Text = "---";
            lblStatus.Text = "感應中...請放置會員卡，或直接感應餐具歸還";
            lblStatus.ForeColor = Color.Blue;
            pnlRegister.Visible = false;
            // pnlBorrowReturn 永久顯示
            lblScanResult.Text = "";
            ClearScanInput();
            
            // 重新啟動會員卡感應計時器
            if (_isScanning)
            {
                _scanTimer.Start();
            }
            
            // 聚焦餐具輸入框
            txtScanTableware.Focus();
        }

        private void StartScanning()
        {
            _isScanning = true;
            _scanTimer.Start();
            Log("系統啟動 - 等待會員卡或餐具...");
        }
        
        private bool _isReadingCard = false; // 防止重複讀取

        private async void ScanTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isScanning) return;
            if (_isReadingCard) return; // 如果正在讀取中，跳過
            
            // 當第二台讀卡機（餐具）有輸入時，暫停讀取會員卡
            if (!string.IsNullOrEmpty(txtScanTableware.Text))
            {
                return; // 跳過這次讀取，避免干擾餐具輸入
            }

            try
            {
                _isReadingCard = true;
                
                // 在背景執行緒讀取會員卡（不阻塞 UI 執行緒）
                string result = await Task.Run(() => _rfidReader.ReadCardUID());
                
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
                        lblStatus.Text = "感應中...請放置會員卡，或直接感應餐具歸還";
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
            finally
            {
                _isReadingCard = false;
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
                    lblStatus.Text = $"✅ 歡迎！手機: {result.PhoneNumber}";
                    lblStatus.ForeColor = Color.Green;
                    pnlRegister.Visible = false;
                    pnlBorrowReturn.Visible = true;
                    txtScanTableware.Clear();
                    lblScanResult.Text = "請將餐具靠近讀卡機...";
                    lblScanResult.ForeColor = Color.Gray;
                    Log($"已註冊使用者，ID: {result.UserId}");
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
        /// 當餐具輸入框內容變化時，重置延時計時器
        /// </summary>
        private void txtScanTableware_TextChanged(object? sender, EventArgs e)
        {
            // 每次輸入變化時重置計時器
            _tablewareInputTimer?.Stop();
            _tablewareInputTimer?.Start();
        }
        
        /// <summary>
        /// 延時計時器觸發 - 輸入完成，自動處理
        /// </summary>
        private async void TablewareInputTimer_Tick(object? sender, EventArgs e)
        {
            _tablewareInputTimer?.Stop();
            
            string uid = txtScanTableware.Text.Trim().ToUpperInvariant();
            if (IsValidHexUid(uid))
            {
                Log($"🔖 自動偵測到餐具: {uid}");
                await ProcessTablewareScan(uid);
            }
        }

        /// <summary>
        /// 處理餐具感應輸入框的按鍵事件（Enter 鍵）
        /// </summary>
        private async void txtScanTableware_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _tablewareInputTimer?.Stop(); // 停止計時器，避免重複處理

                string tablewareUid = txtScanTableware.Text.Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(tablewareUid) && IsValidHexUid(tablewareUid))
                {
                    await ProcessTablewareScan(tablewareUid);
                }
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
                    ShowResult($"✅ 借用成功！餐具: {tablewareUid}", Color.DarkGreen);
                    Log($"✅ 借用成功: {tablewareUid}");
                    
                    // 記錄冷卻資訊並立即清除輸入框
                    _lastProcessedUid = tablewareUid;
                    _lastProcessedTime = DateTime.Now;
                    txtScanTableware.Clear();
                    
                    // 短暫顯示成功訊息後，清除會員等待下一位
                    await Task.Delay(2000);
                    ClearMember();
                    Log("等待下一位會員靠卡...");
                }
                else
                {
                    ShowResult($"❌ 借用失敗: {json}", Color.Red);
                    Log($"借用失敗: {json}");
                    ClearScanInput();
                }
            }
            catch (Exception ex)
            {
                ShowResult($"❌ 借用錯誤: {ex.Message}", Color.Red);
                Log($"借用錯誤: {ex.Message}");
                ClearScanInput();
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
                    ShowResult($"✅ 歸還成功！餐具: {tablewareUid}", Color.DarkBlue);
                    Log($"✅ 歸還成功: {tablewareUid}");
                    
                    // 記錄冷卻資訊並立即清除輸入框
                    _lastProcessedUid = tablewareUid;
                    _lastProcessedTime = DateTime.Now;
                    txtScanTableware.Clear();
                    
                    // 短暫顯示成功訊息後，繼續等待
                    await Task.Delay(2000);
                    
                    // 如果有會員登入，保持登入狀態
                    if (!string.IsNullOrEmpty(_currentCardUid))
                    {
                        lblScanResult.Text = "請將餐具靠近讀卡機...";
                        lblScanResult.ForeColor = Color.Gray;
                    }
                    else
                    {
                        // 沒有會員登入，重置為初始狀態
                        lblStatus.Text = "感應中...請放置會員卡，或直接感應餐具歸還";
                        lblStatus.ForeColor = Color.Blue;
                    }
                }
                else
                {
                    ShowResult($"❌ 歸還失敗: {json}", Color.Red);
                    Log($"歸還失敗: {json}");
                    ClearScanInput();
                }
            }
            catch (Exception ex)
            {
                ShowResult($"❌ 歸還錯誤: {ex.Message}", Color.Red);
                Log($"歸還錯誤: {ex.Message}");
                ClearScanInput();
            }
        }

        /// <summary>
        /// 重置狀態，繼續感應下一位會員的卡片
        /// </summary>
        private void ResumeScanning()
        {
            _currentCardUid = null;
            _currentUserId = null;
            lblCardUid.Text = "---";
            lblStatus.Text = "感應中...請放置會員卡，或直接感應餐具歸還";
            lblStatus.ForeColor = Color.Blue;
            pnlRegister.Visible = false;
            // pnlBorrowReturn 永久顯示
            lblScanResult.Text = "";
            ClearScanInput();
            
            if (_isScanning)
            {
                _scanTimer.Start();
                Log("等待下一位會員靠卡...");
            }
        }

        private void ClearScanInput()
        {
            txtScanTableware.Clear();
            _tablewareInputBuffer.Clear();
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
        /// </summary>
        private void txtTablewareUid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
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

            string typeStr = cmbTablewareType.SelectedItem.ToString()!;
            string type = typeStr.Split(' ')[0];

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
