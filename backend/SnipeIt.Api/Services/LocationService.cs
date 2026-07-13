using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for location management operations.</summary>
public interface ILocationService
{
    Task<PagedResult<LocationResponseDto>> GetLocationsAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<LocationResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<LocationResponseDto> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken = default);
    Task<LocationResponseDto?> UpdateAsync(uint id, UpdateLocationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class LocationService : ILocationService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<LocationService> _logger;

    public LocationService(SnipeitContext context, ILogger<LocationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<LocationResponseDto>> GetLocationsAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Locations.Where(l => l.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(l => (l.Name != null && l.Name.ToLower().Contains(s)) ||
                              (l.City != null && l.City.ToLower().Contains(s)) ||
                              (l.Country != null && l.Country.ToLower().Contains(s)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)(x.Name ?? ""));

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        var locationIds = items.Select(l => (int)l.Id).ToList();
        var locationUIds = items.Select(l => l.Id).ToList();

        var parentIds = items.Where(l => l.ParentId.HasValue).Select(l => (uint)l.ParentId!.Value).Distinct().ToList();
        var managerIds = items.Where(l => l.ManagerId.HasValue).Select(l => (uint)l.ManagerId!.Value).Distinct().ToList();

        var parentNames = parentIds.Any()
            ? await _context.Locations.Where(l => parentIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.Name ?? "", cancellationToken)
            : new Dictionary<uint, string>();

        var managerNames = managerIds.Any()
            ? await _context.Users.Where(u => managerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.FirstName ?? u.Username ?? "", cancellationToken)
            : new Dictionary<uint, string>();

        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.LocationId.HasValue && locationIds.Contains(a.LocationId.Value))
            .GroupBy(a => a.LocationId)
            .Select(g => new { LocId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LocId!.Value, x => x.Count, cancellationToken);

        var userCounts = await _context.Users
            .Where(u => u.LocationId.HasValue && locationIds.Contains(u.LocationId.Value))
            .GroupBy(u => u.LocationId)
            .Select(g => new { LocId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LocId!.Value, x => x.Count, cancellationToken);

        var childrenCounts = await _context.Locations
            .Where(l => l.DeletedAt == null && l.ParentId.HasValue && locationIds.Contains(l.ParentId.Value))
            .GroupBy(l => l.ParentId)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(l =>
        {
            var dto = MapToDto(l,
                assetCounts.TryGetValue((int)l.Id, out var ac) ? ac : 0,
                userCounts.TryGetValue((int)l.Id, out var uc) ? uc : 0,
                childrenCounts.TryGetValue((int)l.Id, out var cc) ? cc : 0);

            if (l.ParentId.HasValue && parentNames.TryGetValue((uint)l.ParentId.Value, out var pn))
                dto.ParentName = pn;
            if (l.ManagerId.HasValue && managerNames.TryGetValue((uint)l.ManagerId.Value, out var mn))
                dto.ManagerName = mn;
            return dto;
        }).ToList();

        return new PagedResult<LocationResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<LocationResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        var dto = MapToDto(entity, 0, 0, 0);
        if (entity.ParentId.HasValue)
            dto.ParentName = await _context.Locations.Where(l => l.Id == (uint)entity.ParentId.Value)
                .Select(l => l.Name).FirstOrDefaultAsync(cancellationToken);
        if (entity.ManagerId.HasValue)
            dto.ManagerName = await _context.Users.Where(u => u.Id == (uint)entity.ManagerId.Value)
                .Select(u => u.DisplayName ?? u.FirstName ?? u.Username).FirstOrDefaultAsync(cancellationToken);
        return dto;
    }

    public async Task<LocationResponseDto> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Location
        {
            Name = dto.Name, Address = dto.Address, Address2 = dto.Address2,
            City = dto.City, State = dto.State, Country = dto.Country, Zip = dto.Zip,
            Phone = dto.Phone, Fax = dto.Fax, Currency = dto.Currency,
            ParentId = dto.ParentId, ManagerId = dto.ManagerId, CompanyId = dto.CompanyId,
            Image = dto.Image, Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _context.Locations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Location {Name} created with ID {Id}", entity.Name, entity.Id);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? MapToDto(entity, 0, 0, 0);
    }

    public async Task<LocationResponseDto?> UpdateAsync(uint id, UpdateLocationDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, cancellationToken);
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
        if (dto.Currency is not null) entity.Currency = dto.Currency;
        if (dto.ParentId.HasValue) entity.ParentId = dto.ParentId;
        if (dto.ManagerId.HasValue) entity.ManagerId = dto.ManagerId;
        if (dto.CompanyId.HasValue) entity.CompanyId = dto.CompanyId;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Location {Id} updated", id);
        return await GetByIdAsync(id, cancellationToken) ?? MapToDto(entity, 0, 0, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Location {Id} soft-deleted", id);
        return true;
    }

    private static LocationResponseDto MapToDto(Location l, int assetsCount, int usersCount, int childrenCount) => new()
    {
        Id = l.Id, Name = l.Name, Address = l.Address, Address2 = l.Address2,
        City = l.City, State = l.State, Country = l.Country, Zip = l.Zip,
        Phone = l.Phone, Fax = l.Fax, Currency = l.Currency,
        ParentId = l.ParentId, ManagerId = l.ManagerId, CompanyId = l.CompanyId,
        Image = l.Image, Notes = l.Notes,
        CreatedAt = l.CreatedAt, UpdatedAt = l.UpdatedAt,
        AssetsCount = assetsCount, UsersCount = usersCount, ChildrenCount = childrenCount
    };
}
