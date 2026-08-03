using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DasAppDbContext _db;
    public ProductRepository(DasAppDbContext db) => _db = db;

    public Task<Product?> GetByCode(string code)
        => _db.Products.FirstOrDefaultAsync(p => p.ProductCode == code);

    public Task<Product?> GetById(int id)
        => _db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public Task<List<Product>> GetAllAsync()
        => _db.Products.OrderBy(p => p.ProductCode).ToListAsync();

    public async Task<int> InsertAsync(Product product)
    {
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();
        return product.Id;
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
    }
}
