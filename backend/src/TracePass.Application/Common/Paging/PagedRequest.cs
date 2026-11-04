namespace TracePass.Application.Common.Paging;

public class PagedRequest
{
    private const int MaxPageSize = 500;

    public int? PageNumber { get; set; } = 1;

    public int? PageSize { get; set; }

    public string? SearchTerm { get; set; }

    public string? SortBy { get; set; }

    public bool? SortDescending { get; set; } = false;

    // Helper properties for easy consumption in services
    public int ActualPageNumber => PageNumber.HasValue && PageNumber.Value > 0 ? PageNumber.Value : 1;

    public bool IsPagingRequested => PageSize.HasValue && PageSize.Value > 0;

    public int ActualPageSize => PageSize.HasValue && PageSize.Value > 0
        ? Math.Min(PageSize.Value, MaxPageSize)
        : MaxPageSize;
}