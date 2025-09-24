using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Ports;

public interface IGeoClient
{
    /// <summary>
    ///     Получить информацию о геолокации по улице
    /// </summary>
    Task<Result<Location, Error>> GetGeolocationAsync(string street, CancellationToken cancellationToken);
}