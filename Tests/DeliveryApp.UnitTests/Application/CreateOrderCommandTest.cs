using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class CreateOrderCommandShould
{
    private readonly IGeoClient _geoClientMock;
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandShould()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _geoClientMock = Substitute.For<IGeoClient>();
    }

    private Maybe<Order> EmptyOrder()
    {
        return null;
    }

    private Maybe<Order> ExistedOrder()
    {
        return Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value,5).Value;
    }

    private Result<Location, Error> DefaultLocation()
    {
        return Location.Create(1, 1).Value;
    }

    [Fact]
    public async Task ReturnTrueWhenOrderExists()
    {
        //Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(ExistedOrder()));
        _geoClientMock.GetGeolocationAsync(Arg.Any<string>(), new CancellationToken())
            .Returns(Task.FromResult(DefaultLocation()));
        _unitOfWork.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        //Act
        var createCreateOrderCommandResult = CreateOrderCommand.Create(Guid.NewGuid(), "улица",5);
        createCreateOrderCommandResult.IsSuccess.Should().BeTrue();
        var handler = new CreateOrderHandler(_unitOfWork, _orderRepositoryMock, _geoClientMock);
        var result = await handler.Handle(createCreateOrderCommandResult.Value, new CancellationToken());

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnTrueWhenOrderCreatedSuccessfully()
    {
        //Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(EmptyOrder()));
        _geoClientMock.GetGeolocationAsync(Arg.Any<string>(), new CancellationToken())
            .Returns(Task.FromResult(DefaultLocation()));
        _unitOfWork.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        //Act
        var createCreateOrderCommandResult = CreateOrderCommand.Create(Guid.NewGuid(), "улица",5);
        createCreateOrderCommandResult.IsSuccess.Should().BeTrue();
        var handler = new CreateOrderHandler(_unitOfWork, _orderRepositoryMock, _geoClientMock);
        var result = await handler.Handle(createCreateOrderCommandResult.Value, new CancellationToken());

        //Assert
        _orderRepositoryMock.Received(1);
        _geoClientMock.Received(1);
        _unitOfWork.Received(1);
        result.IsSuccess.Should().BeTrue();
    }
}