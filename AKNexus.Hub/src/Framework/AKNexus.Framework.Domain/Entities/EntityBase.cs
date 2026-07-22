using AKNexus.Framework.Abstractions.Entities;

namespace AKNexus.Framework.Domain.Entities;

/// <summary>
/// 实体基类 - 提供实体基本实现
/// </summary>
public abstract class EntityBase : IEntity
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    protected EntityBase()
    {
    }

    protected EntityBase(Guid id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(EntityBase? left, EntityBase? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(EntityBase? left, EntityBase? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// 软删除实体基类
/// </summary>
public abstract class SoftDeleteEntity : EntityBase, ISoftDelete
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTimeOffset? DeletedTime { get; set; }

    protected SoftDeleteEntity()
    {
    }

    protected SoftDeleteEntity(Guid id) : base(id)
    {
    }
}

/// <summary>
/// 完整审计实体基类（包含软删除）
/// </summary>
public abstract class FullAuditEntity : SoftDeleteEntity, IAuditable
{
    /// <summary>
    /// 创建者标识
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 修改者标识
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTimeOffset? UpdatedTime { get; set; }

    protected FullAuditEntity()
    {
    }

    protected FullAuditEntity(Guid id) : base(id)
    {
    }
}

/// <summary>
/// 审计实体基类（不含软删除）
/// </summary>
public abstract class AuditEntity : EntityBase, IAuditable
{
    /// <summary>
    /// 创建者标识
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 修改者标识
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTimeOffset? UpdatedTime { get; set; }

    protected AuditEntity()
    {
    }

    protected AuditEntity(Guid id) : base(id)
    {
    }
}