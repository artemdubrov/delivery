using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class StoragePlace : Entity<Guid>
{
    private StoragePlace()
    { }


    public string Name { get; }
    public int TotalVolume { get; }
    public Guid? OrderId { get; private set; }

    private StoragePlace(string name, int volume) : this()
    {
        Id = Guid.NewGuid();
        Name = name;
        TotalVolume = volume;
    }

    public static Result<StoragePlace, Error> Create(string name, int volume)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsInvalid("Provie correct name of a storage");
        if (volume <= 0) return Errors.VolumeMustBeMoreThanZero();

        return new StoragePlace(name, volume);
    }

    public UnitResult<Error> CanStore(int volume)
    {
        if (volume <= 0) return Errors.VolumeMustBeMoreThanZero();
        if (volume > TotalVolume) return Errors.OrderVolumeExceedStorageVolume();
        if (IsOccupied()) return Errors.StorageIsOccupied();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Store(Guid orderId, int volume)
    {
        var res = CanStore(volume);
        if (res.IsFailure)
            return res;

        OrderId = orderId;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Clear(Guid orderId)
    {
        if (OrderId != orderId) GeneralErrors.ValueIsInvalid("This order is not in storage");

        OrderId = null;
        return UnitResult.Success<Error>();
    }

    private bool IsOccupied() => OrderId.HasValue;


    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    public static class Errors
    {
        public static Error VolumeMustBeMoreThanZero()
        {
            return new Error($"volume.is.zero.or.less",
                $"Volume must be more than zero");
        }
        
        public static Error OrderVolumeExceedStorageVolume()
        {
            return new Error($"volume.is.wrong",
                $"Order volume exceed storage volume");
        }

        public static Error StorageIsOccupied()
        {
            return new Error($"Storage.is.occupied",
                $"Storage is occupied");
        }
    }
}