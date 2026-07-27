namespace AKNexus.Framework.Application.Contracts;

/// <summary>
/// 服务接口契约 - 用于定义服务公共接口
/// </summary>
/// <typeparam name="TDto">DTO类型</typeparam>
/// <typeparam name="TCreateInput">创建输入类型</typeparam>
/// <typeparam name="TUpdateInput">更新输入类型</typeparam>
public interface IServiceContract<TDto, in TCreateInput, in TUpdateInput>
    where TDto : class
    where TCreateInput : class
    where TUpdateInput : class
{
    /// <summary>
    /// 根据ID获取
    /// </summary>
    Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有
    /// </summary>
    Task<List<TDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建
    /// </summary>
    Task<TDto> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新
    /// </summary>
    Task UpdateAsync(Guid id, TUpdateInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}