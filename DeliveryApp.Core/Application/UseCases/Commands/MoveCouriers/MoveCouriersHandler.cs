using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

public class MoveCouriersHandler : IRequestHandler<MoveCouriersCommand, UnitResult<Error>>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctr
    /// </summary>
    public MoveCouriersHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository,
        ICourierRepository courierRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
    }

    public async Task<UnitResult<Error>> Handle(MoveCouriersCommand message, CancellationToken cancellationToken)
    {
        // Восстанавливаем агрегаты
        var assignedOrders = _orderRepository.GetAllInAssignedStatus().ToList();
        if (assignedOrders.Count == 0) return UnitResult.Success<Error>();

        // Изменяем агрегаты
        foreach (var order in assignedOrders)
        {
            if (order.CourierId == null)
                return GeneralErrors.ValueIsInvalid(nameof(order.CourierId));

            var getCourierResult = await _courierRepository.GetAsync((Guid)order.CourierId);
            if (getCourierResult.HasNoValue) return GeneralErrors.ValueIsInvalid(nameof(order.CourierId));
            var courier = getCourierResult.Value;

            // Перемещаем курьера
            var courierMoveResult = courier.Move(order.Location);
            if (courierMoveResult.IsFailure) return courierMoveResult;

            // Если курьер дошел до точки заказа - завершаем заказ, освобождаем курьера
            if (order.Location == courier.Location)
            {
                order.Complete();
                courier.CompleteOrder(order);
            }

            _courierRepository.Update(courier);
            _orderRepository.Update(order);
        }

        // Сохраняем
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}