namespace SnipeIt.Api.Common;

/// <summary>
/// Generic paginated result envelope. The JSON shape (data/skip/take/total) is
/// intentionally identical to the existing <c>PaginatedResponse&lt;T&gt;</c> used by
/// AssetsController so the Angular clients that read <c>res.data</c>/<c>res.total</c>
/// keep working unchanged. Adds computed page metadata for newer clients.
/// </summary>
/// <typeparam name="T">Item DTO type.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>The page of items.</summary>
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();

    /// <summary>Number of records skipped (offset).</summary>
    public int Skip { get; init; }

    /// <summary>Page size requested.</summary>
    public int Take { get; init; }

    /// <summary>Total number of matching records across all pages.</summary>
    public int Total { get; init; }

    /// <summary>1-based page number derived from skip/take.</summary>
    public int Page => Take <= 0 ? 1 : (Skip / Take) + 1;

    /// <summary>Total number of pages derived from total/take.</summary>
    public int TotalPages => Take <= 0 ? 0 : (int)Math.Ceiling(Total / (double)Take);

    /// <summary>True when there is a further page after this one.</summary>
    public bool HasNext => Skip + Take < Total;

    /// <summary>True when there is a page before this one.</summary>
    public bool HasPrevious => Skip > 0;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> data, int skip, int take, int total)
    {
        Data = data;
        Skip = skip;
        Take = take;
        Total = total;
    }
}
