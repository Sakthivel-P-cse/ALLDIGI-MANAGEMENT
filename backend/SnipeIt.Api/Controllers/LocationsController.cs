using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing locations.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class LocationsController : ControllerBase
{
    private readonly ILocationService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(
        ILocationService service,
        ICurrentUserService currentUser,
        ILogger<LocationsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of locations.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LocationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<LocationResponseDto>>> GetLocations(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetLocationsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single location by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LocationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationResponseDto>> GetLocation(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Location with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new location (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LocationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LocationResponseDto>> CreateLocation(
        [FromBody] CreateLocationDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetLocation), new { id = result.Id }, result);
    }

    /// <summary>Updates a location (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LocationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LocationResponseDto>> UpdateLocation(
        uint id, [FromBody] UpdateLocationDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Location with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a location (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteLocation(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Location with ID {id} not found" });
        return NoContent();
    }
}
