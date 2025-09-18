using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;

/// <summary>
///     Назначить заказы на курьеров
/// </summary>
public class AssignOrdersCommand : IRequest<UnitResult<Error>>;