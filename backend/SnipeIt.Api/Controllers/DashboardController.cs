using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>
/// Controller for dashboard metrics and analytics
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the high-level dashboard summary
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard summary metrics</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard data" });
        }
    }
}
