using System.Text;

namespace EcoloopSystem.WinForm
{
    public class RFIDReader
    {
        private MW_EasyPOD easyPOD;
        private bool isConnected = false;
        public event EventHandler<string>? DataReceived;

        public unsafe void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    fixed (MW_EasyPOD* pEasyPOD = &easyPOD)
                    {
                        PODfuncs.DisconnectPOD(pEasyPOD);
                    }
                }
                catch { }
            }
            isConnected = false;
        }

        static byte[]? HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0) return null;
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        private string ParseMifareBlockResponse(byte[] response, int length)
        {
            if (length < 4) return "回應資料不足";

            byte stx = response[0];
            byte len = response[1];
            byte cmd = response[2];
            byte status = response[3];

            string debugInfo = $"回應解析: STX:{stx:X2} LEN:{len:X2} CMD:{cmd:X2} STATUS:{status:X2}\n";

            // 格式驗證
            if (stx != 0x02 || cmd != 0x15)
                return debugInfo + "❌ 回應格式不正確";

            // 狀態處理
            return status switch
            {
                0x00 => ProcessMifareSuccess(response, length, len, debugInfo),
                0x01 => debugInfo + "❌ 無卡片或無效金鑰 (手冊狀態碼0x01)",
                0x10 => debugInfo + "❌ 指令錯誤 (手冊狀態碼0x10)",
                _ => debugInfo + $"❌ 未知錯誤狀態: {status:X2}"
            };
        }

        // 提取 MIFARE 成功處理邏輯
        private string ProcessMifareSuccess(byte[] response, int length, byte len, string debugInfo)
        {
            // 標準 16 位元組回應
            if (len == 0x12 && length >= 20)
            {
                byte[] blockData = new byte[16];
                Array.Copy(response, 4, blockData, 0, 16);
                return BitConverter.ToString(blockData).Replace("-", "");
            }

            // 其他長度回應
            int dataLength = len - 2;
            if (dataLength > 0 && length >= (4 + dataLength))
            {
                byte[] blockData = new byte[dataLength];
                Array.Copy(response, 4, blockData, 0, dataLength);
                string result = BitConverter.ToString(blockData).Replace("-", "");
                return debugInfo + $"✅ 區塊資料 ({dataLength}字節): {result}\n" +
                       $"期望16字節，但得到{dataLength}字節";
            }

            return debugInfo + $"資料長度不符: LEN={len:X2}，期望0x12 (18)，實際收到{length}字節";
        }
        /// <summary>
        /// 讀取卡片 UID (Card ID)
        /// MIFARE 卡的 UID 儲存在 Sector 0, Block 0 的前 4 個字節
        /// </summary>
        /// <returns>卡片 UID 的 HEX 字串，或錯誤訊息</returns>
        public unsafe string ReadCardUID()
        {
            // 使用預設金鑰讀取 Sector 0, Block 0
            string defaultKey = "FFFFFFFFFFFF";
            byte[]? keyBytes = HexStringToByteArray(defaultKey);
            if (keyBytes?.Length != 6) return "❌ 金鑰格式錯誤";

            // 讀取 Sector 0, Block 0 (包含 UID)
            byte[] WriteBuffer = {
                0x02, 0x0A, 0x15,
                0x60, // Key A
                keyBytes[0], keyBytes[1], keyBytes[2], keyBytes[3], keyBytes[4], keyBytes[5],
                0x00, // Sector 0
                0x00  // Block 0
            };

            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                uint dwResult = PODfuncs.ConnectPOD(pPOD, 1);

                try
                {
                    easyPOD.ReadTimeOut = 2000;
                    easyPOD.WriteTimeOut = 1000;

                    UInt32 uiWritten = 0, uiRead = 0;
                    PODfuncs.WriteData(pPOD, WriteBuffer, 12, &uiWritten);

                    byte[] ReadBuffer = new byte[64];
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);

                    if (uiRead < 4)
                        return "❌ 回應資料不足，請確認卡片是否放置正確";

                    byte status = ReadBuffer[3];

                    if (status == 0x00)
                    {
                        // 成功：從 Block 0 提取 UID (前 4 個字節)
                        byte len = ReadBuffer[1];
                        if (len >= 0x06 && uiRead >= 8)
                        {
                            // UID 位於回應資料的前 4 個字節
                            byte[] uid = new byte[4];
                            Array.Copy(ReadBuffer, 4, uid, 0, 4);
                            return BitConverter.ToString(uid).Replace("-", "");
                        }
                        return "❌ UID 資料長度異常";
                    }

                    return status switch
                    {
                        0x01 => "❌ 無卡片",
                        0x10 => "❌ 指令錯誤",
                        _ => $"❌ 錯誤狀態: 0x{status:X2}"
                    };
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }

        /// <summary>
        /// 原讀取方式
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="block"></param>
        /// <param name="keyType"></param>
        /// <param name="loadKey"></param>
        /// <returns></returns>
        public unsafe string ReadMifareSpecificBlock(int sector, int block, string keyType, string loadKey)
        {
            byte[]? keyBytes = HexStringToByteArray(loadKey);
            if (keyBytes?.Length != 6) return "金鑰格式錯誤";

            // 直接建立 WriteBuffer 陣列
            byte[] WriteBuffer = {
                0x02, 0x0A, 0x15,
                (byte)(keyType == "A" ? 0x60 : 0x61),
                keyBytes[0], keyBytes[1], keyBytes[2], keyBytes[3], keyBytes[4], keyBytes[5],
                (byte)sector, (byte)block
            };

            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                uint dwResult = PODfuncs.ConnectPOD(pPOD, 1);

                try
                {
                    easyPOD.ReadTimeOut = 2000;
                    easyPOD.WriteTimeOut = 1000;

                    UInt32 uiWritten = 0, uiRead = 0;
                    PODfuncs.WriteData(pPOD, WriteBuffer, 12, &uiWritten);

                    byte[] ReadBuffer = new byte[64];
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);

                    if (uiRead < 4)
                        return $"❌ 扇區{sector}區塊{block} 金鑰{keyType} 回應資料不足";

                    byte status = ReadBuffer[3];
                    return status switch
                    {
                        0x00 => ParseMifareBlockResponse(ReadBuffer, (int)uiRead),
                        0x01 => $"❌ 扇區{sector}區塊{block} 無卡片或金鑰{keyType}無效",
                        _ => $"❌ 扇區{sector}區塊{block} 金鑰{keyType} 錯誤狀態: 0x{status:X2}"
                    };
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }
    }
}
