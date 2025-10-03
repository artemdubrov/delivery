using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CourierRepositoryShould : IAsyncLifetime
{
    private readonly IMediator _mediator;

    /// <summary>
    ///     Настройка Postgres из библиотеки TestContainers
    /// </summary>
    /// <remarks>По сути это Docker контейнер с Postgres</remarks>
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:14.7")
        .WithDatabase("order")
        .WithUsername("username")
        .WithPassword("secret")
        .WithCleanUp(true)
        .Build();

    private ApplicationDbContext _context;

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <remarks>Вызывается один раз перед всеми тестами в рамках этого класса</remarks>
    public CourierRepositoryShould()
    {
      _mediator = Substitute.For<IMediator>();
    }

    /// <summary>
    ///     Инициализируем окружение
    /// </summary>
    /// <remarks>Вызывается перед каждым тестом</remarks>
    public async Task InitializeAsync()
    {
        //Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
        await _postgreSqlContainer.StartAsync();

        //Накатываем миграции и справочники
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
            .Options;
        _context = new ApplicationDbContext(contextOptions);
        _context.Database.Migrate();
    }

    /// <summary>
    ///     Уничтожаем окружение
    /// </summary>
    /// <remarks>Вызывается после каждого теста</remarks>
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task CanAddCourier()
    {
        //Arrange
        var courierCreateResult = Courier.Create( "Pedestrian", 1, Location.MinLocation);
        courierCreateResult.IsSuccess.Should().BeTrue();
        var courier = courierCreateResult.Value;

        //Act
        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var getCourierResult = await courierRepository.GetAsync(courier.Id);
        getCourierResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierResult.Value;
        courier.Should().BeEquivalentTo(courierFromDb);
    }

    [Fact]
    public async Task CanUpdateCourier()
    {
        //Arrange
        var orderCreateResult = Order.Create(Guid.NewGuid(), Location.MinLocation,5);
        orderCreateResult.IsSuccess.Should().BeTrue();
        var order = orderCreateResult.Value;

        var courierCreateResult = Courier.Create( "Pedestrian", 1, Location.MinLocation);
        courierCreateResult.IsSuccess.Should().BeTrue();
        var courier = courierCreateResult.Value;

        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        //Act
        var courierStartWorkResult = courier.TakeOrder(order);
        courierStartWorkResult.IsSuccess.Should().BeTrue();
        courierRepository.Update(courier);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var getCourierResult = await courierRepository.GetAsync(courier.Id);
        getCourierResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierResult.Value;
        courier.Should().BeEquivalentTo(courierFromDb);
    }

    [Fact]
    public async Task CanGetById()
    {
        //Arrange
        var courierCreateResult = Courier.Create("Pedestrian", 1, Location.MinLocation);
        courierCreateResult.IsSuccess.Should().BeTrue();
        var courier = courierCreateResult.Value;

        //Act
        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        //Assert
        var getCourierResult = await courierRepository.GetAsync(courier.Id);
        getCourierResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierResult.Value;
        courier.Should().BeEquivalentTo(courierFromDb);
    }

    [Fact]
    public async Task CanGetAllFree()
    {
        //Arrange
        var orderCreateResult = Order.Create(Guid.NewGuid(), Location.MinLocation,5);
        orderCreateResult.IsSuccess.Should().BeTrue();
        var order = orderCreateResult.Value;
        
        var courier1CreateResult = Courier.Create("Pedestrian", 1, Location.MinLocation);
        courier1CreateResult.IsSuccess.Should().BeTrue();
        var courier1 = courier1CreateResult.Value;
        var courierTakeOrderResult =courier1.TakeOrder(order);
        courierTakeOrderResult.IsSuccess.Should().BeTrue();

        var courier2CreateResult = Courier.Create( "Pedestrian",1, Location.MinLocation);
        courier2CreateResult.IsSuccess.Should().BeTrue();
        var courier2 = courier2CreateResult.Value;

        var courierRepository = new CourierRepository(_context);
        var unitOfWork = new UnitOfWork(_context, _mediator);
        await courierRepository.AddAsync(courier1);
        await courierRepository.AddAsync(courier2);
        await unitOfWork.SaveChangesAsync();

        //Act
        var activeCouriersFromDb = courierRepository.GetAllFree();

        //Assert
        var couriersFromDb = activeCouriersFromDb.ToList();
        couriersFromDb.Should().NotBeEmpty();
        couriersFromDb.Count().Should().Be(1);
        couriersFromDb.First().Should().BeEquivalentTo(courier2);
    }
}