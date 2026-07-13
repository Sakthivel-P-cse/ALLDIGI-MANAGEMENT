using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing asset categories.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService service,
        ICurrentUserService currentUser,
        ILogger<CategoriesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of categories.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CategoryResponseDto>>> GetCategories(
        [FromQuery] PageQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCategoriesAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single category by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDto>> GetCategory(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Category with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new category (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory(
        [FromBody] CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, result);
    }

    /// <summary>Updates an existing category (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryResponseDto>> UpdateCategory(
        uint id,
        [FromBody] UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Category with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a category (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCategory(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Category with ID {id} not found" });
        return NoContent();
    }
}
