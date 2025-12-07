namespace EcoloopSystem.Server.Models
{
    public class RfidReadRequest
    {
        public int DeviceIndex { get; set; } = 1;
        public int Sector { get; set; }
        public int Block { get; set; }
        public string KeyType { get; set; } = "A";
        public string LoadKey { get; set; } = "FFFFFFFFFFFF";
    }

    public class RfidReadResponse
    {
        public bool Success { get; set; }
        public string? Data { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class RfidStatusResponse
    {
        public bool IsConnected { get; set; }
        public DeviceInfo? DeviceInfo { get; set; }
    }

    public class DeviceInfo
    {
        public string Vid { get; set; } = "0x0E6A";
        public string Pid { get; set; } = "0x0317";
    }

    public class RfidDeviceInfo
    {
        public int Index { get; set; }
        public string Status { get; set; } = "";
    }

    public class RfidScanResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<RfidDeviceInfo>? Devices { get; set; }
    }
}
