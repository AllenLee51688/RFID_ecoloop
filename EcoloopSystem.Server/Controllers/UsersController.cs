using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoloopSystem.Server.Data;
using EcoloopSystem.Server.Models;

namespace EcoloopSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EcoloopContext _context;

        public UsersController(EcoloopContext context)
        {
            _context = context;
        }

        // GET: api/users/check/{cardId}
        [HttpGet("check/{cardId}")]
        public async Task<IActionResult> CheckCard(string cardId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.CardId == cardId);
            if (user == null)
            {
                return Ok(new { IsRegistered = false, Message = "卡片尚未註冊" });
            }
            return Ok(new { 
                IsRegistered = true, 
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                RegisteredAt = user.RegisteredAt,
                Message = "已註冊使用者" 
            });
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // 檢查手機號碼格式
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Length < 10)
            {
                return BadRequest(new { Success = false, Message = "手機號碼格式不正確" });
            }

            // 檢查密碼
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 4)
            {
                return BadRequest(new { Success = false, Message = "密碼至少需要4個字元" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CardId == request.CardId);
            if (user == null)
            {
                user = new User
                {
                    CardId = request.CardId,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password,
                    RegisteredAt = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { Success = true, Message = "註冊成功", User = new { user.Id, user.CardId, user.PhoneNumber, user.RegisteredAt } });
            }
            else
            {
                return BadRequest(new { Success = false, Message = "此卡片已註冊" });
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { Success = false, Message = "請輸入手機號碼" });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Success = false, Message = "請輸入密碼" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "此手機號碼尚未註冊" });
            }

            if (user.Password != request.Password)
            {
                return BadRequest(new { Success = false, Message = "密碼錯誤" });
            }

            return Ok(new { 
                Success = true, 
                Message = "登入成功",
                User = new { 
                    user.Id, 
                    user.PhoneNumber, 
                    user.RegisteredAt 
                }
            });
        }

        // GET: api/users/{phone}/history
        [HttpGet("{phone}/history")]
        public async Task<ActionResult<IEnumerable<RentalDTO>>> GetHistory(string phone)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user == null)
            {
                return NotFound("找不到該使用者 (User not found).");
            }

            var history = await _context.Rentals
                .Include(r => r.Tableware)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.BorrowedAt)
                .Select(r => new RentalDTO
                {
                    Id = r.Id,
                    TablewareType = r.Tableware != null ? r.Tableware.Type.ToString() : "Unknown",
                    BorrowedAt = r.BorrowedAt,
                    ReturnedAt = r.ReturnedAt,
                    Status = r.ReturnedAt.HasValue ? "Returned" : "Rented"
                })
                .ToListAsync();

            return Ok(history);
        }
    }

    public class RegisterRequest
    {
        public string CardId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RentalDTO
    {
        public int Id { get; set; }
        public string TablewareType { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
