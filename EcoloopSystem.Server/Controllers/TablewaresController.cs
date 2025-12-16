using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoloopSystem.Server.Data;
using EcoloopSystem.Server.Models;

namespace EcoloopSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablewaresController : ControllerBase
    {
        private readonly EcoloopContext _context;

        public TablewaresController(EcoloopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有餐具清單
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TablewareDto>>> GetTablewares()
        {
            var tablewares = await _context.Tablewares
                .Select(t => new TablewareDto
                {
                    Id = t.Id,
                    TagId = t.TagId,
                    Type = t.Type.ToString(),
                    Status = t.Status.ToString()
                })
                .ToListAsync();

            return Ok(tablewares);
        }

        /// <summary>
        /// 檢查餐具是否已註冊
        /// </summary>
        [HttpGet("check/{tagId}")]
        public async Task<ActionResult<TablewareCheckResponse>> CheckTableware(string tagId)
        {
            var tableware = await _context.Tablewares
                .FirstOrDefaultAsync(t => t.TagId == tagId);

            if (tableware == null)
            {
                return Ok(new TablewareCheckResponse
                {
                    IsRegistered = false,
                    Message = "餐具尚未註冊"
                });
            }

            return Ok(new TablewareCheckResponse
            {
                IsRegistered = true,
                TablewareId = tableware.Id,
                TagId = tableware.TagId,
                Type = tableware.Type.ToString(),
                Status = tableware.Status.ToString(),
                Message = "餐具已註冊"
            });
        }

        /// <summary>
        /// 註冊新餐具
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<Tableware>> RegisterTableware(RegisterTablewareRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TagId))
            {
                return BadRequest(new { Message = "TagId 不可為空" });
            }

            // 檢查是否已存在
            var existing = await _context.Tablewares
                .FirstOrDefaultAsync(t => t.TagId == request.TagId);

            if (existing != null)
            {
                return Conflict(new { Message = "此餐具已註冊", TablewareId = existing.Id });
            }

            // 解析餐具類型
            if (!Enum.TryParse<TablewareType>(request.Type, true, out var tablewareType))
            {
                return BadRequest(new { Message = $"無效的餐具類型: {request.Type}。有效值: Bowl, Cup, Chopsticks" });
            }

            var tableware = new Tableware
            {
                TagId = request.TagId,
                Type = tablewareType,
                Status = TablewareStatus.Available
            };

            _context.Tablewares.Add(tableware);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CheckTableware), new { tagId = tableware.TagId }, new
            {
                Message = "餐具註冊成功",
                Tableware = new TablewareDto
                {
                    Id = tableware.Id,
                    TagId = tableware.TagId,
                    Type = tableware.Type.ToString(),
                    Status = tableware.Status.ToString()
                }
            });
        }
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

    public class RegisterTablewareRequest
    {
        public string TagId { get; set; } = string.Empty;
        public string Type { get; set; } = "Bowl"; // 預設為碗
    }
}
