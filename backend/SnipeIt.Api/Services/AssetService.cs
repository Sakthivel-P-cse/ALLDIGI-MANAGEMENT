using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>
/// Service for managing asset operations
/// </summary>
public sealed class AssetService : IAssetService
{
    private readonly SnipeitContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AssetService> _logger;
    private readonly IAuditLogService _auditLogService;

    /// <summary>
    /// Initializes a new instance of the AssetService
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="auditLogService">Audit log service</param>
    public AssetService(
        SnipeitContext context,
        IMapper mapper,
        ILogger<AssetService> logger,
        IAuditLogService auditLogService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<AssetResponseDto> Data, int Total)> GetAssetsAsync(
        int skipCount = 0,
        int takeCount = 50,
        int? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Assets.Where(a => a.DeletedAt == null);
            
            if (assignedToUserId.HasValue)
            {
                query = query.Where(a => a.AssignedTo == assignedToUserId.Value);
            }

            var total = await query.CountAsync(cancellationToken);

            var assets = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skipCount)
                .Take(takeCount)
                .ToListAsync(cancellationToken);

            // Batch-load related data to avoid N+1 and to provide display names
            var modelIds = assets.Where(a => a.ModelId.HasValue).Select(a => (uint)a.ModelId!.Value).Distinct().ToList();
            var locationIds = assets.Where(a => a.LocationId.HasValue).Select(a => (uint)a.LocationId!.Value).Distinct().ToList();
            var supplierIds = assets.Where(a => a.SupplierId.HasValue).Select(a => (uint)a.SupplierId!.Value).Distinct().ToList();
            var assignedToIds = assets.Where(a => a.AssignedTo.HasValue).Select(a => (uint)a.AssignedTo!.Value).Distinct().ToList();

            var models = modelIds.Any()
                ? await _context.Models.Where(m => modelIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken)
                : new Dictionary<uint, string>();

            var locations = locationIds.Any()
                ? await _context.Locations.Where(l => locationIds.Contains(l.Id)).ToDictionaryAsync(l => l.Id, l => l.Name, cancellationToken)
                : new Dictionary<uint, string>();

            var suppliers = supplierIds.Any()
                ? await _context.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken)
                : new Dictionary<uint, string>();

            var users = assignedToIds.Any()
                ? await _context.Users.Where(u => assignedToIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => (u.DisplayName ?? u.FirstName ?? u.Email)!, cancellationToken)
                : new Dictionary<uint, string>();

            var assetDtos = assets.Select(a =>
            {
                var dto = _mapper.Map<AssetResponseDto>(a);

                if (a.ModelId.HasValue && models.TryGetValue((uint)a.ModelId.Value, out var mName))
                {
                    dto.ModelName = mName;
                }

                if (a.LocationId.HasValue && locations.TryGetValue((uint)a.LocationId.Value, out var lName))
                {
                    dto.LocationName = lName;
                }

                if (a.SupplierId.HasValue && suppliers.TryGetValue((uint)a.SupplierId.Value, out var sName))
                {
                    dto.SupplierName = sName;
                }

                if (a.AssignedTo.HasValue && users.TryGetValue((uint)a.AssignedTo.Value, out var uName))
                {
                    dto.AssignedToName = uName;
                }

                return dto;
            }).ToList();

            return (assetDtos, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<AssetResponseDto?> GetAssetByIdAsync(
        uint id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning("Asset with ID {AssetId} not found", id);
                return null;
            }

            var dto = _mapper.Map<AssetResponseDto>(asset);

            if (asset.ModelId.HasValue)
            {
                dto.ModelName = await _context.Models
                    .Where(m => m.Id == (uint)asset.ModelId.Value)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (asset.LocationId.HasValue)
            {
                dto.LocationName = await _context.Locations
                    .Where(l => l.Id == (uint)asset.LocationId.Value)
                    .Select(l => l.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (asset.SupplierId.HasValue)
            {
                dto.SupplierName = await _context.Suppliers
                    .Where(s => s.Id == (uint)asset.SupplierId.Value)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (asset.AssignedTo.HasValue)
            {
                dto.AssignedToName = await _context.Users
                    .Where(u => u.Id == (uint)asset.AssignedTo.Value)
                    .Select(u => u.DisplayName ?? u.FirstName ?? u.Email)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with ID {AssetId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<AssetResponseDto?> GetAssetByTagAsync(
        string assetTag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.AssetTag == assetTag && a.DeletedAt == null, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning("Asset with tag {AssetTag} not found", assetTag);
                return null;
            }

            // reuse single-asset mapping logic
            return await GetAssetByIdAsync(asset.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with tag {AssetTag}", assetTag);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<AssetResponseDto> CreateAssetAsync(
        CreateAssetDto createAssetDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if asset tag is unique
            var existingAsset = await _context.Assets
                .FirstOrDefaultAsync(a => a.AssetTag == createAssetDto.AssetTag, cancellationToken);

            if (existingAsset is not null)
            {
                throw new InvalidOperationException($"Asset tag '{createAssetDto.AssetTag}' already exists");
            }

            var asset = _mapper.Map<Asset>(createAssetDto);
            asset.CreatedAt = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset {AssetTag} created with ID {AssetId}", asset.AssetTag, asset.Id);
            return await GetAssetByIdAsync(asset.Id, cancellationToken) ?? _mapper.Map<AssetResponseDto>(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<AssetResponseDto?> UpdateAssetAsync(
        uint id,
        UpdateAssetDto updateAssetDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning("Asset with ID {AssetId} not found", id);
                return null;
            }

            // Check if asset tag already exists on another asset
            if (!string.IsNullOrEmpty(updateAssetDto.AssetTag) && updateAssetDto.AssetTag != asset.AssetTag)
            {
                var existingAsset = await _context.Assets
                    .FirstOrDefaultAsync(
                        a => a.AssetTag == updateAssetDto.AssetTag && a.Id != id,
                        cancellationToken);

                if (existingAsset is not null)
                {
                    throw new InvalidOperationException($"Asset tag '{updateAssetDto.AssetTag}' already exists");
                }
            }

            _mapper.Map(updateAssetDto, asset);
            asset.UpdatedAt = DateTime.UtcNow;

            _context.Assets.Update(asset);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset with ID {AssetId} updated", id);
            return await GetAssetByIdAsync(id, cancellationToken) ?? _mapper.Map<AssetResponseDto>(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset with ID {AssetId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAssetAsync(uint id, CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning("Asset with ID {AssetId} not found", id);
                return false;
            }

            asset.DeletedAt = DateTime.UtcNow;
            _context.Assets.Update(asset);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset with ID {AssetId} deleted", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset with ID {AssetId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<AssetResponseDto?> CheckoutAssetAsync(
        uint id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning("Asset with ID {AssetId} not found", id);
                return null;
            }

            asset.AssignedTo = userId;
            asset.AssignedType = "App\\Models\\User";
            asset.LastCheckout = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            _context.Assets.Update(asset);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                actionType: "checkout",
                itemType: "Asset",
                itemId: (int)id,
                targetId: userId,
                targetType: "App\\Models\\User",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Asset with ID {AssetId} checked out to user {UserId}", id, userId);
            return await GetAssetByIdAsync(id, cancellationToken) ?? _mapper.Map<AssetResponseDto>(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out asset with ID {AssetId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<AssetResponseDto?> CheckinAssetAsync(
        uint id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning("Asset with ID {AssetId} not found", id);
                return null;
            }

            asset.AssignedTo = null;
            asset.AssignedType = null;
            asset.LastCheckin = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.CheckinCounter += 1;

            _context.Assets.Update(asset);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                actionType: "checkin",
                itemType: "Asset",
                itemId: (int)id,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Asset with ID {AssetId} checked in", id);
            return await GetAssetByIdAsync(id, cancellationToken) ?? _mapper.Map<AssetResponseDto>(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in asset with ID {AssetId}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<ActionLogResponseDto> Data, int Total)> GetAssetHistoryAsync(
        uint id,
        int skipCount = 0,
        int takeCount = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ActionLogs
                .Where(log => log.ItemType == "Asset" && log.ItemId == id);

            var total = await query.CountAsync(cancellationToken);

            var logs = await query
                .OrderByDescending(log => log.CreatedAt)
                .Skip(skipCount)
                .Take(takeCount)
                .ToListAsync(cancellationToken);

            var userIds = logs.Select(l => l.CreatedBy)
                .Concat(logs.Where(l => l.TargetType == "App\\Models\\User").Select(l => l.TargetId))
                .Where(u => u.HasValue)
                .Select(u => (uint)u!.Value)
                .Distinct()
                .ToList();

            var users = userIds.Any()
                ? await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => (u.DisplayName ?? u.FirstName ?? u.Email)!, cancellationToken)
                : new Dictionary<uint, string>();

            var dtos = logs.Select(log =>
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

            return (dtos, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for asset {AssetId}", id);
            throw;
        }
    }
}
