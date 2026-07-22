using AKNexus.Framework.Abstractions.Repositories;
using AKNexus.Framework.Infrastructure.Repositories;
using AKNexus.Hub.Web.DbContexts;
using AKNexus.Hub.Web.Entities;

namespace AKNexus.Hub.Web.Repositories;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository : IRepository<User>
{
}

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : EfRepositoryBase<User, HubDbContext>, IUserRepository
{
    public UserRepository(HubDbContext dbContext) : base(dbContext)
    {
    }
}

/// <summary>
/// 通用仓储实现
/// </summary>
public class Repository<TEntity> : EfRepositoryBase<TEntity, Guid, HubDbContext>, IRepository<TEntity>
    where TEntity : class
{
    public Repository(HubDbContext dbContext) : base(dbContext)
    {
    }
}