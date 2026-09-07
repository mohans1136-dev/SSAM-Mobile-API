using SsamMobileApi.Models;

namespace SsamMobileApi.Services;

/// <summary>Read-side operations over MTL.MR / MTL.MRDetail.</summary>
public interface IMrService
{
    Task<PagedResult<MrListItemDto>> GetAsync(MrQuery query, CancellationToken ct);

    /// <summary>MR header with all its detail lines, or null if not found.</summary>
    Task<MrDto?> GetByIdAsync(int mrId, CancellationToken ct);

    /// <summary>Detail lines for one MR (empty list if the MR has none or doesn't exist).</summary>
    Task<IReadOnlyList<MrDetailDto>> GetDetailsAsync(int mrId, CancellationToken ct);
}

/// <summary>Filters for the MR list endpoint. All optional.</summary>
public class MrQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int? TenantId { get; set; }
    public int? CustomerId { get; set; }
    public int? LocationId { get; set; }
    public int? Status { get; set; }
    public string? RequestType { get; set; }
    public DateOnly? MonthYear { get; set; }
    public string? RefId { get; set; }
}
