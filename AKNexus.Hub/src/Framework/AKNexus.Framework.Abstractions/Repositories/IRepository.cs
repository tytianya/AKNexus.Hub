namespace AKNexus.Framework.Abstractions.Repositories;

/// <summary>
/// 仓储基础接口 - 定义数据访问基本操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取所有实体
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 添加实体
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 批量添加实体
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新实体
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 删除实体
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据ID删除实体
    /// </summary>
    Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 判断实体是否存在
    /// </summary>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取实体数量
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 仓储基础接口 - 默认Guid主键
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
{
}