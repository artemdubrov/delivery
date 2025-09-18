using Dapper;
using DeliveryApp.Core.Application.UseCases.Queries.CommonDto;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class GetAllCouriersHandler : IRequestHandler<GetAllCouriersQuery, GetAllCouriersResponse>
{
    private readonly string _connectionString;

    public GetAllCouriersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<GetAllCouriersResponse> Handle(GetAllCouriersQuery message,
        CancellationToken cancellationToken)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var result = await connection.QueryAsync<dynamic>(
            @"SELECT id, name, location_x, location_y FROM public.couriers"
            , new { });

        if (result.AsList().Count == 0)
            return null;

        var couriers = new List<CourierDto>();
        foreach (var item in result) couriers.Add(MapToCourierDto(item));

        return new GetAllCouriersResponse(couriers);
    }

    private CourierDto MapToCourierDto(dynamic result)
    {
        var location = new LocationDto() { X = result.location_x, Y = result.location_y };
        var courier = new CourierDto
            { Id = result.id, Name = result.name, Location = location };
        return courier;
    }
}