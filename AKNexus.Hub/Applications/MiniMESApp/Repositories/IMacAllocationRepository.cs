using MiniMESApp.Entities;

namespace MiniMESApp.Repositories;

public interface IMacAllocationRepository
{
    Task<long> InsertAsync(MacAllocation allocation);
    Task<List<MacAllocation>> GetAllAsync();
}
