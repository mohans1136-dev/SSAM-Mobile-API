using System.ComponentModel.DataAnnotations;

namespace ConnectorDistributor.Api.Models;

/// <summary>
/// DTOs (Data Transfer Objects) are the shapes the API exposes over HTTP.
/// Keep them separate from EF entities so internal DB changes don't leak
/// into the public contract that OutSystems depends on.
/// </summary>
public record DistributorDto(int Id, string Name, string? Region, bool IsActive);

public class CreateDistributorRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Region { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateDistributorRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Region { get; set; }

    public bool IsActive { get; set; }
}

/// <summary>Standard envelope for paged list responses.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
