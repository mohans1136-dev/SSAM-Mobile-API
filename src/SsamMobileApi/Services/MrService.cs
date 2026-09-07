using SsamMobileApi.Data;
using SsamMobileApi.Models;
using Microsoft.EntityFrameworkCore;

namespace SsamMobileApi.Services;

public class MrService(AppDbContext db) : IMrService
{
    public async Task<PagedResult<MrListItemDto>> GetAsync(MrQuery q, CancellationToken ct)
    {
        var page = Math.Max(q.Page, 1);
        var pageSize = Math.Clamp(q.PageSize, 1, 200);

        var query = db.MR.AsNoTracking();

        if (q.TenantId is { } tenantId) query = query.Where(m => m.TenantId == tenantId);
        if (q.CustomerId is { } customerId) query = query.Where(m => m.CustomerId == customerId);
        if (q.LocationId is { } locationId) query = query.Where(m => m.LocationId == locationId);
        if (q.Status is { } status) query = query.Where(m => m.Status == status);
        if (q.MonthYear is { } monthYear) query = query.Where(m => m.MonthYear == monthYear);
        if (!string.IsNullOrWhiteSpace(q.RequestType)) query = query.Where(m => m.RequestType == q.RequestType);
        if (!string.IsNullOrWhiteSpace(q.RefId)) query = query.Where(m => m.RefId == q.RefId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.MRId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MrListItemDto(
                m.MRId, m.TenantId, m.RequestType, m.MonthYear, m.CustomerId, m.LocationId,
                m.RefId, m.RefDate, m.Status, m.ACKStatus,
                m.MRDetails.Count(), m.CreatedOn, m.UpdatedOn))
            .ToListAsync(ct);

        return new PagedResult<MrListItemDto>(items, page, pageSize, total);
    }

    public async Task<MrDto?> GetByIdAsync(int mrId, CancellationToken ct)
    {
        var mr = await db.MR.AsNoTracking()
            .Include(m => m.MRDetails)
            .FirstOrDefaultAsync(m => m.MRId == mrId, ct);

        return mr?.ToDto();
    }

    public async Task<IReadOnlyList<MrDetailDto>> GetDetailsAsync(int mrId, CancellationToken ct)
    {
        var rows = await db.MRDetail.AsNoTracking()
            .Where(d => d.MRId == mrId)
            .OrderBy(d => d.MRDetailId)
            .ToListAsync(ct);

        return rows.Select(d => d.ToDto()).ToList();
    }
}
