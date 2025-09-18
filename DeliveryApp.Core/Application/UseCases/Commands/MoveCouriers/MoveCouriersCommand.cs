using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;

/// <summary>
///     Передвинуть курьеров
/// </summary>
public class MoveCouriersCommand : IRequest<UnitResult<Error>>;