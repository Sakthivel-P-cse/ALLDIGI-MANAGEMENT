using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for status label management operations.</summary>
public interface IStatusLabelService
{
    Task<PagedResult<StatusLabelResponseDto>> GetStatusLabelsAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<StatusLabelResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<StatusLabelResponseDto> CreateAsync(CreateStatusLabelDto dto, CancellationToken cancellationToken = default);
    Task<StatusLabelResponseDto?> UpdateAsync(uint id, UpdateStatusLabelDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class StatusLabelService : IStatusLabelService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<StatusLabelService> _logger;

    public StatusLabelService(SnipeitContext context, ILogger<StatusLabelService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<StatusLabelResponseDto>> GetStatusLabelsAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.StatusLabels.Where(s => s.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(sl => sl.Name != null && sl.Name.ToLower().Contains(s));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)(x.Name ?? ""));

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        var labelIds = items.Select(s => (int)s.Id).ToList();
        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.StatusId.HasValue && labelIds.Contains(a.StatusId.Value))
            .GroupBy(a => a.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StatusId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(sl => MapToDto(sl,
            assetCounts.TryGetValue((int)sl.Id, out var cnt) ? cnt : 0)).ToList();

        return new PagedResult<StatusLabelResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<StatusLabelResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.StatusLabels
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
        return entity is null ? null : MapToDto(entity, 0);
    }

    public async Task<StatusLabelResponseDto> CreateAsync(CreateStatusLabelDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new StatusLabel
        {
            Name = dto.Name,
            Color = dto.Color,
            ShowInNav = dto.ShowInNav,
            DefaultLabel = dto.DefaultLabel,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ApplyStatusType(entity, dto.StatusType);

        _context.StatusLabels.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StatusLabel {Name} created with ID {Id}", entity.Name, entity.Id);
        return MapToDto(entity, 0);
    }

    public async Task<StatusLabelResponseDto?> UpdateAsync(uint id, UpdateStatusLabelDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.StatusLabels
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.StatusType is not null) ApplyStatusType(entity, dto.StatusType);
        if (dto.Color is not null) entity.Color = dto.Color;
        if (dto.ShowInNav.HasValue) entity.ShowInNav = dto.ShowInNav;
        if (dto.DefaultLabel.HasValue) entity.DefaultLabel = dto.DefaultLabel;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StatusLabel {Id} updated", id);
        return MapToDto(entity, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.StatusLabels
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("StatusLabel {Id} soft-deleted", id);
        return true;
    }

    /// <summary>Maps a string status type to the three boolean flags on the entity.</summary>
    private static void ApplyStatusType(StatusLabel entity, string statusType)
    {
        entity.Deployable = false;
        entity.Pending = false;
        entity.Archived = false;

        switch (statusType.ToLowerInvariant())
        {
            case "deployable": entity.Deployable = true; break;
            case "pending":    entity.Pending = true;    break;
            case "archived":   entity.Archived = true;   break;
            // "undeployable" — all three remain false
        }
    }

    private static StatusLabelResponseDto MapToDto(StatusLabel s, int assetsCount)
    {
        string statusType = s.Deployable ? "deployable"
                         : s.Pending    ? "pending"
                         : s.Archived   ? "archived"
                         : "undeployable";

        return new StatusLabelResponseDto
        {
            Id = s.Id, Name = s.Name, StatusType = statusType,
            Deployable = s.Deployable, Pending = s.Pending, Archived = s.Archived,
            Color = s.Color, ShowInNav = s.ShowInNav, DefaultLabel = s.DefaultLabel,
            Notes = s.Notes, CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt,
            AssetsCount = assetsCount
        };
    }
}
