using MiniMESApp.Data;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;
using AKNexus.Framework.Abstractions.Dtos;

namespace MiniMESApp.Repositories;

public class ProductStationLogRepository : IProductStationLogRepository
{
    private readonly DasAppDbContext _db;
    public ProductStationLogRepository(DasAppDbContext db) => _db = db;

    public async Task<long> InsertAsync(ProductStationLog log)
    {
        await _db.ProductStationLogs.AddAsync(log);
        await _db.SaveChangesAsync();
        return log.Id;
    }

    public Task<ProductStationLog?> GetLastLog(int productId, int flowId)
        => _db.ProductStationLogs
            .Where(l => l.ProductId == productId && l.FlowId == flowId)
            .OrderByDescending(l => l.PassTime)
            .FirstOrDefaultAsync();

    public async Task<PagedResult<ProductStationLogDto>> QueryAsync(ProductQueryRequest req)
    {
        var query = from log in _db.ProductStationLogs
                    join prod in _db.Products on log.ProductId equals prod.Id
                    join st in _db.Stations on log.StationId equals st.Id
                    join fl in _db.StationFlows on log.FlowId equals fl.Id
                    select new ProductStationLogDto
                    {
                        Id = log.Id,
                        ProductCode = prod.ProductCode,
                        ProductName = prod.ProductName,
                        StationCode = st.StationCode,
                        StationName = st.StationName,
                        FlowCode = fl.FlowCode,
                        PassResult = log.PassResult,
                        Remark = log.Remark,
                        Operator = log.Operator,
                        PassTime = log.PassTime
                    };

        if (!string.IsNullOrWhiteSpace(req.ProductCode))
            query = query.Where(q => q.ProductCode.Contains(req.ProductCode));
        if (!string.IsNullOrWhiteSpace(req.StationCode))
            query = query.Where(q => q.StationCode == req.StationCode);
        if (!string.IsNullOrWhiteSpace(req.FlowCode))
            query = query.Where(q => q.FlowCode == req.FlowCode);
        if (!string.IsNullOrWhiteSpace(req.PassResult))
            query = query.Where(q => q.PassResult == req.PassResult);
        if (req.StartTime.HasValue)
            query = query.Where(q => q.PassTime >= req.StartTime.Value);
        if (req.EndTime.HasValue)
            query = query.Where(q => q.PassTime <= req.EndTime.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(q => q.PassTime)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return new PagedResult<ProductStationLogDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = req.Page,
            PageSize = req.PageSize
        };
    }
}
