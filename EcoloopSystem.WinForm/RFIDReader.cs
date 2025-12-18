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
        /// 方式1: Anti-Collision 直接讀取 UID（適用於大多數卡片）
        /// 方式2: 讀取 MIFARE Block 0（需要金鑰認證）
        /// </summary>
        /// <returns>卡片 UID 的 HEX 字串，或錯誤訊息</returns>
        public unsafe string ReadCardUID()
        {
            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;
            //DebugReadCard();
            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                uint dwResult = PODfuncs.ConnectPOD(pPOD, 1);

                try
                {
                    easyPOD.ReadTimeOut = 2000;
                    easyPOD.WriteTimeOut = 1000;

                    // 方式1: 嘗試 Anti-Collision 讀取 UID (命令 0x12)
                    // 這個方式不需要金鑰，適用於大多數 RFID 卡片
                    byte[] antiCollisionCmd = { 0x02, 0x02, 0x12, 0x00 };
                    
                    UInt32 uiWritten = 0, uiRead = 0;
                    PODfuncs.WriteData(pPOD, antiCollisionCmd, 4, &uiWritten);

                    byte[] ReadBuffer = new byte[64];
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);

                    if (uiRead >= 4 && ReadBuffer[3] == 0x00)
                    {
                        // Anti-Collision 成功
                        byte len = ReadBuffer[1];
                        int uidLength = len - 2; // 扣除 CMD 和 STATUS
                        
                        if (uidLength >= 4 && uiRead >= (4 + uidLength))
                        {
                            byte[] uid = new byte[uidLength];
                            Array.Copy(ReadBuffer, 4, uid, 0, uidLength);
                            return BitConverter.ToString(uid).Replace("-", "");
                        }
                    }

                    // 清除緩衝區，準備嘗試方式2
                    PODfuncs.ClearPODBuffer(pPOD);

                    // 方式2: 使用 MIFARE Block 讀取 (需要金鑰認證)
                    string defaultKey = "FFFFFFFFFFFF";
                    byte[]? keyBytes = HexStringToByteArray(defaultKey);
                    if (keyBytes?.Length != 6) return "❌ 金鑰格式錯誤";

                    byte[] mifareCmd = {
                        0x02, 0x0A, 0x15,
                        0x60, // Key A
                        keyBytes[0], keyBytes[1], keyBytes[2], keyBytes[3], keyBytes[4], keyBytes[5],
                        0x00, // Sector 0
                        0x00  // Block 0
                    };

                    uiWritten = 0; uiRead = 0;
                    PODfuncs.WriteData(pPOD, mifareCmd, 12, &uiWritten);
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);

                    if (uiRead < 4)
                        return "❌ 無卡片或讀取失敗";

                    byte status = ReadBuffer[3];

                    if (status == 0x00)
                    {
                        byte len = ReadBuffer[1];
                        if (len >= 0x06 && uiRead >= 8)
                        {
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

        /// <summary>
        /// Debug 用：測試不同命令並顯示原始回應
        /// 可用來測試讀卡機支援的協議
        /// </summary>
        /// <returns>詳細的 debug 資訊</returns>
        public unsafe string DebugReadCard()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RFID 讀卡機 Debug 測試 ===\n");

            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                uint dwResult = PODfuncs.ConnectPOD(pPOD, 1);
                sb.AppendLine($"連線結果: {dwResult}");

                try
                {
                    easyPOD.ReadTimeOut = 3000;
                    easyPOD.WriteTimeOut = 1000;

                    byte[] ReadBuffer = new byte[64];
                    UInt32 uiWritten = 0, uiRead = 0;

                    // 測試1: Anti-Collision (命令 0x12)
                    sb.AppendLine("\n--- 測試1: Anti-Collision (0x12) ---");
                    byte[] cmd1 = { 0x02, 0x02, 0x12, 0x00 };
                    sb.AppendLine($"發送: {BitConverter.ToString(cmd1)}");
                    PODfuncs.WriteData(pPOD, cmd1, 4, &uiWritten);
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);
                    sb.AppendLine($"讀取 {uiRead} 字節: {BitConverter.ToString(ReadBuffer, 0, (int)Math.Min(uiRead, 20))}");
                    if (uiRead >= 4) sb.AppendLine($"Status: 0x{ReadBuffer[3]:X2}");
                    
                    PODfuncs.ClearPODBuffer(pPOD);
                    System.Threading.Thread.Sleep(100);

                    // 測試2: Request A (命令 0x11) - ISO14443A
                    sb.AppendLine("\n--- 測試2: Request A (0x11) ---");
                    byte[] cmd2 = { 0x02, 0x02, 0x11, 0x26 }; // 0x26 = REQA
                    sb.AppendLine($"發送: {BitConverter.ToString(cmd2)}");
                    PODfuncs.WriteData(pPOD, cmd2, 4, &uiWritten);
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);
                    sb.AppendLine($"讀取 {uiRead} 字節: {BitConverter.ToString(ReadBuffer, 0, (int)Math.Min(uiRead, 20))}");
                    if (uiRead >= 4) sb.AppendLine($"Status: 0x{ReadBuffer[3]:X2}");
                    
                    PODfuncs.ClearPODBuffer(pPOD);
                    System.Threading.Thread.Sleep(100);

                    // 測試3: Get Version (命令 0x01)
                    sb.AppendLine("\n--- 測試3: Get Version (0x01) ---");
                    byte[] cmd3 = { 0x02, 0x01, 0x01 };
                    sb.AppendLine($"發送: {BitConverter.ToString(cmd3)}");
                    PODfuncs.WriteData(pPOD, cmd3, 3, &uiWritten);
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);
                    sb.AppendLine($"讀取 {uiRead} 字節: {BitConverter.ToString(ReadBuffer, 0, (int)Math.Min(uiRead, 20))}");
                    
                    PODfuncs.ClearPODBuffer(pPOD);
                    System.Threading.Thread.Sleep(100);

                    // 測試4: MIFARE Read Block 0 with Key A
                    sb.AppendLine("\n--- 測試4: MIFARE Block 0 (Key A = FFFFFFFFFFFF) ---");
                    byte[] cmd4 = { 0x02, 0x0A, 0x15, 0x60, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00 };
                    sb.AppendLine($"發送: {BitConverter.ToString(cmd4)}");
                    PODfuncs.WriteData(pPOD, cmd4, 12, &uiWritten);
                    PODfuncs.ReadData(pPOD, ReadBuffer, 64, &uiRead);
                    sb.AppendLine($"讀取 {uiRead} 字節: {BitConverter.ToString(ReadBuffer, 0, (int)Math.Min(uiRead, 24))}");
                    if (uiRead >= 4) sb.AppendLine($"Status: 0x{ReadBuffer[3]:X2}");

                    sb.AppendLine("\n=== 測試完成 ===");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"\n錯誤: {ex.Message}");
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }

            return sb.ToString();
        }
    }
}
