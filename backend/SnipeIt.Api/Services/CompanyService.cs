using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for company management operations.</summary>
public interface ICompanyService
{
    Task<PagedResult<CompanyResponseDto>> GetCompaniesAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<CompanyResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<CompanyResponseDto> CreateAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default);
    Task<CompanyResponseDto?> UpdateAsync(uint id, UpdateCompanyDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class CompanyService : ICompanyService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(SnipeitContext context, ILogger<CompanyService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<CompanyResponseDto>> GetCompaniesAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Companies.Where(c => c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(s) ||
                             (c.Email != null && c.Email.ToLower().Contains(s)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)x.Name);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        var companyIds = items.Select(c => c.Id).ToList();

        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.CompanyId.HasValue && companyIds.Contains(a.CompanyId.Value))
            .GroupBy(a => a.CompanyId)
            .Select(g => new { CompanyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CompanyId!.Value, x => x.Count, cancellationToken);

        var userCounts = await _context.Users
            .Where(u => u.CompanyId.HasValue && companyIds.Contains(u.CompanyId.Value))
            .GroupBy(u => u.CompanyId)
            .Select(g => new { CompanyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CompanyId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(c => MapToDto(c,
            assetCounts.TryGetValue(c.Id, out var ac) ? ac : 0,
            userCounts.TryGetValue(c.Id, out var uc) ? uc : 0)).ToList();

        return new PagedResult<CompanyResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<CompanyResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        return entity is null ? null : MapToDto(entity, 0, 0);
    }

    public async Task<CompanyResponseDto> CreateAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Company
        {
            Name = dto.Name, Email = dto.Email, Phone = dto.Phone,
            Fax = dto.Fax, Image = dto.Image, Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _context.Companies.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company {Name} created with ID {Id}", entity.Name, entity.Id);
        return MapToDto(entity, 0, 0);
    }

    public async Task<CompanyResponseDto?> UpdateAsync(uint id, UpdateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.Email is not null) entity.Email = dto.Email;
        if (dto.Phone is not null) entity.Phone = dto.Phone;
        if (dto.Fax is not null) entity.Fax = dto.Fax;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company {Id} updated", id);
        return MapToDto(entity, 0, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Company {Id} soft-deleted", id);
        return true;
    }

    private static CompanyResponseDto MapToDto(Company c, int assetsCount, int usersCount) => new()
    {
        Id = c.Id, Name = c.Name, Email = c.Email,
        Phone = c.Phone, Fax = c.Fax, Image = c.Image, Notes = c.Notes,
        CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt,
        AssetsCount = assetsCount, UsersCount = usersCount
    };
}
