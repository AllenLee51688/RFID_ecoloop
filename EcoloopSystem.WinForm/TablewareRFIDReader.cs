using System.IO.Ports;
using System.Text;

namespace EcoloopSystem.WinForm
{
    /// <summary>
    /// 餐具 RFID 讀卡機 (透過 COM Port 通訊)
    /// 用於讀取貼在餐具上的 RFID 貼紙 UID
    /// </summary>
    public class TablewareRFIDReader : IDisposable
    {
        private SerialPort? _serialPort;
        private bool _isConnected = false;
        private readonly StringBuilder _buffer = new StringBuilder();
        private System.Threading.Timer? _readTimer;

        /// <summary>
        /// 當讀取到 RFID 標籤時觸發
        /// </summary>
        public event EventHandler<string>? TagRead;

        /// <summary>
        /// 當連線狀態改變時觸發
        /// </summary>
        public event EventHandler<bool>? ConnectionChanged;

        /// <summary>
        /// 當發生錯誤時觸發
        /// </summary>
        public event EventHandler<string>? ErrorOccurred;

        /// <summary>
        /// 取得可用的 COM Port 清單
        /// </summary>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames().OrderBy(p => p).ToArray();
        }

        /// <summary>
        /// 是否已連接
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 連接到指定的 COM Port
        /// </summary>
        /// <param name="portName">COM Port 名稱 (例如 COM3)</param>
        /// <param name="baudRate">鮑率 (預設 9600)</param>
        public void Connect(string portName, int baudRate = 9600)
        {
            try
            {
                if (_isConnected)
                {
                    Disconnect();
                }

                _serialPort = new SerialPort(portName)
                {
                    BaudRate = baudRate,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000,
                    Encoding = Encoding.ASCII
                };

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.ErrorReceived += SerialPort_ErrorReceived;
                _serialPort.Open();

                _isConnected = true;
                ConnectionChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                _isConnected = false;
                ErrorOccurred?.Invoke(this, $"連接失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 中斷連接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _readTimer?.Dispose();
                _readTimer = null;

                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            catch { }
            finally
            {
                _isConnected = false;
                ConnectionChanged?.Invoke(this, false);
            }
        }

        /// <summary>
        /// 主動讀取一次（輪詢模式）
        /// </summary>
        public string? TryRead()
        {
            if (!_isConnected || _serialPort == null || !_serialPort.IsOpen)
            {
                return null;
            }

            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    string data = _serialPort.ReadExisting();
                    return ProcessData(data);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"讀取錯誤: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 處理接收的資料，提取 UID
        /// </summary>
        private string? ProcessData(string data)
        {
            _buffer.Append(data);
            string bufferContent = _buffer.ToString();

            // 嘗試解析 UID
            // 許多讀卡機會以換行符結束一次完整的讀取
            if (bufferContent.Contains('\r') || bufferContent.Contains('\n'))
            {
                string[] lines = bufferContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                _buffer.Clear();

                foreach (string line in lines)
                {
                    string uid = ExtractUID(line.Trim());
                    if (!string.IsNullOrEmpty(uid))
                    {
                        return uid;
                    }
                }
            }
            // 如果 buffer 太長，嘗試直接解析
            else if (bufferContent.Length >= 8)
            {
                string uid = ExtractUID(bufferContent.Trim());
                if (!string.IsNullOrEmpty(uid))
                {
                    _buffer.Clear();
                    return uid;
                }
            }

            return null;
        }

        /// <summary>
        /// 從原始資料提取 UID
        /// 支援多種常見格式
        /// </summary>
        private string ExtractUID(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData))
                return string.Empty;

            // 移除常見的前綴/後綴符號
            string cleaned = rawData
                .Replace(" ", "")
                .Replace(":", "")
                .Replace("-", "")
                .Trim();

            // 驗證是否為有效的 HEX 字串 (通常 UID 為 4-10 bytes)
            if (cleaned.Length >= 8 && cleaned.Length <= 20 && IsHexString(cleaned))
            {
                return cleaned.ToUpperInvariant();
            }

            return string.Empty;
        }

        private bool IsHexString(string str)
        {
            return str.All(c => "0123456789ABCDEFabcdef".Contains(c));
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

                string data = _serialPort.ReadExisting();
                string? uid = ProcessData(data);

                if (!string.IsNullOrEmpty(uid))
                {
                    TagRead?.Invoke(this, uid);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"資料處理錯誤: {ex.Message}");
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorOccurred?.Invoke(this, $"串口錯誤: {e.EventType}");
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
