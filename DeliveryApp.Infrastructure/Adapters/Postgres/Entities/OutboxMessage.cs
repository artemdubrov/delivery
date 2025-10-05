#nullable enable
namespace DeliveryApp.Infrastructure.Adapters.Postgres.Entities;

/// <summary>
///     Outbox
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    ///     Уникальный идентификатор сообщения
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Тип сообщения
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Тело сообщения (полезная информация)
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    ///     Дата создания
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    ///     Дата публикации
    /// </summary>
    public DateTime? ProcessedAtUtc { get; set; }
}