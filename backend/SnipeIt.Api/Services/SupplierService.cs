using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for supplier management operations.</summary>
public interface ISupplierService
{
    Task<PagedResult<SupplierResponseDto>> GetSuppliersAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<SupplierResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<SupplierResponseDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default);
    Task<SupplierResponseDto?> UpdateAsync(uint id, UpdateSupplierDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class SupplierService : ISupplierService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(SnipeitContext context, ILogger<SupplierService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<SupplierResponseDto>> GetSuppliersAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Suppliers.Where(s => s.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(sup => sup.Name.ToLower().Contains(s) ||
                               (sup.City != null && sup.City.ToLower().Contains(s)) ||
                               (sup.Email != null && sup.Email.ToLower().Contains(s)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)x.Name);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        var supplierIds = items.Select(s => (int)s.Id).ToList();

        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.SupplierId.HasValue && supplierIds.Contains(a.SupplierId.Value))
            .GroupBy(a => a.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId!.Value, x => x.Count, cancellationToken);

        var licenseCounts = await _context.Licenses
            .Where(l => l.SupplierId.HasValue && supplierIds.Contains(l.SupplierId.Value))
            .GroupBy(l => l.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId!.Value, x => x.Count, cancellationToken);

        var accessoryCounts = await _context.Accessories
            .Where(a => a.SupplierId.HasValue && supplierIds.Contains(a.SupplierId.Value))
            .GroupBy(a => a.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(s => MapToDto(s,
            assetCounts.TryGetValue((int)s.Id, out var ac) ? ac : 0,
            licenseCounts.TryGetValue((int)s.Id, out var lc) ? lc : 0,
            accessoryCounts.TryGetValue((int)s.Id, out var acc) ? acc : 0)).ToList();

        return new PagedResult<SupplierResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<SupplierResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
        return entity is null ? null : MapToDto(entity, 0, 0, 0);
    }

    public async Task<SupplierResponseDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Supplier
        {
            Name = dto.Name, Address = dto.Address, Address2 = dto.Address2,
            City = dto.City, State = dto.State, Country = dto.Country, Zip = dto.Zip,
            Phone = dto.Phone, Fax = dto.Fax, Email = dto.Email, Url = dto.Url,
            Contact = dto.Contact, Image = dto.Image, Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _context.Suppliers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier {Name} created with ID {Id}", entity.Name, entity.Id);
        return MapToDto(entity, 0, 0, 0);
    }

    public async Task<SupplierResponseDto?> UpdateAsync(uint id, UpdateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Address is not null) entity.Address = dto.Address;
        if (dto.Address2 is not null) entity.Address2 = dto.Address2;
        if (dto.City is not null) entity.City = dto.City;
        if (dto.State is not null) entity.State = dto.State;
        if (dto.Country is not null) entity.Country = dto.Country;
        if (dto.Zip is not null) entity.Zip = dto.Zip;
        if (dto.Phone is not null) entity.Phone = dto.Phone;
        if (dto.Fax is not null) entity.Fax = dto.Fax;
        if (dto.Email is not null) entity.Email = dto.Email;
        if (dto.Url is not null) entity.Url = dto.Url;
        if (dto.Contact is not null) entity.Contact = dto.Contact;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier {Id} updated", id);
        return MapToDto(entity, 0, 0, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier {Id} soft-deleted", id);
        return true;
    }

    private static SupplierResponseDto MapToDto(Supplier s, int assetsCount, int licensesCount, int accessoriesCount) => new()
    {
        Id = s.Id, Name = s.Name, Address = s.Address, Address2 = s.Address2,
        City = s.City, State = s.State, Country = s.Country, Zip = s.Zip,
        Phone = s.Phone, Fax = s.Fax, Email = s.Email, Url = s.Url,
        Contact = s.Contact, Image = s.Image, Notes = s.Notes,
        CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt,
        AssetsCount = assetsCount, LicensesCount = licensesCount, AccessoriesCount = accessoriesCount
    };
}
