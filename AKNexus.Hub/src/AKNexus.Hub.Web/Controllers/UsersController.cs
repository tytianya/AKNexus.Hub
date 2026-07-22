using AKNexus.Framework.Abstractions.Dtos;
using AKNexus.Framework.Web.Controllers;
using AKNexus.Hub.Web.Dtos;
using Microsoft.AspNetCore.Mvc;
using Furion;
using Asp.Versioning;

namespace AKNexus.Hub.Web.Controllers;

/// <summary>
/// 用户管理控制器
/// </summary>
[ApiVersion("1.0")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Success(users);
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFoundResult("用户不存在");
        }
        return Success(user);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateInput input, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(input, cancellationToken);
        return Success(user, "用户创建成功");
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateInput input, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.UpdateAsync(id, input, cancellationToken);
            return Success("用户更新成功");
        }
        catch (InvalidOperationException ex)
        {
            return NotFoundResult(ex.Message);
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return Success("用户删除成功");
    }

    /// <summary>
    /// 分页查询用户
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedList([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        // 这里简化处理，实际应该实现分页查询逻辑
        var allUsers = await _userService.GetAllAsync(cancellationToken);
        var result = new PagedResult<UserDto>
        {
            Items = allUsers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList(),
            TotalCount = allUsers.Count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        return Paged(result);
    }

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByUsernameAsync(username, cancellationToken);
        if (user == null)
        {
            return NotFoundResult("用户不存在");
        }
        return Success(user);
    }
}

/// <summary>
/// 健康检查控制器
/// </summary>
[ApiVersion("1.0")]
public class HealthController : ApiControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        return Success(new
        {
            System = "AKNexus Hub",
            Version = "1.0.0",
            Status = "Healthy",
            Time = DateTimeOffset.UtcNow
        });
    }
}