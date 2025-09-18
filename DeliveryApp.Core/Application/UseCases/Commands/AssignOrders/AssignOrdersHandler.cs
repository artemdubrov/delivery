using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;

public class AssignOrdersHandler : IRequestHandler<AssignOrdersCommand, UnitResult<Error>>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IDispatchService _dispatchService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctr
    /// </summary>
    public AssignOrdersHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        IDispatchService dispatchService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
        _dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
    }

    public async Task<UnitResult<Error>> Handle(AssignOrdersCommand message, CancellationToken cancellationToken)
    {
        // Восстанавливаем агрегаты
        var getFirstInCreatedStatusResult = await _orderRepository.GetFirstInCreatedStatusAsync();
        if (getFirstInCreatedStatusResult.HasNoValue) return Errors.NotAvailableOrders();
        var order = getFirstInCreatedStatusResult.Value;

        var availableCouriers = _courierRepository.GetAllFree().ToList();
        if (availableCouriers.Count == 0) return Errors.NotAvailableCouriers();

        // Распределяем заказы на курьеров
        var dispatchResult = _dispatchService.Dispatch(order, availableCouriers);
        if (dispatchResult.IsFailure) return dispatchResult;
        var courier = dispatchResult.Value;

        // Сохраняем
        _courierRepository.Update(courier);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error NotAvailableOrders()
        {
            return new Error("Not Available Orders",
                "Нет заказов, для назначения");
        }

        public static Error NotAvailableCouriers()
        {
            return new Error("Not Available Couriers",
                "Нет доступных курьеров");
        }
    }
}