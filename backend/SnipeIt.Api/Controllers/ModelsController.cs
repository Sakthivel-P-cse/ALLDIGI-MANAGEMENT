using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing asset models.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class ModelsController : ControllerBase
{
    private readonly IModelService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ModelsController> _logger;

    public ModelsController(
        IModelService service,
        ICurrentUserService currentUser,
        ILogger<ModelsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of models.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ModelResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ModelResponseDto>>> GetModels(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetModelsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single model by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ModelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelResponseDto>> GetModel(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Model with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new model (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ModelResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ModelResponseDto>> CreateModel(
        [FromBody] CreateModelDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetModel), new { id = result.Id }, result);
    }

    /// <summary>Updates a model (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ModelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ModelResponseDto>> UpdateModel(
        uint id, [FromBody] UpdateModelDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Model with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a model (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteModel(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Model with ID {id} not found" });
        return NoContent();
    }
}
