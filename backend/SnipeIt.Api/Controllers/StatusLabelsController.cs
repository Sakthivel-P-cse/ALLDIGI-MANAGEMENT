using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing status labels.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class StatusLabelsController : ControllerBase
{
    private readonly IStatusLabelService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<StatusLabelsController> _logger;

    public StatusLabelsController(
        IStatusLabelService service,
        ICurrentUserService currentUser,
        ILogger<StatusLabelsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of status labels.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StatusLabelResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StatusLabelResponseDto>>> GetStatusLabels(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetStatusLabelsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single status label by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StatusLabelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusLabelResponseDto>> GetStatusLabel(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Status label with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new status label (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(StatusLabelResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StatusLabelResponseDto>> CreateStatusLabel(
        [FromBody] CreateStatusLabelDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetStatusLabel), new { id = result.Id }, result);
    }

    /// <summary>Updates a status label (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StatusLabelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<StatusLabelResponseDto>> UpdateStatusLabel(
        uint id, [FromBody] UpdateStatusLabelDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Status label with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a status label (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteStatusLabel(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Status label with ID {id} not found" });
        return NoContent();
    }
}
