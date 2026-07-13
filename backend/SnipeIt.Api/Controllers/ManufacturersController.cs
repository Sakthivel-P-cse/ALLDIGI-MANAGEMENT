using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing manufacturers.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ManufacturersController> _logger;

    public ManufacturersController(
        IManufacturerService service,
        ICurrentUserService currentUser,
        ILogger<ManufacturersController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of manufacturers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ManufacturerResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ManufacturerResponseDto>>> GetManufacturers(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetManufacturersAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single manufacturer by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ManufacturerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManufacturerResponseDto>> GetManufacturer(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Manufacturer with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new manufacturer (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ManufacturerResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ManufacturerResponseDto>> CreateManufacturer(
        [FromBody] CreateManufacturerDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetManufacturer), new { id = result.Id }, result);
    }

    /// <summary>Updates a manufacturer (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ManufacturerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ManufacturerResponseDto>> UpdateManufacturer(
        uint id, [FromBody] UpdateManufacturerDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Manufacturer with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a manufacturer (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteManufacturer(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Manufacturer with ID {id} not found" });
        return NoContent();
    }
}
