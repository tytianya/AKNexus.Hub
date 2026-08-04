using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Linq.Expressions;

namespace AKNexus.Framework.Infrastructure.Data;

/// <summary>
/// 基础数据库上下文 - 提供公共配置
/// </summary>
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region 全局查询过滤器
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            //软删除
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");


                var property = Expression.Property(
                    parameter,
                    nameof(ISoftDelete.IsDeleted)
                );

                var condition = Expression.Equal(
                    property,
                    Expression.Constant(false)
                );

                var lambda = Expression.Lambda(
                    condition,
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(lambda);

            }
        } 
        #endregion

        // 应用所有配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateAuditEntities();
        FixUnspecifiedDateTimeKinds();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditEntities();
        FixUnspecifiedDateTimeKinds();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 自动修正所有 State 为 Added 或 Modified 实体中 Kind == Unspecified 的 DateTime 属性
    /// </summary>
    private void FixUnspecifiedDateTimeKinds()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                // C# 拆箱机制：当 CurrentValue 是 DateTime? 且不为空时，也会匹配为 DateTime
                if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }
    /// <summary>
    /// 自动更新审计字段
    /// </summary>
    private void UpdateAuditEntities()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedTime = now;
                    entry.Entity.UpdatedTime = null;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedTime = now;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedTime = now;
                    break;
            }
        }
    }
}

/// <summary>
/// 软删除接口标记
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedTime { get; set; }
}

/// <summary>
/// 审计接口
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedTime { get; set; }
    DateTimeOffset? UpdatedTime { get; set; }
}