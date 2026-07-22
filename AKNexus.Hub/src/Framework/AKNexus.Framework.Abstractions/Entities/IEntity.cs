namespace AKNexus.Framework.Abstractions.Entities;

/// <summary>
/// 实体基础接口 - 定义实体基本行为
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// 软删除实体接口
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// 删除时间
    /// </summary>
    DateTimeOffset? DeletedTime { get; set; }
}

/// <summary>
/// 审计实体接口
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// 创建者标识
    /// </summary>
    Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTimeOffset CreatedTime { get; set; }
    
    /// <summary>
    /// 修改者标识
    /// </summary>
    Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// 修改时间
    /// </summary>
    DateTimeOffset? UpdatedTime { get; set; }
}