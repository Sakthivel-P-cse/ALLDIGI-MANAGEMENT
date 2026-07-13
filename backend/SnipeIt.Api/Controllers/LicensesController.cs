using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Models.Entities;
using SnipeIt.Api.Common;

namespace SnipeIt.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class LicensesController : ControllerBase
{
    private readonly SnipeitContext _context;
    private readonly IAuditLogService _auditLogService;

    public LicensesController(SnipeitContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLicenses([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var query = _context.Licenses.AsNoTracking();

        if (!User.IsInRole("admin"))
        {
            var idClaim = User.FindFirst("id")?.Value;
            if (int.TryParse(idClaim, out int userId))
            {
                var assignedLicenseIds = _context.LicenseSeats
                    .Where(ls => ls.AssignedTo == userId)
                    .Select(ls => (uint)ls.LicenseId!)
                    .Distinct();
                
                query = query.Where(l => assignedLicenseIds.Contains(l.Id));
            }
        }

        var total = await query.CountAsync();
        var licenses = await query.OrderBy(l => l.Id).Skip(skip).Take(take).ToListAsync();

        object dataToReturn = licenses;

        if (User.IsInRole("admin"))
        {
            var licenseIds = licenses.Select(l => l.Id).ToList();
            var seats = await _context.LicenseSeats
                .Where(ls => ls.LicenseId.HasValue && licenseIds.Contains((uint)ls.LicenseId.Value))
                .ToListAsync();

            var userIds = seats.Where(s => s.AssignedTo.HasValue).Select(s => (uint)s.AssignedTo!.Value).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            dataToReturn = licenses.Select(l =>
            {
                var assignedSeats = seats.Where(s => s.LicenseId == l.Id && s.AssignedTo.HasValue).ToList();
                var assignedNames = assignedSeats
                    .Select(s => users.TryGetValue((uint)s.AssignedTo!.Value, out var name) ? name : $"User {s.AssignedTo}")
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
                
                return new
                {
                    l.Id,
                    l.Name,
                    l.Serial,
                    l.Seats,
                    l.PurchaseCost,
                    l.CreatedAt,
                    AssignedUsers = assignedNames.Any() ? string.Join(", ", assignedNames) : null
                };
            });
        }

        return Ok(new
        {
            Data = dataToReturn,
            Skip = skip,
            Take = take,
            Total = total
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseDto dto)
    {
        if (!User.IsInRole("admin")) return Forbid();

        var license = new License
        {
            Name = dto.Name,
            Serial = dto.Serial,
            Seats = dto.Seats,
            PurchaseCost = dto.PurchaseCost,
            CreatedAt = DateTime.UtcNow
        };
        _context.Licenses.Add(license);
        await _context.SaveChangesAsync();

        // Create seats
        for (int i = 0; i < dto.Seats; i++)
        {
            _context.LicenseSeats.Add(new LicenseSeat
            {
                LicenseId = (int)license.Id,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLicenses), new { id = license.Id }, license);
    }

    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> CheckoutLicense(uint id, [FromBody] CheckoutLicenseDto dto)
    {
        if (!User.IsInRole("admin")) return Forbid();

        var license = await _context.Licenses.FindAsync(id);
        if (license == null) return NotFound(new { message = "License not found" });

        var seat = await _context.LicenseSeats.FirstOrDefaultAsync(ls => ls.LicenseId == id && ls.AssignedTo == null);
        if (seat == null) return BadRequest(new { message = "No available seats for this license" });

        seat.AssignedTo = dto.UserId;
        seat.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            actionType: "checkout",
            itemType: "License",
            itemId: (int)id,
            targetId: dto.UserId,
            targetType: "App\\Models\\User");

        return Ok(new { message = "License checked out successfully" });
    }

    [HttpPost("seats/{seatId}/checkin")]
    public async Task<IActionResult> CheckinLicenseSeat(uint seatId)
    {
        if (!User.IsInRole("admin")) return Forbid();

        var seat = await _context.LicenseSeats.FindAsync(seatId);
        if (seat == null || seat.AssignedTo == null) return NotFound(new { message = "Active license seat not found" });

        seat.AssignedTo = null;
        seat.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (seat.LicenseId.HasValue)
        {
            await _auditLogService.LogAsync(
                actionType: "checkin",
                itemType: "License",
                itemId: seat.LicenseId.Value);
        }

        return Ok(new { message = "License checked in successfully" });
    }
}

public class CheckoutLicenseDto
{
    public required int UserId { get; set; }
}

public class CreateLicenseDto
{
    public required string Name { get; set; }
    public string? Serial { get; set; }
    public int Seats { get; set; }
    public decimal? PurchaseCost { get; set; }
}
