using AKNexus.Framework.Abstractions.Dtos;

namespace AKNexus.Framework.Application.Dtos;

/// <summary>
/// 通用响应结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class Result<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static Result<T> Ok(T data, string message = "操作成功")
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static Result<T> Fail(string message, string? errorCode = null)
    {
        return new Result<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// 无数据的通用响应结果
/// </summary>
public class Result
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static Result Ok(string message = "操作成功")
    {
        return new Result { Success = true, Message = message };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static Result Fail(string message, string? errorCode = null)
    {
        return new Result
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
    }
}