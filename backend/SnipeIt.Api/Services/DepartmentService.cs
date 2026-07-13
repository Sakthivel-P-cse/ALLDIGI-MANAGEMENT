using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for department management operations.</summary>
public interface IDepartmentService
{
    Task<PagedResult<DepartmentResponseDto>> GetDepartmentsAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto, int createdByUserId, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto?> UpdateAsync(uint id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class DepartmentService : IDepartmentService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(SnipeitContext context, ILogger<DepartmentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<DepartmentResponseDto>> GetDepartmentsAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Departments.Where(d => d.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(d => d.Name.ToLower().Contains(s));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)x.Name);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        var deptIds = items.Select(d => (int)d.Id).ToList();
        var companyIds = items.Where(d => d.CompanyId.HasValue).Select(d => (uint)d.CompanyId!.Value).Distinct().ToList();
        var locationIds = items.Where(d => d.LocationId.HasValue).Select(d => (uint)d.LocationId!.Value).Distinct().ToList();
        var managerIds = items.Where(d => d.ManagerId.HasValue).Select(d => (uint)d.ManagerId!.Value).Distinct().ToList();

        var companies = companyIds.Any()
            ? await _context.Companies.Where(c => companyIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
            : new Dictionary<uint, string>();

        var locations = locationIds.Any()
            ? await _context.Locations.Where(l => locationIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.Name ?? "", cancellationToken)
            : new Dictionary<uint, string>();

        var managers = managerIds.Any()
            ? await _context.Users.Where(u => managerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.FirstName ?? u.Username ?? "", cancellationToken)
            : new Dictionary<uint, string>();

        var userCounts = await _context.Users
            .Where(u => u.DepartmentId.HasValue && deptIds.Contains(u.DepartmentId.Value))
            .GroupBy(u => u.DepartmentId)
            .Select(g => new { DeptId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeptId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(d =>
        {
            var dto = MapToDto(d, userCounts.TryGetValue((int)d.Id, out var uc) ? uc : 0);
            if (d.CompanyId.HasValue && companies.TryGetValue((uint)d.CompanyId.Value, out var cn)) dto.CompanyName = cn;
            if (d.LocationId.HasValue && locations.TryGetValue((uint)d.LocationId.Value, out var ln)) dto.LocationName = ln;
            if (d.ManagerId.HasValue && managers.TryGetValue((uint)d.ManagerId.Value, out var mn)) dto.ManagerName = mn;
            return dto;
        }).ToList();

        return new PagedResult<DepartmentResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<DepartmentResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        var dto = MapToDto(entity, 0);
        if (entity.CompanyId.HasValue)
            dto.CompanyName = await _context.Companies.Where(c => c.Id == (uint)entity.CompanyId.Value)
                .Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);
        if (entity.LocationId.HasValue)
            dto.LocationName = await _context.Locations.Where(l => l.Id == (uint)entity.LocationId.Value)
                .Select(l => l.Name).FirstOrDefaultAsync(cancellationToken);
        if (entity.ManagerId.HasValue)
            dto.ManagerName = await _context.Users.Where(u => u.Id == (uint)entity.ManagerId.Value)
                .Select(u => u.DisplayName ?? u.FirstName ?? u.Username).FirstOrDefaultAsync(cancellationToken);
        return dto;
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto, int createdByUserId, CancellationToken cancellationToken = default)
    {
        var entity = new Department
        {
            Name = dto.Name,
            CompanyId = dto.CompanyId,
            LocationId = dto.LocationId,
            ManagerId = dto.ManagerId,
            Phone = dto.Phone,
            Fax = dto.Fax,
            Image = dto.Image,
            Notes = dto.Notes,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Departments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Department {Name} created with ID {Id}", entity.Name, entity.Id);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? MapToDto(entity, 0);
    }

    public async Task<DepartmentResponseDto?> UpdateAsync(uint id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.CompanyId.HasValue) entity.CompanyId = dto.CompanyId;
        if (dto.LocationId.HasValue) entity.LocationId = dto.LocationId;
        if (dto.ManagerId.HasValue) entity.ManagerId = dto.ManagerId;
        if (dto.Phone is not null) entity.Phone = dto.Phone;
        if (dto.Fax is not null) entity.Fax = dto.Fax;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Department {Id} updated", id);
        return await GetByIdAsync(id, cancellationToken) ?? MapToDto(entity, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Department {Id} soft-deleted", id);
        return true;
    }

    private static DepartmentResponseDto MapToDto(Department d, int usersCount) => new()
    {
        Id = d.Id, Name = d.Name,
        CompanyId = d.CompanyId, LocationId = d.LocationId, ManagerId = d.ManagerId,
        Phone = d.Phone, Fax = d.Fax, Image = d.Image, Notes = d.Notes,
        CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt,
        UsersCount = usersCount
    };
}
