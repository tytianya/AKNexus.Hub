using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface ITestSoftwareRepository
{
    Task<List<TestSoftware>> GetAllAsync();
    Task<TestSoftware?> GetById(int id);
    Task<int> InsertAsync(TestSoftware software);
    Task UpdateAsync(TestSoftware software);
    Task DeleteAsync(int id);
}
