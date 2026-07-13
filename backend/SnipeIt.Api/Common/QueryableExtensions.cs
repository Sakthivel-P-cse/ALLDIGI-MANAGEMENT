using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace SnipeIt.Api.Common;

/// <summary>
/// Reusable IQueryable helpers for server-side sorting and pagination that every
/// list endpoint can share, so sorting/paging logic is written once rather than
/// re-implemented per controller.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic ordering by a property name supplied at runtime (e.g. from the
    /// query string). The name is resolved case-insensitively against the entity's public
    /// properties; if it does not match, <paramref name="defaultSort"/> is used instead.
    /// This validation is what makes accepting a string sort column safe — an unknown or
    /// malicious name can never reach <see cref="Expression.Property"/>.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">Source query.</param>
    /// <param name="sortBy">Requested property name (nullable).</param>
    /// <param name="descending">True for descending order.</param>
    /// <param name="defaultSort">Fallback expression applied when sortBy is missing/invalid.</param>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending,
        Expression<Func<T, object>> defaultSort)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return descending ? query.OrderByDescending(defaultSort) : query.OrderBy(defaultSort);
        }

        var property = typeof(T).GetProperty(
            sortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
        {
            // Unknown column — fall back to the safe default rather than throwing.
            return descending ? query.OrderByDescending(defaultSort) : query.OrderBy(defaultSort);
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var converted = Expression.Convert(propertyAccess, typeof(object));
        var lambda = Expression.Lambda<Func<T, object>>(converted, parameter);

        return descending ? query.OrderByDescending(lambda) : query.OrderBy(lambda);
    }

    /// <summary>
    /// Counts the matching rows, then applies skip/take and materializes a
    /// <see cref="PagedResult{TDto}"/>. The projection converts each entity to its DTO.
    /// </summary>
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        int skip,
        int take,
        Func<TEntity, TDto> projection,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        var data = items.Select(projection).ToList();
        return new PagedResult<TDto>(data, skip, take, total);
    }
}
