using MiniMESApp.Data;
using MiniMESApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Repositories;

public class TestSoftwareRepository : ITestSoftwareRepository
{
    private readonly DasAppDbContext _db;
    public TestSoftwareRepository(DasAppDbContext db) => _db = db;

    public Task<List<TestSoftware>> GetAllAsync()
        => _db.TestSoftwares.OrderByDescending(s => s.UploadedAt).ToListAsync();

    public Task<TestSoftware?> GetById(int id)
        => _db.TestSoftwares.FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> InsertAsync(TestSoftware software)
    {
        await _db.TestSoftwares.AddAsync(software);
        await _db.SaveChangesAsync();
        return software.Id;
    }

    public async Task UpdateAsync(TestSoftware software)
    {
        _db.TestSoftwares.Update(software);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var sw = await _db.TestSoftwares.FirstOrDefaultAsync(s => s.Id == id);
        if (sw != null)
        {
            _db.TestSoftwares.Remove(sw);
            await _db.SaveChangesAsync();
        }
    }
}
