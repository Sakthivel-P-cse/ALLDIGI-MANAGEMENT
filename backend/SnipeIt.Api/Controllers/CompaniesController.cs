using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing companies.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        ICompanyService service,
        ICurrentUserService currentUser,
        ILogger<CompaniesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Returns a paginated, searchable, sortable list of companies.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CompanyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CompanyResponseDto>>> GetCompanies(
        [FromQuery] PageQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetCompaniesAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a single company by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponseDto>> GetCompany(uint id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Company with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Creates a new company (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CompanyResponseDto>> CreateCompany(
        [FromBody] CreateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetCompany), new { id = result.Id }, result);
    }

    /// <summary>Updates a company (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CompanyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CompanyResponseDto>> UpdateCompany(
        uint id, [FromBody] UpdateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (result is null) return NotFound(new { message = $"Company with ID {id} not found" });
        return Ok(result);
    }

    /// <summary>Soft-deletes a company (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCompany(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"Company with ID {id} not found" });
        return NoContent();
    }
}
