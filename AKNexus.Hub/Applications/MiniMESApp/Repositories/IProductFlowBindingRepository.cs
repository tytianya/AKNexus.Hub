using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IProductFlowBindingRepository
{
    Task<ProductFlowBinding?> GetByProductId(int productId);
    Task<ProductFlowBinding?> GetByProductCode(string productCode);
    Task<int> InsertAsync(ProductFlowBinding binding);
    Task UpdateAsync(ProductFlowBinding binding);
    Task<Dictionary<int, (int FlowId, string FlowCode)>> GetAllBindingsAsync();
}
