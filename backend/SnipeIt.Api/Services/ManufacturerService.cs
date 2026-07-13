using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for manufacturer management operations.</summary>
public interface IManufacturerService
{
    Task<PagedResult<ManufacturerResponseDto>> GetManufacturersAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<ManufacturerResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<ManufacturerResponseDto> CreateAsync(CreateManufacturerDto dto, CancellationToken cancellationToken = default);
    Task<ManufacturerResponseDto?> UpdateAsync(uint id, UpdateManufacturerDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class ManufacturerService : IManufacturerService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<ManufacturerService> _logger;

    public ManufacturerService(SnipeitContext context, ILogger<ManufacturerService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<ManufacturerResponseDto>> GetManufacturersAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Manufacturers.Where(m => m.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(m => m.Name.ToLower().Contains(s) ||
                             (m.SupportEmail != null && m.SupportEmail.ToLower().Contains(s)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)x.Name);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        // Batch count assets and licenses per manufacturer (via model)
        var mfrIds = items.Select(m => (int)m.Id).ToList();
        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.ModelId.HasValue)
            .Join(_context.Models.Where(m => m.ManufacturerId.HasValue && mfrIds.Contains(m.ManufacturerId!.Value)),
                  a => a.ModelId, m => (int?)m.Id, (a, m) => m.ManufacturerId)
            .GroupBy(mid => mid)
            .Select(g => new { MfrId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MfrId!.Value, x => x.Count, cancellationToken);

        var licenseCounts = await _context.Licenses
            .Where(l => l.ManufacturerId.HasValue && mfrIds.Contains(l.ManufacturerId!.Value))
            .GroupBy(l => l.ManufacturerId)
            .Select(g => new { MfrId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MfrId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(m => MapToDto(m,
            assetCounts.TryGetValue((int)m.Id, out var ac) ? ac : 0,
            licenseCounts.TryGetValue((int)m.Id, out var lc) ? lc : 0)).ToList();

        return new PagedResult<ManufacturerResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<ManufacturerResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);
        return entity is null ? null : MapToDto(entity, 0, 0);
    }

    public async Task<ManufacturerResponseDto> CreateAsync(CreateManufacturerDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Manufacturer
        {
            Name = dto.Name,
            Url = dto.Url,
            SupportUrl = dto.SupportUrl,
            SupportPhone = dto.SupportPhone,
            SupportEmail = dto.SupportEmail,
            WarrantyLookupUrl = dto.WarrantyLookupUrl,
            Image = dto.Image,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Manufacturers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Manufacturer {Name} created with ID {Id}", entity.Name, entity.Id);
        return MapToDto(entity, 0, 0);
    }

    public async Task<ManufacturerResponseDto?> UpdateAsync(uint id, UpdateManufacturerDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Url is not null) entity.Url = dto.Url;
        if (dto.SupportUrl is not null) entity.SupportUrl = dto.SupportUrl;
        if (dto.SupportPhone is not null) entity.SupportPhone = dto.SupportPhone;
        if (dto.SupportEmail is not null) entity.SupportEmail = dto.SupportEmail;
        if (dto.WarrantyLookupUrl is not null) entity.WarrantyLookupUrl = dto.WarrantyLookupUrl;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Manufacturer {Id} updated", id);
        return MapToDto(entity, 0, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Manufacturer {Id} soft-deleted", id);
        return true;
    }

    private static ManufacturerResponseDto MapToDto(Manufacturer m, int assetsCount, int licensesCount) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Url = m.Url,
        SupportUrl = m.SupportUrl,
        SupportPhone = m.SupportPhone,
        SupportEmail = m.SupportEmail,
        WarrantyLookupUrl = m.WarrantyLookupUrl,
        Image = m.Image,
        Notes = m.Notes,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt,
        AssetsCount = assetsCount,
        LicensesCount = licensesCount
    };
}
