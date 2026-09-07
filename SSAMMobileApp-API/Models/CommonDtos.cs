namespace SSAMMobileApp.Models;

/// <summary>Standard envelope for paged list responses.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
