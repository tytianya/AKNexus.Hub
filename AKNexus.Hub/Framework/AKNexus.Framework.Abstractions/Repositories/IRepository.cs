namespace AKNexus.Framework.Abstractions.Repositories;

/// <summary>
/// 仓储基础接口 - 定义数据访问基本操作
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    #region 常规方法
		
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
    #endregion

    #region 高级方法

    /// <summary>
    /// 条件查询（场景一）
    /// </summary>
    Task<IEnumerable<TEntity>> WhereAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取可查询对象（场景二：LINQ复杂查询）
    /// </summary>
    IQueryable<TEntity> Query();

    /// <summary>
    /// 原生SQL查询（场景三：高性能DTO查询）
    /// </summary>
    Task<IEnumerable<TDto>> FromSqlRawAsync<TDto>(string sql, params object[] parameters) where TDto : class;

    #endregion
}

/// <summary>
/// 仓储基础接口 - 默认Guid主键
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
{
}