using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Abstractions.UnitOfWork;
using AKNexus.Framework.Application.Contracts;
using AKNexus.Framework.Infrastructure.Data;
using Furion;
using Furion.DynamicApiController;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Furion.FriendlyException;
using AKNexus.Framework.Domain;
using AKNexus.Framework.Infrastructure.Entities;
using AKNexus.Framework.Application.Dtos;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace AKNexus.Framework.Web.Services;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取所有用户
    /// </summary>
    Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建用户
    /// </summary>
    Task<UserDto> CreateAsync(UserCreateInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用户
    /// </summary>
    Task UpdateAsync(Guid id, UserUpdateInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除用户
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 用户服务 - 动态 API
/// </summary>
/// <remarks>
/// 使用 [DynamicApiController] 特性，无需创建 Controller
/// 自动生成路由：/api/user/get-all-async, /api/user/get-by-id-async 等
/// </remarks>
[ApiDescriptionSettings("User", Name = "User")]
public class UserService : IUserService, IDynamicApiController
{
    private readonly IRepository<User> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IRepository<User> repository, IUnitOfWork unitOfWork,ILogger<UserService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger= logger; 
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    /// <remarks>GET /api/user/get-all-async</remarks>
    [HttpGet]
    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tts = AppDomain.CurrentDomain.GetAssemblies();
        var users = await _repository.GetAllAsync(cancellationToken);
        _logger.LogWarning("hello:" + JsonConvert.SerializeObject(users));
        return users.Adapt<List<UserDto>>();
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <remarks>GET /api/user/get-by-id-async?id={id}</remarks>
    [HttpGet]
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user?.Adapt<UserDto>();
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <remarks>POST /api/user/create-async</remarks>
    [HttpPost]
    public async Task<UserDto> CreateAsync(UserCreateInput input, CancellationToken cancellationToken = default)
    {
        var user = input.Adapt<User>();
        user.PasswordHash = HashPassword(input.Password);
        var result = await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result.Adapt<UserDto>();
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <remarks>PUT /api/user/update-async</remarks>
    [HttpPut]
    public async Task UpdateAsync(Guid id, UserUpdateInput input, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw Oops.Oh("用户不存在");

        input.Adapt(user);
        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <remarks>DELETE /api/user/delete-async?id={id}</remarks>
    [HttpDelete]
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteByIdAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string HashPassword(string password)
    {
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
    }
}