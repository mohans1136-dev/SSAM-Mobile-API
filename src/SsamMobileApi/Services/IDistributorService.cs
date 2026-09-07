using SsamMobileApi.Models;

namespace SsamMobileApi.Services;

public interface IDistributorService
{
    Task<PagedResult<DistributorDto>> GetAsync(int page, int pageSize, string? search, CancellationToken ct);
    Task<DistributorDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<DistributorDto> CreateAsync(CreateDistributorRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateDistributorRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
