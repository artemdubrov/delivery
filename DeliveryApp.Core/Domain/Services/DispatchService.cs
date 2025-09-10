using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services;

/// <summary>
///     Распределяет заказы на курьеров
/// </summary>
/// <remarks>Доменный сервис</remarks>
public class DispatchService : IDispatchService
{
    /// <summary>
    ///     Распределить заказ на курьера
    /// </summary>
    /// <returns>Результат</returns>
    public Result<Courier, Error> Dispatch(Order order, List<Courier> couriers)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (couriers == null || couriers.Count == 0) return GeneralErrors.InvalidLength(nameof(couriers));

        // Ищем курьера, который быстрее всего доставит заказ
        var minTimeToLocation = double.MaxValue;
        Courier fastestCourier = null;
        
        // Отбираем только тех, кто потенциально может взять заказ
        var availableCouriers = couriers.Where(x => x.CanTakeOrder(order).Value);
        foreach (var courier in availableCouriers)
        {
            var courierCalculateTimeToLocationResult = courier.CalculateTimeToLocation(order.Location);
            if (courierCalculateTimeToLocationResult.IsFailure) return courierCalculateTimeToLocationResult.Error;
            var timeToLocation = courierCalculateTimeToLocationResult.Value;

            if (timeToLocation < minTimeToLocation)
            {
                minTimeToLocation = timeToLocation;
                fastestCourier = courier;
            }
        }

        // Если подходящий курьер не был найден, возвращаем ошибку
        if (fastestCourier == null) return Errors.SuitableCourierWasNotFound();

        // Если курьер найден - назначаем заказ на курьера
        var orderAssignToCourierResult = order.Assign(fastestCourier);
        if (orderAssignToCourierResult.IsFailure) return orderAssignToCourierResult.Error;

        // Делаем курьера занятым
        var courierSetBusyResult = fastestCourier.TakeOrder(order);
        if (courierSetBusyResult.IsFailure) return orderAssignToCourierResult.Error;

        return fastestCourier;
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сервис
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error SuitableCourierWasNotFound()
        {
            return new Error("suitable.courier.was.not.found",
                "Подходящий курьер не был найден");
        }
    }
}