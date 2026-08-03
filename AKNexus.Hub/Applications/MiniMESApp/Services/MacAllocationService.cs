using System.Globalization;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;

namespace MiniMESApp.Services;

public class MacAllocationService
{
    private readonly IMacRangeRepository _rangeRepo;
    private readonly IMacAllocationRepository _allocRepo;

    public MacAllocationService(IMacRangeRepository rangeRepo, IMacAllocationRepository allocRepo)
    {
        _rangeRepo = rangeRepo;
        _allocRepo = allocRepo;
    }

    public async Task<MacAllocationResult> AllocateAsync(string? allocatedTo)
    {
        var ranges = await _rangeRepo.GetAllAsync();
        var activeRanges = ranges.Where(r => r.IsActive).ToList();
        if (!activeRanges.Any())
            return MacAllocationResult.Fail("没有可用的 MAC 地址段");

        var sorted = activeRanges
            .Select(r => new { Range = r, StartLong = MacToLong(r.StartMac), EndLong = MacToLong(r.EndMac) })
            .Where(r => r.StartLong >= 0 && r.EndLong >= 0)
            .OrderBy(r => r.StartLong)
            .ToList();

        foreach (var item in sorted)
        {
            var nextLong = MacToLong(item.Range.NextMac);
            if (nextLong < item.StartLong)
                nextLong = item.StartLong;

            if (nextLong > item.EndLong)
                continue;

            var macAddress = LongToMac(nextLong);
            var allocation = new MacAllocation
            {
                MacRangeId = item.Range.Id,
                MacAddress = macAddress,
                AllocatedTo = allocatedTo,
                AllocatedAt = DateTime.UtcNow
            };
            await _allocRepo.InsertAsync(allocation);

            item.Range.NextMac = LongToMac(nextLong + 1);
            await _rangeRepo.UpdateAsync(item.Range);

            return MacAllocationResult.Ok(macAddress);
        }

        return MacAllocationResult.Fail("所有 MAC 地址段已分配完毕");
    }

    public static long MacToLong(string mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return -1;
        try
        {
            var hex = mac.Replace(":", "").Replace("-", "").Replace(" ", "");
            if (hex.Length > 12) return -1;
            return long.Parse(hex, NumberStyles.HexNumber);
        }
        catch
        {
            return -1;
        }
    }

    public static string LongToMac(long value)
    {
        if (value < 0 || value > 0xFFFF_FFFF_FFFF) return "00:00:00:00:00:00";
        return string.Join(":", new byte[] {
            (byte)((value >> 40) & 0xFF),
            (byte)((value >> 32) & 0xFF),
            (byte)((value >> 24) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF)
        }.Select(b => b.ToString("X2")));
    }
}
