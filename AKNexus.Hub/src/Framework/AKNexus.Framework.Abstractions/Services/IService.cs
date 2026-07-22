namespace AKNexus.Framework.Abstractions.Services;

/// <summary>
/// 服务基础接口 - 定义服务基本行为
/// </summary>
public interface IService
{
}

/// <summary>
/// 服务基础接口 - 支持依赖注入标记
/// </summary>
/// <typeparam name="TService">服务类型</typeparam>
public interface IService<TService> : IService where TService : IService
{
}