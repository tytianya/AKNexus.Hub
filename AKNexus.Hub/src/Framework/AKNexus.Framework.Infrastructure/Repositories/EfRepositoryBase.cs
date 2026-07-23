using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Abstractions.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace AKNexus.Framework.Infrastructure.Repositories;

/// <summary>
/// EF Core 仓储基类实现
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
/// <typeparam name="TDbContext">数据库上下文类型</typeparam>
public abstract class EfRepositoryBase<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    protected EfRepositoryBase(TDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id! }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 条件查询
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> WhereAsync(
        System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取可查询对象（支持LINQ）
    /// </summary>
    public virtual IQueryable<TEntity> Query()
    {
        return DbSet.AsQueryable();
    }

    /// <summary>
    /// 原生SQL查询（返回DTO）
    /// </summary>
    public virtual async Task<IEnumerable<TDto>> FromSqlRawAsync<TDto>(string sql, params object[] parameters)
        where TDto : class
    {
        return await DbContext.Database.SqlQueryRaw<TDto>(sql, parameters).ToListAsync();
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
}

/// <summary>
/// EF Core 仓储基类 - 默认Guid主键
/// </summary>
public abstract class EfRepositoryBase<TEntity, TDbContext> : EfRepositoryBase<TEntity, Guid, TDbContext>
    where TEntity : class
    where TDbContext : DbContext
{
    protected EfRepositoryBase(TDbContext dbContext) : base(dbContext)
    {
    }
}