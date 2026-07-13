namespace SnipeIt.Api.Common;

/// <summary>
/// Standard query parameters for list endpoints: paging, searching, sorting.
/// Bound from the query string, e.g. ?skip=0&amp;take=50&amp;search=foo&amp;sortBy=name&amp;sortDir=desc
/// </summary>
public class PageQuery
{
    private const int MaxTake = 500;

    private int _skip;
    private int _take = 50;

    /// <summary>Number of records to skip (offset). Clamped to &gt;= 0.</summary>
    public int Skip
    {
        get => _skip;
        set => _skip = value < 0 ? 0 : value;
    }

    /// <summary>Number of records to return. Clamped to the range [1, 500].</summary>
    public int Take
    {
        get => _take;
        set => _take = value < 1 ? 1 : (value > MaxTake ? MaxTake : value);
    }

    /// <summary>Free-text search term applied by the service to relevant columns.</summary>
    public string? Search { get; set; }

    /// <summary>Property name to sort by (validated against a whitelist by the service).</summary>
    public string? SortBy { get; set; }

    /// <summary>Sort direction: "asc" or "desc" (default asc).</summary>
    public string? SortDir { get; set; }

    /// <summary>True when the caller requested descending order.</summary>
    public bool IsDescending =>
        string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);
}
