using System.Net.Http.Json;

namespace EcoloopSystem.WinForm.Services
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
        public DateTime Timestamp { get; set; }
    }

    public class RfidStatusResponse
    {
        public bool IsConnected { get; set; }
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

    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl = "http://localhost:5035")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<RfidStatusResponse?> GetStatusAsync()
        {
            var response = await _httpClient.GetAsync("/api/rfid/status");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RfidStatusResponse>();
        }

        public async Task<RfidReadResponse?> ConnectAsync()
        {
            var response = await _httpClient.PostAsync("/api/rfid/connect", null);
            return await response.Content.ReadFromJsonAsync<RfidReadResponse>();
        }

        public async Task<RfidReadResponse?> DisconnectAsync()
        {
            var response = await _httpClient.PostAsync("/api/rfid/disconnect", null);
            return await response.Content.ReadFromJsonAsync<RfidReadResponse>();
        }

        public async Task<RfidReadResponse?> ReadBlockAsync(RfidReadRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/rfid/read", request);
            return await response.Content.ReadFromJsonAsync<RfidReadResponse>();
        }

        public async Task<RfidScanResponse?> ScanDevicesAsync()
        {
            var response = await _httpClient.GetAsync("/api/rfid/scan");
            return await response.Content.ReadFromJsonAsync<RfidScanResponse>();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
