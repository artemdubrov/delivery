using DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Controllers;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Courier = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Courier;
using Location = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Location;
using Order = DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Models.Order;

namespace DeliveryApp.Api.Adapters.Http;

public class DeliveryController : DefaultApiController
{
    private readonly IMediator _mediator;

    public DeliveryController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public override async Task<IActionResult> CreateOrder()
    {
        var orderId = Guid.NewGuid();
        var street = "Несуществующая";
        var createOrderCommandResult = CreateOrderCommand.Create(orderId, street,5);
        if (createOrderCommandResult.IsFailure) return BadRequest(createOrderCommandResult.Error);
        
        var response = await _mediator.Send(createOrderCommandResult.Value);
        if (response.IsSuccess) return Ok();
        return Conflict(response.Error.Message); // не делайте так в проде!
    }

    public override async Task<IActionResult> GetOrders()
    {
        // Вызываем Query
        var getActiveOrdersQuery = new GetNotCompletedOrdersQuery();
        var response = await _mediator.Send(getActiveOrdersQuery);

        // Мапим результат Query на Model
        if (response == null) return NotFound();
        var model = response.Orders.Select(o => new Order
        {
            Id = o.Id,
            Location = new Location { X = o.LocationDto.X, Y = o.LocationDto.Y }
        });
        return Ok(model);
    }

    public override async Task<IActionResult> GetCouriers()
    {
        // Вызываем Query
        var getAllCouriersQuery = new GetAllCouriersQuery();
        var response = await _mediator.Send(getAllCouriersQuery);

        // Мапим результат Query на Model
        if (response == null) return NotFound();
        var model = response.Couriers.Select(c => new Courier
        {
            Id = c.Id,
            Name = c.Name,
            Location = new Location { X = c.Location.X, Y = c.Location.Y }
        });
        return Ok(model);
    }
}