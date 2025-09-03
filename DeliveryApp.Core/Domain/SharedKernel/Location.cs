using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

namespace DeliveryApp.Core.Domain.SharedKernel;

public class Location : ValueObject
{
    
    /// <summary>
    /// Координата по горизонтали
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// Координата по вертикали
    /// </summary>
    public int Y { get; }

    public static Location MinLocation => new(1, 1);
    public static Location MaxLocation => new(10, 10);
    
    [ExcludeFromCodeCoverage]
    private Location()
    {}

    private Location(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    /// <summary>
    /// Создать объект с заданными координатами
    /// </summary>
    /// <param name="x">Координата по горизонтали</param>
    /// <param name="y">Координата по вертикали</param>
    public static Location Create(int x, int y)
    {
        
        if (x > MaxLocation.X || x < MinLocation.X) throw new ArgumentOutOfRangeException(nameof(x), x, "Значение должно быть в диапазоне от 1 до 10");
        if (y > MaxLocation.Y || y < MinLocation.Y) throw new ArgumentOutOfRangeException(nameof(y), y, "Значение должно быть в диапазоне от 1 до 10");

        return new Location(x, y);
    }
    
    /// <summary>
    /// Создать объект с рандомными координатами
    /// </summary>
    /// <returns></returns>
    public static Location CreateRandom()
    {
        var random = new Random();
        var randomX = random.Next(1, 11);
        var randomY = random.Next(1, 11);

        return new Location(randomX, randomY);
    }

    /// <summary>
    /// Расчитать совокупное количество шагов по X и Y
    /// </summary>
    /// <param name="location">Координата до которой следует рассчитать количество шагов</param>
    /// <returns>Количество шагов (блоков) которые следует пройти</returns>
    public int DistansceTo(Location location)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));
        if (this == location) return 0;

        var x = Math.Abs(X - location.X);
        var y = Math.Abs(Y - location.Y);

        return x + y;
    }
    
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}