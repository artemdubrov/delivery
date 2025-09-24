using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    private readonly IGeoClient _geoClient;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctr
    /// </summary>
    public CreateOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IGeoClient geoClient)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _geoClient = geoClient ?? throw new ArgumentNullException(nameof(geoClient));
    }

    public async Task<UnitResult<Error>> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
    {
        // Проверяем нет ли уже такого заказа
        var getOrderResult = await _orderRepository.GetAsync(message.OrderId);
        if (getOrderResult.HasValue) return UnitResult.Success<Error>();

        // Получили геопозицию из Geo. Пока ставим рандом значение
        var getGeolocationResult = await _geoClient.GetGeolocationAsync(message.Street, cancellationToken);
        if (getGeolocationResult.IsFailure) return getGeolocationResult;

        var location = getGeolocationResult.Value;

        // Создаем заказ
        var orderCreateResult = Order.Create(message.OrderId, location, message.Volume);
        if (orderCreateResult.IsFailure) return orderCreateResult;
        var order = orderCreateResult.Value;

        // Сохраняем
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}