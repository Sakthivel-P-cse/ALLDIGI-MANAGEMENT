using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing departments.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IDepartmentService service,
        ICurrentUserService currentUser,
        ILogger<DepartmentsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of departments.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DepartmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DepartmentResponseDto>>> GetDepartments(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetDepartmentsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single department by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResponseDto>> GetDepartment(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Department with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new department (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment(
        [FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, _currentUser.UserId.GetValueOrDefault(0), cancellationToken);
        return CreatedAtAction(nameof(GetDepartment), new { id = result.Id }, result);
    }

    /// <summary>Updates a department (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DepartmentResponseDto>> UpdateDepartment(
        uint id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Department with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a department (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteDepartment(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Department with ID {id} not found" });
        return NoContent();
    }
}
