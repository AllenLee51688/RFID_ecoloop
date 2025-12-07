using System.Runtime.InteropServices;

namespace EcoloopSystem.Server.Hardware
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MW_EasyPOD
    {
        public uint VID;
        public uint PID;
        public uint ReadTimeOut;
        public uint WriteTimeOut;
        public uint Handle;
        public uint FeatureReportSize;
        public uint InputReportSize;
        public uint OutputReportSize;
    }

    public static class EasyPodNativeMethods
    {
        private const string DllName = "EasyPOD.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe uint ConnectPOD(MW_EasyPOD* pEasyPOD, uint Index);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe uint WriteData(MW_EasyPOD* pEasyPOD, byte[] lpBuffer, 
            uint nNumberOfBytesToWrite, uint* lpNumberOfBytesWritten);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe uint ReadData(MW_EasyPOD* pEasyPOD, byte[] lpBuffer, 
            uint nNumberOfBytesToRead, uint* lpNumberOfBytesRead);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe uint DisconnectPOD(MW_EasyPOD* pEasyPOD);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe uint ClearPODBuffer(MW_EasyPOD* pEasyPOD);
    }

    public interface IRfidHardware
    {
        bool Connect(uint deviceIndex = 1);
        void Disconnect();
        bool IsConnected { get; }
        string ReadBlock(int sector, int block, string keyType, string loadKey, uint deviceIndex = 1);
        List<(int Index, string Status)> ScanDevices(int maxDevices = 5);
    }

    public class MifareReader : IRfidHardware, IDisposable
    {
        private MW_EasyPOD _easyPod;
        private bool _isConnected;
        private readonly object _lockObject = new();

        public bool IsConnected => _isConnected;

        public unsafe bool Connect(uint deviceIndex = 1)
        {
            lock (_lockObject)
            {
                if (_isConnected) return true;

                _easyPod.VID = 0x0E6A;
                _easyPod.PID = 0x0317;
                _easyPod.ReadTimeOut = 2000;
                _easyPod.WriteTimeOut = 1000;

                fixed (MW_EasyPOD* pPod = &_easyPod)
                {
                    uint result = EasyPodNativeMethods.ConnectPOD(pPod, deviceIndex);
                    _isConnected = result == 0;
                    return _isConnected;
                }
            }
        }

        public unsafe void Disconnect()
        {
            lock (_lockObject)
            {
                if (!_isConnected) return;

                fixed (MW_EasyPOD* pPod = &_easyPod)
                {
                    EasyPodNativeMethods.DisconnectPOD(pPod);
                }
                _isConnected = false;
            }
        }

        public unsafe List<(int Index, string Status)> ScanDevices(int maxDevices = 5)
        {
            var devices = new List<(int Index, string Status)>();
            
            for (int i = 1; i <= maxDevices; i++)
            {
                var tempPod = new MW_EasyPOD
                {
                    VID = 0x0E6A,
                    PID = 0x0317,
                    ReadTimeOut = 500,
                    WriteTimeOut = 500
                };

                try
                {
                    uint result = EasyPodNativeMethods.ConnectPOD(&tempPod, (uint)i);
                    if (result == 0)
                    {
                        devices.Add((i, "可用"));
                        EasyPodNativeMethods.DisconnectPOD(&tempPod);
                    }
                }
                catch
                {
                    // 裝置不可用，跳過
                }
            }

            return devices;
        }

        public unsafe string ReadBlock(int sector, int block, string keyType, string loadKey, uint deviceIndex = 1)
        {
            byte[]? keyBytes = HexStringToByteArray(loadKey);
            if (keyBytes?.Length != 6)
                throw new ArgumentException("金鑰格式錯誤，必須為 12 位 HEX 字串");

            byte[] writeBuffer = new byte[]
            {
                0x02, 0x0A, 0x15,
                (byte)(keyType == "A" ? 0x60 : 0x61),
                keyBytes[0], keyBytes[1], keyBytes[2], keyBytes[3], keyBytes[4], keyBytes[5],
                (byte)sector, (byte)block
            };

            // 每次讀取都建立新連線，使用指定的 deviceIndex
            var tempPod = new MW_EasyPOD
            {
                VID = 0x0E6A,
                PID = 0x0317,
                ReadTimeOut = 2000,
                WriteTimeOut = 1000
            };

            uint dwResult = EasyPodNativeMethods.ConnectPOD(&tempPod, deviceIndex);
            if (dwResult != 0)
            {
                throw new InvalidOperationException($"無法連接裝置 {deviceIndex}，錯誤碼: {dwResult}");
            }

            try
            {
                uint written = 0;
                EasyPodNativeMethods.WriteData(&tempPod, writeBuffer, 12, &written);

                byte[] readBuffer = new byte[64];
                uint read = 0;
                EasyPodNativeMethods.ReadData(&tempPod, readBuffer, 64, &read);

                return ParseResponse(readBuffer, (int)read, sector, block, keyType);
            }
            finally
            {
                EasyPodNativeMethods.ClearPODBuffer(&tempPod);
                EasyPodNativeMethods.DisconnectPOD(&tempPod);
            }
        }

        private string ParseResponse(byte[] response, int length, int sector, int block, string keyType)
        {
            if (length < 4)
                return $"回應資料不足";

            byte status = response[3];
            
            return status switch
            {
                0x00 when length >= 20 => BitConverter.ToString(response, 4, 16).Replace("-", ""),
                0x01 => throw new InvalidOperationException($"無卡片或金鑰 {keyType} 無效"),
                _ => throw new InvalidOperationException($"讀取錯誤，狀態碼: 0x{status:X2}")
            };
        }

        private static byte[]? HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0) return null;
            
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }
    }
}
