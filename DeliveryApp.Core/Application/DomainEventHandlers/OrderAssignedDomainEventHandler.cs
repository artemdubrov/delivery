using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public sealed class OrderAssignedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IMessageBusProducer _messageBusProducer;

    public OrderAssignedDomainEventHandler(IMessageBusProducer messageBusProducer)
    {
        _messageBusProducer = messageBusProducer;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _messageBusProducer.PublishOrderCreatedDomainEvent(notification, cancellationToken);
    }
}