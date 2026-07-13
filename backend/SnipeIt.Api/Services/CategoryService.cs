using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for category management operations.</summary>
public interface ICategoryService
{
    Task<PagedResult<CategoryResponseDto>> GetCategoriesAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto?> UpdateAsync(uint id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class CategoryService : ICategoryService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(SnipeitContext context, ILogger<CategoryService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<CategoryResponseDto>> GetCategoriesAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Categories.Where(c => c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(s) ||
                             (c.CategoryType != null && c.CategoryType.ToLower().Contains(s)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)x.Name);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        // Batch count assets per category by joining through models
        var categoryIds = items.Select(c => (int)c.Id).ToList();
        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.ModelId.HasValue)
            .Join(_context.Models.Where(m => m.DeletedAt == null && m.CategoryId.HasValue && categoryIds.Contains(m.CategoryId!.Value)),
                  a => a.ModelId, m => (int?)m.Id, (a, m) => m.CategoryId!.Value)
            .GroupBy(cid => cid)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        var data = items.Select(c => MapToDto(c, assetCounts.TryGetValue((int)c.Id, out var cnt) ? cnt : 0)).ToList();
        return new PagedResult<CategoryResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        return entity is null ? null : MapToDto(entity, 0);
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Category
        {
            Name = dto.Name,
            CategoryType = dto.CategoryType,
            UseDefaultEula = dto.UseDefaultEula,
            RequireAcceptance = dto.RequireAcceptance,
            CheckinEmail = dto.CheckinEmail,
            AlertOnResponse = dto.AlertOnResponse,
            TagColor = dto.TagColor,
            Image = dto.Image,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {Name} created with ID {Id}", entity.Name, entity.Id);
        return MapToDto(entity, 0);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(uint id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.CategoryType is not null) entity.CategoryType = dto.CategoryType;
        if (dto.UseDefaultEula.HasValue) entity.UseDefaultEula = dto.UseDefaultEula.Value;
        if (dto.RequireAcceptance.HasValue) entity.RequireAcceptance = dto.RequireAcceptance.Value;
        if (dto.CheckinEmail.HasValue) entity.CheckinEmail = dto.CheckinEmail.Value;
        if (dto.AlertOnResponse.HasValue) entity.AlertOnResponse = dto.AlertOnResponse.Value;
        if (dto.TagColor is not null) entity.TagColor = dto.TagColor;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {Id} updated", id);
        return MapToDto(entity, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Category {Id} soft-deleted", id);
        return true;
    }

    private static CategoryResponseDto MapToDto(Category c, int assetsCount) => new()
    {
        Id = c.Id,
        Name = c.Name,
        CategoryType = c.CategoryType,
        UseDefaultEula = c.UseDefaultEula,
        RequireAcceptance = c.RequireAcceptance,
        CheckinEmail = c.CheckinEmail,
        AlertOnResponse = c.AlertOnResponse,
        TagColor = c.TagColor,
        Image = c.Image,
        Notes = c.Notes,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        AssetsCount = assetsCount
    };
}
