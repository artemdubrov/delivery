using Dapper;
using DeliveryApp.Core.Application.UseCases.Queries.CommonDto;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;

public class
    GetNotCompletedOrdersHandler : IRequestHandler<GetNotCompletedOrdersQuery,
    GetNotCompletedOrdersResponse>
{
    private readonly string _connectionString;

    public GetNotCompletedOrdersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<GetNotCompletedOrdersResponse> Handle(GetNotCompletedOrdersQuery message,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var result = await connection.QueryAsync<dynamic>(
            @"SELECT id, courier_id, location_x, location_y FROM public.orders where status!=@status;"
            , new { status = OrderStatus.Completed.Name });

        if (result.AsList().Count == 0)
            return null;

        var orders = new List<OrderDto>();
        foreach (var item in result) orders.Add(MapToOrderDto(item));

        return new GetNotCompletedOrdersResponse(orders);
    }

    private OrderDto MapToOrderDto(dynamic result)
    {
        var location = new LocationDto { X = result.location_x, Y = result.location_y };
        var order = new OrderDto { Id = result.id, LocationDto = location };
        return order;
    }
}