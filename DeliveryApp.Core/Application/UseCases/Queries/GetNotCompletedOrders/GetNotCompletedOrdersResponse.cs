using DeliveryApp.Core.Application.UseCases.Queries.CommonDto;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;

public class GetNotCompletedOrdersResponse
{
    public GetNotCompletedOrdersResponse(List<OrderDto> orders)
    {
        Orders.AddRange(orders);
    }

    public List<OrderDto> Orders { get; set; } = new();
}

public class OrderDto
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public LocationDto LocationDto { get; set; }
}