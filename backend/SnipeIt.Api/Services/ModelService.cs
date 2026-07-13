using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>Contract for asset model management operations.</summary>
public interface IModelService
{
    Task<PagedResult<ModelResponseDto>> GetModelsAsync(PageQuery query, CancellationToken cancellationToken = default);
    Task<ModelResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);
    Task<ModelResponseDto> CreateAsync(CreateModelDto dto, CancellationToken cancellationToken = default);
    Task<ModelResponseDto?> UpdateAsync(uint id, UpdateModelDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class ModelService : IModelService
{
    private readonly SnipeitContext _context;
    private readonly ILogger<ModelService> _logger;

    public ModelService(SnipeitContext context, ILogger<ModelService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<ModelResponseDto>> GetModelsAsync(
        PageQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.Models.Where(m => m.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.ToLower();
            q = q.Where(m => m.Name.ToLower().Contains(s) ||
                             (m.ModelNumber != null && m.ModelNumber.ToLower().Contains(s)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, x => (object)x.Name);

        var total = await q.CountAsync(cancellationToken);
        var items = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        // Batch load related display names
        var mfrIds = items.Where(m => m.ManufacturerId.HasValue).Select(m => (uint)m.ManufacturerId!.Value).Distinct().ToList();
        var catIds = items.Where(m => m.CategoryId.HasValue).Select(m => (uint)m.CategoryId!.Value).Distinct().ToList();
        var modelIds = items.Select(m => (int?)m.Id).ToList();

        var manufacturers = mfrIds.Any()
            ? await _context.Manufacturers.Where(m => mfrIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken)
            : new Dictionary<uint, string>();

        var categories = catIds.Any()
            ? await _context.Categories.Where(c => catIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
            : new Dictionary<uint, string>();

        var assetCounts = await _context.Assets
            .Where(a => a.DeletedAt == null && a.ModelId.HasValue && modelIds.Contains(a.ModelId))
            .GroupBy(a => a.ModelId)
            .Select(g => new { ModelId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ModelId!.Value, x => x.Count, cancellationToken);

        var data = items.Select(m =>
        {
            var dto = MapToDto(m, assetCounts.TryGetValue((int)m.Id, out var ac) ? ac : 0);
            if (m.ManufacturerId.HasValue && manufacturers.TryGetValue((uint)m.ManufacturerId.Value, out var mfrName))
                dto.ManufacturerName = mfrName;
            if (m.CategoryId.HasValue && categories.TryGetValue((uint)m.CategoryId.Value, out var catName))
                dto.CategoryName = catName;
            return dto;
        }).ToList();

        return new PagedResult<ModelResponseDto>(data, query.Skip, query.Take, total);
    }

    public async Task<ModelResponseDto?> GetByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Models
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        var dto = MapToDto(entity, 0);
        if (entity.ManufacturerId.HasValue)
            dto.ManufacturerName = await _context.Manufacturers
                .Where(m => m.Id == (uint)entity.ManufacturerId.Value)
                .Select(m => m.Name).FirstOrDefaultAsync(cancellationToken);
        if (entity.CategoryId.HasValue)
            dto.CategoryName = await _context.Categories
                .Where(c => c.Id == (uint)entity.CategoryId.Value)
                .Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);
        return dto;
    }

    public async Task<ModelResponseDto> CreateAsync(CreateModelDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Model
        {
            Name = dto.Name,
            ModelNumber = dto.ModelNumber,
            ManufacturerId = dto.ManufacturerId,
            CategoryId = dto.CategoryId,
            DepreciationId = dto.DepreciationId,
            FieldsetId = dto.FieldsetId,
            Eol = dto.Eol,
            RequireSerial = dto.RequireSerial,
            Requestable = dto.Requestable,
            Image = dto.Image,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Models.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Model {Name} created with ID {Id}", entity.Name, entity.Id);
        return await GetByIdAsync(entity.Id, cancellationToken) ?? MapToDto(entity, 0);
    }

    public async Task<ModelResponseDto?> UpdateAsync(uint id, UpdateModelDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Models
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);
        if (entity is null) return null;

        if (dto.Name is not null) entity.Name = dto.Name;
        if (dto.ModelNumber is not null) entity.ModelNumber = dto.ModelNumber;
        if (dto.ManufacturerId.HasValue) entity.ManufacturerId = dto.ManufacturerId;
        if (dto.CategoryId.HasValue) entity.CategoryId = dto.CategoryId;
        if (dto.DepreciationId.HasValue) entity.DepreciationId = dto.DepreciationId;
        if (dto.FieldsetId.HasValue) entity.FieldsetId = dto.FieldsetId;
        if (dto.Eol.HasValue) entity.Eol = dto.Eol;
        if (dto.RequireSerial.HasValue) entity.RequireSerial = dto.RequireSerial.Value;
        if (dto.Requestable.HasValue) entity.Requestable = dto.Requestable.Value;
        if (dto.Image is not null) entity.Image = dto.Image;
        if (dto.Notes is not null) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Model {Id} updated", id);
        return await GetByIdAsync(id, cancellationToken) ?? MapToDto(entity, 0);
    }

    public async Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Models
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, cancellationToken);
        if (entity is null) return false;

        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Model {Id} soft-deleted", id);
        return true;
    }

    private static ModelResponseDto MapToDto(Model m, int assetsCount) => new()
    {
        Id = m.Id,
        Name = m.Name,
        ModelNumber = m.ModelNumber,
        ManufacturerId = m.ManufacturerId,
        CategoryId = m.CategoryId,
        DepreciationId = m.DepreciationId,
        FieldsetId = m.FieldsetId,
        Eol = m.Eol,
        RequireSerial = m.RequireSerial,
        Requestable = m.Requestable,
        Image = m.Image,
        Notes = m.Notes,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt,
        AssetsCount = assetsCount
    };
}
