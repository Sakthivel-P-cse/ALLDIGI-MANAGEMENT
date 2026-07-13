using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <inheritdoc />
public sealed class DashboardService : IDashboardService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(SnipeitContext context, ILogger<DashboardService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeAssets = _context.Assets.Where(a => a.DeletedAt == null);
            
            var totalAssets = await activeAssets.CountAsync(cancellationToken);
            var totalUsers = await _context.Users.CountAsync(u => u.DeletedAt == null, cancellationToken);
            var totalLicenses = await _context.Licenses.CountAsync(l => l.DeletedAt == null, cancellationToken);
            var totalAccessories = await _context.Accessories.CountAsync(a => a.DeletedAt == null, cancellationToken);

            // Asset Status Counts
            var assetsDeployed = await activeAssets.CountAsync(a => a.AssignedTo != null, cancellationToken);
            
            // To figure out Ready to Deploy, Pending, Archived, we need to join with StatusLabels
            var assetsByStatus = await activeAssets
                .Where(a => a.StatusId != null && a.AssignedTo == null)
                .Join(_context.StatusLabels,
                    asset => (uint)asset.StatusId!,
                    status => status.Id,
                    (asset, status) => status)
                .ToListAsync(cancellationToken);

            var assetsReadyToDeploy = assetsByStatus.Count(s => s.Deployable);
            var assetsPending = assetsByStatus.Count(s => s.Pending);
            var assetsArchived = assetsByStatus.Count(s => s.Archived);

            // Recent Activity (last 10 logs)
            var recentLogs = await _context.ActionLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            var userIds = recentLogs.Select(l => l.CreatedBy)
                .Concat(recentLogs.Where(l => l.TargetType == "App\\Models\\User").Select(l => l.TargetId))
                .Where(u => u.HasValue)
                .Select(u => (uint)u!.Value)
                .Distinct()
                .ToList();

            var users = userIds.Any()
                ? await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => (u.DisplayName ?? u.FirstName ?? u.Email)!, cancellationToken)
                : new Dictionary<uint, string>();

            var recentActivity = recentLogs.Select(log =>
            {
                var dto = new ActionLogResponseDto
                {
                    Id = log.Id,
                    ActionType = log.ActionType ?? string.Empty,
                    ItemType = log.ItemType ?? string.Empty,
                    ItemId = log.ItemId,
                    TargetId = log.TargetId,
                    TargetType = log.TargetType,
                    CreatedBy = log.CreatedBy,
                    Note = log.Note,
                    LogMeta = log.LogMeta,
                    ActionDate = log.ActionDate ?? log.CreatedAt ?? DateTime.UtcNow,
                    CreatedAt = log.CreatedAt ?? DateTime.UtcNow
                };

                if (log.CreatedBy.HasValue && users.TryGetValue((uint)log.CreatedBy.Value, out var adminName))
                {
                    dto.AdminName = adminName;
                }

                if (log.TargetId.HasValue && log.TargetType == "App\\Models\\User" && users.TryGetValue((uint)log.TargetId.Value, out var targetName))
                {
                    dto.TargetName = targetName;
                }

                return dto;
            }).ToList();

            return new DashboardSummaryDto
            {
                TotalAssets = totalAssets,
                TotalUsers = totalUsers,
                TotalLicenses = totalLicenses,
                TotalAccessories = totalAccessories,
                AssetsDeployed = assetsDeployed,
                AssetsReadyToDeploy = assetsReadyToDeploy,
                AssetsPending = assetsPending,
                AssetsArchived = assetsArchived,
                RecentActivity = recentActivity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard summary");
            throw;
        }
    }
}
