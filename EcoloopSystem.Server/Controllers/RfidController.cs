using Microsoft.AspNetCore.Mvc;
using EcoloopSystem.Server.Models;
using EcoloopSystem.Server.Services;

namespace EcoloopSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RfidController : ControllerBase
    {
        private readonly IRfidReaderService _rfidService;

        public RfidController(IRfidReaderService rfidService)
        {
            _rfidService = rfidService;
        }

        /// <summary>
        /// 取得讀卡器狀態
        /// </summary>
        [HttpGet("status")]
        public ActionResult<RfidStatusResponse> GetStatus()
        {
            return Ok(_rfidService.GetStatus());
        }

        /// <summary>
        /// 掃描可用的 USB RFID 裝置
        /// </summary>
        [HttpGet("scan")]
        public ActionResult<RfidScanResponse> ScanDevices()
        {
            var result = _rfidService.ScanDevices();
            return Ok(result);
        }

        /// <summary>
        /// 連接讀卡器
        /// </summary>
        [HttpPost("connect")]
        public ActionResult<RfidReadResponse> Connect()
        {
            var result = _rfidService.Connect();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// 斷開讀卡器連線
        /// </summary>
        [HttpPost("disconnect")]
        public ActionResult<RfidReadResponse> Disconnect()
        {
            var result = _rfidService.Disconnect();
            return Ok(result);
        }

        /// <summary>
        /// 讀取 Mifare 區塊資料
        /// </summary>
        [HttpPost("read")]
        public ActionResult<RfidReadResponse> ReadBlock([FromBody] RfidReadRequest request)
        {
            var result = _rfidService.ReadBlock(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
