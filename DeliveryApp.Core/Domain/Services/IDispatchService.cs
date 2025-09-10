using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services;

/// <summary>
///     Распределяет заказы на курьеров
/// </summary>
/// <remarks>Доменный сервис</remarks>
public interface IDispatchService
{
    /// <summary>
    ///     Распределить заказ на курьера
    /// </summary>
    /// <returns>Результат</returns>
    public Result<Courier, Error> Dispatch(Order order, List<Courier> couriers);
}