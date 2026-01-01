using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoloopSystem.Server.Data;

namespace EcoloopSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly EcoloopContext _context;
        
        // 寫死的 Admin 帳號密碼
        private const string ADMIN_USERNAME = "admin";
        private const string ADMIN_PASSWORD = "1111";

        public AdminController(EcoloopContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Admin 登入（寫死驗證）
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            if (request.Username == ADMIN_USERNAME && request.Password == ADMIN_PASSWORD)
            {
                return Ok(new { Success = true, Message = "登入成功" });
            }
            return Unauthorized(new { Success = false, Message = "帳號或密碼錯誤" });
        }

        /// <summary>
        /// 取得所有租借記錄（支援篩選）
        /// </summary>
        [HttpGet("rentals")]
        public async Task<IActionResult> GetRentals(
            [FromQuery] bool? onlyUnreturned = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Rentals
                .Include(r => r.User)
                .Include(r => r.Tableware)
                .AsQueryable();

            // 篩選未歸還
            if (onlyUnreturned == true)
            {
                query = query.Where(r => r.ReturnedAt == null);
            }

            // 日期篩選
            if (startDate.HasValue)
            {
                query = query.Where(r => r.BorrowedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(r => r.BorrowedAt < endOfDay);
            }

            var rentals = await query
                .OrderByDescending(r => r.BorrowedAt)
                .Select(r => new RentalDto
                {
                    Id = r.Id,
                    UserPhone = r.User != null ? (r.User.PhoneNumber ?? "未填寫") : "未知",
                    UserCardId = r.User != null ? r.User.CardId : "未知",
                    TablewareTagId = r.Tableware != null ? r.Tableware.TagId : "未知",
                    TablewareType = r.Tableware != null ? r.Tableware.Type.ToString() : "未知",
                    BorrowedAt = r.BorrowedAt,
                    ReturnedAt = r.ReturnedAt,
                    IsReturned = r.ReturnedAt != null
                })
                .ToListAsync();

            return Ok(rentals);
        }

        /// <summary>
        /// 取得統計資訊
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalRentals = await _context.Rentals.CountAsync();
            var unreturned = await _context.Rentals.CountAsync(r => r.ReturnedAt == null);
            var totalUsers = await _context.Users.CountAsync();
            var totalTableware = await _context.Tablewares.CountAsync();

            return Ok(new
            {
                TotalRentals = totalRentals,
                UnreturnedCount = unreturned,
                TotalUsers = totalUsers,
                TotalTableware = totalTableware
            });
        }
    }

    public class AdminLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RentalDto
    {
        public int Id { get; set; }
        public string UserPhone { get; set; } = string.Empty;
        public string UserCardId { get; set; } = string.Empty;
        public string TablewareTagId { get; set; } = string.Empty;
        public string TablewareType { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public bool IsReturned { get; set; }
    }
}
