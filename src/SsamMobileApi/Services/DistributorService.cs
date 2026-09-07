using SsamMobileApi.Data;
using SsamMobileApi.Data.Entities;
using SsamMobileApi.Models;
using Microsoft.EntityFrameworkCore;

namespace SsamMobileApi.Services;

/// <summary>
/// Business logic + data access for distributors. Controllers stay thin and
/// delegate here; this class is the only place that touches the DbContext.
/// </summary>
public class DistributorService(AppDbContext db, ILogger<DistributorService> logger) : IDistributorService
{
    public async Task<PagedResult<DistributorDto>> GetAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        // AsNoTracking: read-only query, skips EF change tracking = faster.
        var query = db.Distributors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // EF Core translates this to a parameterized SQL LIKE - no SQL injection.
            query = query.Where(d => d.Name.Contains(search) || (d.Region != null && d.Region.Contains(search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DistributorDto(d.Id, d.Name, d.Region, d.IsActive))
            .ToListAsync(ct);

        return new PagedResult<DistributorDto>(items, page, pageSize, total);
    }

    public async Task<DistributorDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Distributors.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DistributorDto(d.Id, d.Name, d.Region, d.IsActive))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DistributorDto> CreateAsync(CreateDistributorRequest request, CancellationToken ct)
    {
        var entity = new Distributor
        {
            Name = request.Name.Trim(),
            Region = request.Region?.Trim(),
            IsActive = request.IsActive
        };

        db.Distributors.Add(entity);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created distributor {DistributorId}", entity.Id);
        return new DistributorDto(entity.Id, entity.Name, entity.Region, entity.IsActive);
    }

    public async Task<bool> UpdateAsync(int id, UpdateDistributorRequest request, CancellationToken ct)
    {
        var entity = await db.Distributors.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (entity is null) return false;

        entity.Name = request.Name.Trim();
        entity.Region = request.Region?.Trim();
        entity.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var rows = await db.Distributors.Where(d => d.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }
}
