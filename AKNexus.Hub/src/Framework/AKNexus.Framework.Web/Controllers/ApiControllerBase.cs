using AKNexus.Framework.Abstractions.Dtos;
using AKNexus.Framework.Application.Dtos;
using Furion.UnifyResult;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AKNexus.Framework.Web.Controllers;

/// <summary>
/// API控制器基类 - 提供统一的响应格式
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// 返回成功结果
    /// </summary>
    protected IActionResult Success<T>(T data, string message = "操作成功")
    {
        return Ok(Result<T>.Ok(data, message));
    }

    /// <summary>
    /// 返回成功结果（无数据）
    /// </summary>
    protected IActionResult Success(string message = "操作成功")
    {
        return Ok(Result.Ok(message));
    }

    /// <summary>
    /// 返回失败结果
    /// </summary>
    protected IActionResult Fail(string message, string? errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        var result = Result.Fail(message, errorCode);
        return StatusCode((int)statusCode, result);
    }

    /// <summary>
    /// 返回失败结果（带数据）
    /// </summary>
    protected IActionResult Fail<T>(string message, string? errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        var result = Result<T>.Fail(message, errorCode);
        return StatusCode((int)statusCode, result);
    }

    /// <summary>
    /// 返回未找到结果
    /// </summary>
    protected IActionResult NotFoundResult(string message = "资源不存在")
    {
        return NotFound(Result.Fail(message, "NOT_FOUND"));
    }

    /// <summary>
    /// 返回分页结果
    /// </summary>
    protected IActionResult Paged<T>(PagedResult<T> result)
    {
        return Ok(result);
    }
}