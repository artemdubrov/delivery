using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/// <summary>
///     Курьер
/// </summary>
public sealed class Courier : Aggregate<Guid>
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Courier()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="speed">Скорость</param>
    /// <param name="storagePlace">Место хранения</param>
    /// <param name="location">Местоположение</param>
    private Courier(string name, int speed, Location location, StoragePlace storagePlace) : this()
    {
        Id = Guid.NewGuid();
        Name = name;
        Speed = speed;
        Location = location;
        StoragePlaces.Add(storagePlace);
    }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    ///     Скорость
    /// </summary>
    public int Speed { get; private set; }

    /// <summary>
    ///     Местоположение
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    ///     Места хранения
    /// </summary>
    public List<StoragePlace> StoragePlaces { get; private set;} = new();

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="speed">Скорость</param>
    /// <param name="location">Местоположение</param>
    /// <returns>Результат</returns>
    public static Result<Courier, Error> Create(string name, int speed, Location location)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (speed<=0) return GeneralErrors.ValueIsRequired(nameof(speed));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        // Добавляем дефолтное место хранения
        var (_, isFailure, storagePlace, error) = StoragePlace.Create("Рюкзак", 10);
        if (isFailure) return error;

        return new Courier(name, speed, location, storagePlace);
    }
    
    /// <summary>
    ///     Добавить место хранения
    /// </summary>
    /// <returns>Результат</returns>
    public UnitResult<Error> AddStoragePlace(string name, int volume)
    {
        var createStoragePlaceResult = StoragePlace.Create(name, volume);
        if (createStoragePlaceResult.IsFailure) return createStoragePlaceResult.Error;
        var storagePlace = createStoragePlaceResult.Value;
        
        StoragePlaces.Add(storagePlace);
        return UnitResult.Success<Error>();
    }
    
    /// <summary>
    ///     Можно ли взять заказ?
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <returns>Результат</returns>
    public Result<bool, Error> CanTakeOrder(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        foreach (var canStoreResult in StoragePlaces.Select(storagePlace => storagePlace.CanStore(order.Volume)))
        {
            // if (canStoreResult.IsFailure) return canStoreResult.Error;
            // var canStore = canStoreResult.Value;
            // if (canStore) return true;
            if (canStoreResult.IsSuccess) return true;
        }

        return false;
    }
    
    /// <summary>
    ///     Взять заказ
    /// </summary>
    /// <returns>Результат</returns>
    public UnitResult<Error> TakeOrder(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        
        foreach (var storagePlace in StoragePlaces)
        {
            var canStoreResult = storagePlace.CanStore(order.Volume);
            // if (canStoreResult.IsFailure) return canStoreResult.Error;
            // var canStore = canStoreResult.Value;
            if (canStoreResult.IsSuccess)
            {
                var storagePlaceStoreResult = storagePlace.Store(order.Id, order.Volume);
                if (storagePlaceStoreResult.IsFailure) return storagePlaceStoreResult.Error;
                return UnitResult.Success<Error>();
            }
        }

        return Errors.ErrNoSuitableStoragePlace();
    }

    /// <summary>
    ///     Завершить заказ
    /// </summary>
    /// <returns>Результат</returns>
    public UnitResult<Error> CompleteOrder(Order order)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        var storagePlace = StoragePlaces.SingleOrDefault(c => c.OrderId == order.Id);
        if (storagePlace != null)
        {
            var storagePlaceClearResult= storagePlace.Clear(order.Id);
            if (storagePlaceClearResult.IsFailure) return storagePlaceClearResult.Error;
        }
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Рассчитать время до точки
    /// </summary>
    /// <param name="location">Конечное местоположение</param>
    /// <returns>Результат</returns>
    public Result<double, Error> CalculateTimeToLocation(Location location)
    {
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var distanceToResult = Location.DistanceTo(location);
        if (distanceToResult.IsFailure) return distanceToResult.Error;
        var distance = distanceToResult.Value;

        var time = (double)distance / Speed;
        return time;
    }
    
    /// <summary>
    ///     Изменить местоположение
    /// </summary>
    /// <param name="target">Целевое местоположение</param>
    /// <returns>Местоположение после сдвига</returns>
    public UnitResult<Error> Move(Location target)
    {
        if (target == null) return GeneralErrors.ValueIsRequired(nameof(target));

        var difX = target.X - Location.X;
        var difY = target.Y - Location.Y;
        var cruisingRange = Speed;

        var moveX = Math.Clamp(difX, -cruisingRange, cruisingRange);
        cruisingRange -= Math.Abs(moveX);

        var moveY = Math.Clamp(difY, -cruisingRange, cruisingRange);

        var locationCreateResult = Location.Create(Location.X + moveX, Location.Y + moveY);
        if (locationCreateResult.IsFailure) return locationCreateResult.Error;
        Location = locationCreateResult.Value;
        
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error ErrNoSuitableStoragePlace()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.no.suitable.storage.place",
                "Нельзя взять заказ, нет свободных мест");
        }
    }
}