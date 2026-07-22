namespace AKNexus.Framework.Domain.Events;

/// <summary>
/// 领域事件接口
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}

/// <summary>
/// 领域事件基类
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 领域事件处理器接口
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// 处理事件
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}