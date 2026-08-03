using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class ProductFlowBindingRepository : IProductFlowBindingRepository
{
    private readonly DasAppDbContext _db;
    public ProductFlowBindingRepository(DasAppDbContext db) => _db = db;

    public Task<ProductFlowBinding?> GetByProductId(int productId)
        => _db.ProductFlowBindings.FirstOrDefaultAsync(b => b.ProductId == productId);

    public async Task<ProductFlowBinding?> GetByProductCode(string productCode)
    {
        var result = await (from b in _db.ProductFlowBindings
                            join p in _db.Products on b.ProductId equals p.Id
                            where p.ProductCode == productCode
                            select b).FirstOrDefaultAsync();
        return result;
    }

    public async Task<int> InsertAsync(ProductFlowBinding binding)
    {
        await _db.ProductFlowBindings.AddAsync(binding);
        await _db.SaveChangesAsync();
        return binding.Id;
    }

    public async Task UpdateAsync(ProductFlowBinding binding)
    {
        _db.ProductFlowBindings.Update(binding);
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, (int FlowId, string FlowCode)>> GetAllBindingsAsync()
    {
        var list = await (from b in _db.ProductFlowBindings
                          join f in _db.StationFlows on b.FlowId equals f.Id
                          select new { b.ProductId, b.FlowId, f.FlowCode }).ToListAsync();
        return list.ToDictionary(x => x.ProductId, x => (x.FlowId, x.FlowCode));
    }
}
