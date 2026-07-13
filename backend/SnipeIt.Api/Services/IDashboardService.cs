using SnipeIt.Api.Dtos;

namespace SnipeIt.Api.Services;

/// <summary>
/// Service interface for dashboard metrics and summaries
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets the dashboard summary statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard summary DTO</returns>
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
