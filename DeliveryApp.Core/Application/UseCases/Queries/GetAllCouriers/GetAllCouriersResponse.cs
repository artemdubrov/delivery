using DeliveryApp.Core.Application.UseCases.Queries.CommonDto;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class GetAllCouriersResponse
{
    public GetAllCouriersResponse(List<CourierDto> couriers)
    {
        Couriers.AddRange(couriers);
    }

    public List<CourierDto> Couriers { get; set; } = new();
}

public class CourierDto
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public LocationDto Location { get; set; }
}