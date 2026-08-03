using AKNexus.Framework.Application.Dtos;
using MiniMESApp.Dtos;
using MiniMESApp.Entities;
using MiniMESApp.Repositories;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;

namespace MiniMESApp.Services;

/// <summary>
/// 测试软件管理服务接口
/// </summary>
public interface ITestSoftwareService
{
    /// <summary>
    /// 获取所有测试软件
    /// </summary>
    Task<Result<List<TestSoftwareDto>>> GetAllAsync();

    /// <summary>
    /// 上传测试软件
    /// </summary>
    /// <param name="softwareName">软件名称</param>
    /// <param name="softwareCode">软件编码</param>
    /// <param name="version">版本号</param>
    /// <param name="description">描述</param>
    /// <param name="file">软件文件</param>
    Task<Result<TestSoftwareDto>> UploadAsync([FromForm] string softwareName, [FromForm] string softwareCode, [FromForm] string version, [FromForm] string? description, IFormFile file);

    /// <summary>
    /// 下载测试软件
    /// </summary>
    /// <param name="id">软件ID</param>
    Task<IActionResult> DownloadAsync(int id);

    /// <summary>
    /// 删除测试软件
    /// </summary>
    /// <param name="id">软件ID</param>
    Task<Result> DeleteAsync(int id);

    /// <summary>
    /// 绑定软件到站点
    /// </summary>
    /// <param name="req">绑定请求</param>
    Task<Result> BindAsync(SoftwareBindRequest req);

    /// <summary>
    /// 解绑站点软件
    /// </summary>
    /// <param name="bindingId">绑定ID</param>
    Task<Result> UnbindAsync(int bindingId);

    /// <summary>
    /// 获取站点绑定的软件列表
    /// </summary>
    /// <param name="stationId">站点ID</param>
    Task<Result<List<StationSoftwareDto>>> GetStationSoftwareAsync(int stationId);

    /// <summary>
    /// 获取站点最新版本软件
    /// </summary>
    /// <param name="stationId">站点ID</param>
    Task<Result<List<StationSoftwareDto>>> GetStationLatestAsync(int stationId);
}

/// <summary>
/// 测试软件管理服务 - 动态 API
/// </summary>
[ApiDescriptionSettings("TestSoftware", Name = "TestSoftware")]
public class TestSoftwareService : ITestSoftwareService, IDynamicApiController
{
    private readonly ITestSoftwareRepository _softRepo;
    private readonly IStationSoftwareBindingRepository _bindRepo;
    private readonly IStationRepository _stationRepo;
    private readonly IWebHostEnvironment _env;

    public TestSoftwareService(
        ITestSoftwareRepository softRepo,
        IStationSoftwareBindingRepository bindRepo,
        IStationRepository stationRepo,
        IWebHostEnvironment env)
    {
        _softRepo = softRepo;
        _bindRepo = bindRepo;
        _stationRepo = stationRepo;
        _env = env;
    }

    /// <summary>
    /// 获取所有测试软件
    /// </summary>
    /// <remarks>GET /api/TestSoftware</remarks>
    [HttpGet("")]
    public async Task<Result<List<TestSoftwareDto>>> GetAllAsync()
    {
        var list = await _softRepo.GetAllAsync();
        return Result<List<TestSoftwareDto>>.Ok(list.Select(ToDto).ToList());
    }

    /// <summary>
    /// 上传测试软件
    /// </summary>
    /// <remarks>POST /api/TestSoftware/upload</remarks>
    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)]
    public async Task<Result<TestSoftwareDto>> UploadAsync([FromForm] string softwareName, [FromForm] string softwareCode, [FromForm] string version, [FromForm] string? description, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Result<TestSoftwareDto>.Fail("请选择要上传的文件");
        if (string.IsNullOrWhiteSpace(softwareName) || string.IsNullOrWhiteSpace(softwareCode) || string.IsNullOrWhiteSpace(version))
            return Result<TestSoftwareDto>.Fail("软件名称、编码、版本不能为空");

        var uploadDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "software", softwareCode, version);
        Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, file.FileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var software = new TestSoftware
        {
            SoftwareName = softwareName.Trim(),
            SoftwareCode = softwareCode.Trim(),
            Version = version.Trim(),
            FileName = file.FileName,
            FilePath = Path.Combine("uploads", "software", softwareCode, version, file.FileName).Replace("\\", "/"),
            FileSize = file.Length,
            Description = description?.Trim(),
            IsActive = true,
            UploadedAt = DateTime.UtcNow
        };
        software.Id = await _softRepo.InsertAsync(software);
        return Result<TestSoftwareDto>.Ok(ToDto(software), "上传成功");
    }

    /// <summary>
    /// 下载测试软件
    /// </summary>
    /// <remarks>GET /api/TestSoftware/download/{id}</remarks>
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadAsync(int id)
    {
        var soft = await _softRepo.GetById(id);
        if (soft == null) return new NotFoundResult();

        var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", soft.FilePath);
        if (!System.IO.File.Exists(filePath)) return new NotFoundResult();

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = soft.FileName };
    }

    /// <summary>
    /// 删除测试软件
    /// </summary>
    /// <remarks>DELETE /api/TestSoftware/{id}</remarks>
    [HttpDelete("{id}")]
    public async Task<Result> DeleteAsync(int id)
    {
        var soft = await _softRepo.GetById(id);
        if (soft == null) return Result.Fail("资源不存在");

        var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", soft.FilePath);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        await _softRepo.DeleteAsync(id);
        return Result.Ok("删除成功");
    }

    /// <summary>
    /// 绑定软件到站点
    /// </summary>
    /// <remarks>POST /api/TestSoftware/bind</remarks>
    [HttpPost("bind")]
    public async Task<Result> BindAsync(SoftwareBindRequest req)
    {
        var station = await _stationRepo.GetById(req.StationId);
        if (station == null) return Result.Fail("站点不存在");
        var soft = await _softRepo.GetById(req.SoftwareId);
        if (soft == null) return Result.Fail("软件不存在");

        var existing = await _bindRepo.GetByStationAndSoftware(req.StationId, req.SoftwareId);
        if (existing != null) return Result.Fail("该站点已绑定此软件");

        await _bindRepo.InsertAsync(new StationSoftwareBinding
        {
            StationId = req.StationId,
            SoftwareId = req.SoftwareId,
            BoundAt = DateTime.UtcNow
        });
        return Result.Ok("绑定成功");
    }

    /// <summary>
    /// 解绑站点软件
    /// </summary>
    /// <remarks>DELETE /api/TestSoftware/unbind/{bindingId}</remarks>
    [HttpDelete("unbind/{bindingId}")]
    public async Task<Result> UnbindAsync(int bindingId)
    {
        await _bindRepo.DeleteAsync(bindingId);
        return Result.Ok("解绑成功");
    }

    /// <summary>
    /// 获取站点绑定的软件列表
    /// </summary>
    /// <remarks>GET /api/TestSoftware/station/{stationId}</remarks>
    [HttpGet("station/{stationId}")]
    public async Task<Result<List<StationSoftwareDto>>> GetStationSoftwareAsync(int stationId)
    {
        var bindings = await _bindRepo.GetByStation(stationId);
        var station = await _stationRepo.GetById(stationId);

        var result = new List<StationSoftwareDto>();
        foreach (var b in bindings)
        {
            var soft = await _softRepo.GetById(b.SoftwareId);
            if (soft == null) continue;
            result.Add(new StationSoftwareDto
            {
                Id = soft.Id, SoftwareName = soft.SoftwareName, SoftwareCode = soft.SoftwareCode,
                Version = soft.Version, FileName = soft.FileName, FileSize = soft.FileSize,
                Description = soft.Description, BoundAt = b.BoundAt,
                StationName = station?.StationName, StationCode = station?.StationCode
            });
        }
        return Result<List<StationSoftwareDto>>.Ok(result);
    }

    /// <summary>
    /// 获取站点最新版本软件
    /// </summary>
    /// <remarks>GET /api/TestSoftware/station/{stationId}/latest</remarks>
    [HttpGet("station/{stationId}/latest")]
    public async Task<Result<List<StationSoftwareDto>>> GetStationLatestAsync(int stationId)
    {
        var bindings = await _bindRepo.GetByStation(stationId);
        var station = await _stationRepo.GetById(stationId);
        var all = await _softRepo.GetAllAsync();

        var result = new List<StationSoftwareDto>();
        foreach (var b in bindings)
        {
            var soft = await _softRepo.GetById(b.SoftwareId);
            if (soft == null || !soft.IsActive) continue;

            var latest = all
                .Where(s => s.SoftwareCode == soft.SoftwareCode && s.IsActive)
                .OrderByDescending(s => s.UploadedAt)
                .FirstOrDefault();

            if (latest != null)
            {
                result.Add(new StationSoftwareDto
                {
                    Id = latest.Id, SoftwareName = latest.SoftwareName, SoftwareCode = latest.SoftwareCode,
                    Version = latest.Version, FileName = latest.FileName, FileSize = latest.FileSize,
                    Description = latest.Description, BoundAt = b.BoundAt,
                    StationName = station?.StationName, StationCode = station?.StationCode
                });
            }
        }
        return Result<List<StationSoftwareDto>>.Ok(result);
    }

    private static TestSoftwareDto ToDto(TestSoftware s) => new()
    {
        Id = s.Id, SoftwareName = s.SoftwareName, SoftwareCode = s.SoftwareCode,
        Version = s.Version, FileName = s.FileName, FileSize = s.FileSize,
        Description = s.Description, IsActive = s.IsActive, UploadedAt = s.UploadedAt
    };
}
