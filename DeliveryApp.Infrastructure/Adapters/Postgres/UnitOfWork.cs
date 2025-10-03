using MediatR;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    private bool _disposed;

    public UnitOfWork(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        await PublishDomainEventsAsync();
        return true;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) _dbContext.Dispose();
            _disposed = true;
        }
    }

    private async Task PublishDomainEventsAsync()
    {
        // Получили агрегаты в которых есть доменные события
        var domainEntities = _dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.GetDomainEvents().Any());

        // Переложили в отдельную переменную
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.GetDomainEvents())
            .ToList();

        // Очистили Domain Event в самих агрегатах (поскольку далее они будут отправлены и больше не нужны)
        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        // Отправили в MediatR
        foreach (var domainEvent in domainEvents)
            await _mediator.Publish(domainEvent);
    }
}