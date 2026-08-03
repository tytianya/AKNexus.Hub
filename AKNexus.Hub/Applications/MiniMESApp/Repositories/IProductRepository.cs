using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByCode(string code);
    Task<Product?> GetById(int id);
    Task<List<Product>> GetAllAsync();
    Task<int> InsertAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
