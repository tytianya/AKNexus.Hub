using Furion;
using Furion.DataValidation;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AKNexus.Framework.Web.Middlewares;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = GetExceptionDetails(exception);
        response.StatusCode = (int)statusCode;

        var result = new
        {
            Success = false,
            Message = message,
            ErrorCode = statusCode.ToString(),
            TraceId = context.TraceIdentifier
        };

        await response.WriteAsJsonAsync(result);
    }

    private static (HttpStatusCode statusCode, string message) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException _ => (HttpStatusCode.BadRequest, "参数不能为空"),
            ArgumentException _ => (HttpStatusCode.BadRequest, "参数错误"),
            InvalidOperationException _ => (HttpStatusCode.BadRequest, "操作无效"),
            UnauthorizedAccessException _ => (HttpStatusCode.Unauthorized, "未授权访问"),
            NotImplementedException _ => (HttpStatusCode.NotImplemented, "功能未实现"),
            TimeoutException _ => (HttpStatusCode.RequestTimeout, "请求超时"),
            ValidationException _ => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "服务器内部错误")
        };
    }
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalException(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}