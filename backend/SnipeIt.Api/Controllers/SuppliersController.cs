using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing suppliers.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(
        ISupplierService service,
        ICurrentUserService currentUser,
        ILogger<SuppliersController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of suppliers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SupplierResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SupplierResponseDto>>> GetSuppliers(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetSuppliersAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single supplier by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierResponseDto>> GetSupplier(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Supplier with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new supplier (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SupplierResponseDto>> CreateSupplier(
        [FromBody] CreateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetSupplier), new { id = result.Id }, result);
    }

    /// <summary>Updates a supplier (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SupplierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SupplierResponseDto>> UpdateSupplier(
        uint id, [FromBody] UpdateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Supplier with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a supplier (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteSupplier(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Supplier with ID {id} not found" });
        return NoContent();
    }
}
