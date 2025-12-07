using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoloopSystem.Server.Data;
using EcoloopSystem.Server.Models;

namespace EcoloopSystem.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly EcoloopContext _context;

        public RentalsController(EcoloopContext context)
        {
            _context = context;
        }

        // POST: api/rentals/borrow
        [HttpPost("borrow")]
        public async Task<ActionResult<Rental>> Borrow(BorrowRequest request)
        {
            // 1. Find User
            var user = await _context.Users.FirstOrDefaultAsync(u => u.CardId == request.CardId);
            if (user == null)
            {
                // Auto-register a temporary user or return error? 
                // Requirement says "User goes to swipe card... DB records this card has borrowed..."
                // If user not registered, we might just create a user record with just CardId
                user = new User { CardId = request.CardId };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // 2. Find Tableware
            var tableware = await _context.Tablewares.FirstOrDefaultAsync(t => t.TagId == request.TablewareTagId);
            if (tableware == null)
            {
                // If tableware not found, maybe auto-add it? Or error.
                // Let's auto-add for simplicity of the prototype, or return error.
                // Plan says "Storage area... DB records...".
                // Let's assume tableware must exist.
                return NotFound("找不到該餐具 (Tableware not found).");
            }

            if (tableware.Status == TablewareStatus.Rented)
            {
                return BadRequest("該餐具已被租借 (Tableware is already rented).");
            }

            // 3. Create Rental
            var rental = new Rental
            {
                UserId = user.Id,
                TablewareId = tableware.Id,
                BorrowedAt = DateTime.Now,
                StationId = 1 // Default station
            };

            tableware.Status = TablewareStatus.Rented;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Borrow), new { id = rental.Id }, rental);
        }

        // POST: api/rentals/return
        [HttpPost("return")]
        public async Task<IActionResult> Return(ReturnRequest request)
        {
            var tableware = await _context.Tablewares.FirstOrDefaultAsync(t => t.TagId == request.TablewareTagId);
            if (tableware == null)
            {
                return NotFound("找不到該餐具 (Tableware not found).");
            }

            if (tableware.Status != TablewareStatus.Rented)
            {
                return BadRequest("該餐具目前未被租借 (Tableware is not currently rented).");
            }

            // Find the active rental
            var rental = await _context.Rentals
                .Where(r => r.TablewareId == tableware.Id && r.ReturnedAt == null)
                .OrderByDescending(r => r.BorrowedAt)
                .FirstOrDefaultAsync();

            if (rental == null)
            {
                // Inconsistent state, fix it
                tableware.Status = TablewareStatus.Available;
                await _context.SaveChangesAsync();
                return Ok("餐具狀態已重置為可用 (未找到活躍租借紀錄)。");
            }

            rental.ReturnedAt = DateTime.Now;
            tableware.Status = TablewareStatus.Available;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Return successful", Duration = (rental.ReturnedAt.Value - rental.BorrowedAt).TotalMinutes });
        }
    }

    public class BorrowRequest
    {
        public string CardId { get; set; } = string.Empty;
        public string TablewareTagId { get; set; } = string.Empty;
    }

    public class ReturnRequest
    {
        public string TablewareTagId { get; set; } = string.Empty;
    }
}
