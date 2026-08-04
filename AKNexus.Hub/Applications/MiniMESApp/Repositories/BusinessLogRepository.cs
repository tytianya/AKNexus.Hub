using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public interface IBusinessLogRepository
{
    Task<(List<BusinessLog> Items, int Total)> GetPagedAsync(
        BusinessType? type, BusinessStatus? status, string? keyword,
        int pageNumber, int pageSize);

    Task InsertAsync(BusinessLog entity);
}

public class BusinessLogRepository : IBusinessLogRepository
{
    private readonly DasAppDbContext _db;
    public BusinessLogRepository(DasAppDbContext db) => _db = db;

    public async Task<(List<BusinessLog> Items, int Total)> GetPagedAsync(
        BusinessType? type, BusinessStatus? status, string? keyword,
        int pageNumber, int pageSize)
    {
        var query = _db.BusinessLogs.AsQueryable();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.Code.Contains(keyword) || x.Content.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task InsertAsync(BusinessLog entity)
    {
        await _db.BusinessLogs.AddAsync(entity);
        await _db.SaveChangesAsync();
    }
}
