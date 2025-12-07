using EcoloopSystem.Server.Hardware;
using EcoloopSystem.Server.Models;

namespace EcoloopSystem.Server.Services
{
    public interface IRfidReaderService
    {
        RfidStatusResponse GetStatus();
        RfidReadResponse Connect();
        RfidReadResponse Disconnect();
        RfidReadResponse ReadBlock(RfidReadRequest request);
        RfidScanResponse ScanDevices();
    }

    public class RfidReaderService : IRfidReaderService, IDisposable
    {
        private readonly IRfidHardware _hardware;

        public RfidReaderService()
        {
            _hardware = new MifareReader();
        }

        public RfidStatusResponse GetStatus()
        {
            return new RfidStatusResponse
            {
                IsConnected = _hardware.IsConnected,
                DeviceInfo = new DeviceInfo()
            };
        }

        public RfidReadResponse Connect()
        {
            try
            {
                bool result = _hardware.Connect();
                return new RfidReadResponse
                {
                    Success = result,
                    Message = result ? "連接成功" : "連接失敗"
                };
            }
            catch (Exception ex)
            {
                return new RfidReadResponse
                {
                    Success = false,
                    Message = $"連接錯誤: {ex.Message}"
                };
            }
        }

        public RfidReadResponse Disconnect()
        {
            try
            {
                _hardware.Disconnect();
                return new RfidReadResponse
                {
                    Success = true,
                    Message = "已斷開連線"
                };
            }
            catch (Exception ex)
            {
                return new RfidReadResponse
                {
                    Success = false,
                    Message = $"斷開連線錯誤: {ex.Message}"
                };
            }
        }

        public RfidScanResponse ScanDevices()
        {
            try
            {
                var devices = _hardware.ScanDevices(5);
                return new RfidScanResponse
                {
                    Success = devices.Count > 0,
                    Message = devices.Count > 0 ? $"找到 {devices.Count} 個裝置" : "未找到裝置",
                    Devices = devices.Select(d => new RfidDeviceInfo { Index = d.Index, Status = d.Status }).ToList()
                };
            }
            catch (Exception ex)
            {
                return new RfidScanResponse
                {
                    Success = false,
                    Message = $"掃描錯誤: {ex.Message}",
                    Devices = new List<RfidDeviceInfo>()
                };
            }
        }

        public RfidReadResponse ReadBlock(RfidReadRequest request)
        {
            try
            {
                // 驗證輸入
                if (request.Sector < 0 || request.Sector > 15)
                    throw new ArgumentException("Sector 必須在 0-15 之間");
                if (request.Block < 0 || request.Block > 3)
                    throw new ArgumentException("Block 必須在 0-3 之間");
                if (request.LoadKey?.Length != 12)
                    throw new ArgumentException("LoadKey 必須為 12 位 HEX 字串");

                string data = _hardware.ReadBlock(
                    request.Sector, 
                    request.Block, 
                    request.KeyType, 
                    request.LoadKey,
                    (uint)request.DeviceIndex);

                return new RfidReadResponse
                {
                    Success = true,
                    Data = data,
                    Message = "讀取成功"
                };
            }
            catch (Exception ex)
            {
                return new RfidReadResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public void Dispose()
        {
            (_hardware as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
