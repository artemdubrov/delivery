using Confluent.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderStatusChanged;

namespace DeliveryApp.Infrastructure.Adapters.Kafka.OrderStatusChanged;

public sealed class Producer : IMessageBusProducer
{
    private readonly ProducerConfig _config;
    private readonly string _topicName;

    public Producer(IOptions<Settings> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.MessageBrokerHost))
            throw new ArgumentException(nameof(options.Value.MessageBrokerHost));
        if (string.IsNullOrWhiteSpace(options.Value.BasketConfirmedTopic))
            throw new ArgumentException(nameof(options.Value.OrderStatusChangedTopic));

        _config = new ProducerConfig
        {
            BootstrapServers = options.Value.MessageBrokerHost
        };
        _topicName = options.Value.OrderStatusChangedTopic;
    }

    public async Task PublishOrderCreatedDomainEvent(OrderCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        // Перекладываем данные из Domain Event в Integration Event
        var orderStatusChangedIntegrationEvent = new OrderCreatedIntegrationEvent()
        {
            EventId = notification.EventId.ToString(),
            OccurredAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(notification.OccurredAt),

            OrderId = notification.OrderId.ToString(),
        };

        // Создаем сообщение для Kafka
        var message = new Message<string, string>
        {
            Key = notification.EventId.ToString(),
            Value = JsonConvert.SerializeObject(orderStatusChangedIntegrationEvent)
        };

        // Отправляем сообщение в Kafka
        using var producer = new ProducerBuilder<string, string>(_config).Build();
        await producer.ProduceAsync(_topicName, message);
    }

    public async Task PublishOrderCompletedDomainEvent(OrderCompletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        // Перекладываем данные из Domain Event в Integration Event
        var orderCompletedIntegrationEvent = new OrderCompletedIntegrationEvent()
        {
            EventId = notification.EventId.ToString(),
            OccurredAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(notification.OccurredAt),
            
            OrderId = notification.OrderId.ToString(),
            CourierId = notification.CourierId.ToString()
        };

        // Создаем сообщение для Kafka
        var message = new Message<string, string>
        {
            Key = notification.OrderId.ToString(),
            Value = JsonConvert.SerializeObject(orderCompletedIntegrationEvent)
        };

        // Отправляем сообщение в Kafka
        using var producer = new ProducerBuilder<string, string>(_config).Build();
        await producer.ProduceAsync(_topicName, message);
    }
}