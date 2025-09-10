using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

/// <summary>
///     Координата
/// </summary>
public class Location : ValueObject
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Location()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="x">Горизонталь</param>
    /// <param name="y">Вертикаль</param>
    private Location(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    /// <summary>
    ///     Минимально возможная координата
    /// </summary>
    public static Location MinLocation => new(1, 1);

    /// <summary>
    ///     Максимально возможная координата
    /// </summary>
    public static Location MaxLocation => new(10, 10);

    /// <summary>
    ///     Горизонталь
    /// </summary>
    public int X { get; }

    /// <summary>
    ///     Вертикаль
    /// </summary>
    public int Y { get; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="x">Горизонталь</param>
    /// <param name="y">Вертикаль</param>
    /// <returns>Результат</returns>
    public static Result<Location, Error> Create(int x, int y)
    {
        if (x < MinLocation.X || x > MaxLocation.X) return GeneralErrors.ValueIsInvalid(nameof(x));
        if (y < MinLocation.Y || y > MaxLocation.Y) return GeneralErrors.ValueIsInvalid(nameof(y));

        return new Location(x, y);
    }

    /// <summary>
    ///     Создать рандомную координату
    /// </summary>
    /// <returns>Результат</returns>
    public static Location CreateRandom()
    {
        // https://learn.microsoft.com/ru-ru/dotnet/api/system.random.next?view=net-8.0#system-random-next(system-int32)
        var rnd = new Random(Guid.NewGuid().GetHashCode());
        var x = rnd.Next(MinLocation.X, MaxLocation.X+1);
        var y = rnd.Next(MinLocation.Y, MaxLocation.Y+1);
        var location = new Location(x, y);
        return location;
    }

    /// <summary>
    ///     Рассчитать дистанцию
    /// </summary>
    /// <param name="target">Конечная координата</param>
    /// <returns>Результат</returns>
    public Result<int, Error> DistanceTo(Location target)
    {
        if (target == null) return GeneralErrors.ValueIsRequired(nameof(target));

        // Считаем разницу
        var diffX = Math.Abs(X - target.X);
        var diffY = Math.Abs(Y - target.Y);

        // Считаем дистанцию
        var distance = diffX + diffY;
        return distance;
    }

    /// <summary>
    ///     Перегрузка для определения идентичности
    /// </summary>
    /// <returns>Результат</returns>
    /// <remarks>Идентичность будет происходить по совокупности полей указанных в методе</remarks>
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}