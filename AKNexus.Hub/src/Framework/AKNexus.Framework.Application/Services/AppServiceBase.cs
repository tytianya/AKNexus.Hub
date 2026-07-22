using AKNexus.Framework.Abstractions.Dtos;
using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Abstractions.UnitOfWork;
using Mapster;
using Microsoft.Extensions.Logging;
namespace AKNexus.Framework.Application.Services;

/// <summary>
/// 应用服务基类 - 提供应用服务基本实现
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
/// <typeparam name="TDto">DTO类型</typeparam>
/// <typeparam name="TCreateInput">创建输入类型</typeparam>
/// <typeparam name="TUpdateInput">更新输入类型</typeparam>
/// <typeparam name="TRepository">仓储类型</typeparam>
public abstract class AppServiceBase<TEntity, TKey, TDto, TCreateInput, TUpdateInput, TRepository>
    where TEntity : class
    where TDto : class
    where TCreateInput : class
    where TUpdateInput : class
    where TRepository : IRepository<TEntity, TKey>
{
    protected readonly TRepository Repository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly ILogger Logger;

    protected AppServiceBase(
        TRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        Repository = repository;
        UnitOfWork = unitOfWork;
        Logger = logger;
    }

    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    public virtual async Task<TDto?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        return entity?.Adapt<TDto>();
    }

    /// <summary>
    /// 获取所有实体
    /// </summary>
    public virtual async Task<List<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken);
        return entities.Adapt<List<TDto>>();
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public virtual async Task<PagedResult<TDto>> GetPagedListAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        // 默认实现，子类可重写以支持更复杂的查询
        var allEntities = await Repository.GetAllAsync(cancellationToken);
        var totalCount = allEntities.Count();

        var items = allEntities
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<TDto>
        {
            Items = items.Adapt<List<TDto>>(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// 创建实体
    /// </summary>
    public virtual async Task<TDto> CreateAsync(TCreateInput input, CancellationToken cancellationToken = default)
    {
        var entity = input.Adapt<TEntity>();
        var result = await Repository.AddAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return result.Adapt<TDto>();
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    public virtual async Task UpdateAsync(TKey id, TUpdateInput input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new InvalidOperationException($"Entity with id {id} not found");

        input.Adapt(entity);
        await Repository.UpdateAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        await Repository.DeleteByIdAsync(id, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}