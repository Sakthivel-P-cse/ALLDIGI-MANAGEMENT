using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Models.Entities;
using SnipeIt.Api.Common;

namespace SnipeIt.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class AccessoriesController : ControllerBase
{
    private readonly SnipeitContext _context;
    private readonly IAuditLogService _auditLogService;

    public AccessoriesController(SnipeitContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccessories([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var query = _context.Accessories.AsNoTracking();

        if (!User.IsInRole("admin"))
        {
            var idClaim = User.FindFirst("id")?.Value;
            if (int.TryParse(idClaim, out int userId))
            {
                var assignedAccessoryIds = _context.AccessoriesCheckouts
                    .Where(ac => ac.AssignedTo == userId)
                    .Select(ac => (uint)ac.AccessoryId!)
                    .Distinct();
                
                query = query.Where(a => assignedAccessoryIds.Contains(a.Id));
            }
        }

        var total = await query.CountAsync();
        var accessories = await query.OrderBy(a => a.Id).Skip(skip).Take(take).ToListAsync();

        object dataToReturn = accessories;

        if (User.IsInRole("admin"))
        {
            var accessoryIds = accessories.Select(a => a.Id).ToList();
            var checkouts = await _context.AccessoriesCheckouts
                .Where(ac => ac.AccessoryId.HasValue && accessoryIds.Contains((uint)ac.AccessoryId.Value))
                .ToListAsync();

            var userIds = checkouts.Where(c => c.AssignedTo.HasValue).Select(c => (uint)c.AssignedTo!.Value).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            dataToReturn = accessories.Select(a =>
            {
                var assignedCheckouts = checkouts.Where(c => c.AccessoryId == a.Id && c.AssignedTo.HasValue).ToList();
                var assignedNames = assignedCheckouts
                    .Select(c => users.TryGetValue((uint)c.AssignedTo!.Value, out var name) ? name : $"User {c.AssignedTo}")
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
                
                return new
                {
                    a.Id,
                    a.Name,
                    a.Qty,
                    a.LocationId,
                    a.CreatedAt,
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
    public async Task<IActionResult> CreateAccessory([FromBody] CreateAccessoryDto dto)
    {
        if (!User.IsInRole("admin")) return Forbid();

        var accessory = new Accessory
        {
            Name = dto.Name,
            Qty = dto.Qty,
            LocationId = dto.LocationId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accessories.Add(accessory);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAccessories), new { id = accessory.Id }, accessory);
    }

    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> CheckoutAccessory(uint id, [FromBody] CheckoutAccessoryDto dto)
    {
        if (!User.IsInRole("admin")) return Forbid();

        var accessory = await _context.Accessories.FindAsync(id);
        if (accessory == null) return NotFound(new { message = "Accessory not found" });

        var checkedOutCount = await _context.AccessoriesCheckouts.CountAsync(c => c.AccessoryId == id);
        if (checkedOutCount >= accessory.Qty)
        {
            return BadRequest(new { message = "No available quantity for this accessory" });
        }

        var checkout = new AccessoriesCheckout
        {
            AccessoryId = (int)id,
            AssignedTo = dto.UserId,
            AssignedType = "App\\Models\\User",
            CreatedAt = DateTime.UtcNow
        };

        _context.AccessoriesCheckouts.Add(checkout);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            actionType: "checkout",
            itemType: "Accessory",
            itemId: (int)id,
            targetId: dto.UserId,
            targetType: "App\\Models\\User");

        return Ok(new { message = "Accessory checked out successfully" });
    }

    [HttpPost("checkouts/{checkoutId}/checkin")]
    public async Task<IActionResult> CheckinAccessory(uint checkoutId)
    {
        if (!User.IsInRole("admin")) return Forbid();

        var checkout = await _context.AccessoriesCheckouts.FindAsync(checkoutId);
        if (checkout == null) return NotFound(new { message = "Accessory checkout record not found" });

        var accessoryId = checkout.AccessoryId;

        _context.AccessoriesCheckouts.Remove(checkout);
        await _context.SaveChangesAsync();

        if (accessoryId.HasValue)
        {
            await _auditLogService.LogAsync(
                actionType: "checkin",
                itemType: "Accessory",
                itemId: accessoryId.Value);
        }

        return Ok(new { message = "Accessory checked in successfully" });
    }
}

public class CheckoutAccessoryDto
{
    public required int UserId { get; set; }
}

public class CreateAccessoryDto
{
    public required string Name { get; set; }
    public int Qty { get; set; }
    public int? LocationId { get; set; }
}
