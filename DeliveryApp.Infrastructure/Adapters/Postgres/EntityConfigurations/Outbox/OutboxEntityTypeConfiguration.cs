using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox;

internal class OutboxEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> goodConfiguration)
    {
        goodConfiguration
            .ToTable("outbox");

        goodConfiguration
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        goodConfiguration
            .Property(entity => entity.Type)
            .HasColumnName("type")
            .IsRequired();

        goodConfiguration
            .Property(entity => entity.Payload)
            .HasColumnName("content")
            .IsRequired();

        goodConfiguration
            .Property(entity => entity.OccurredAtUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        goodConfiguration
            .Property(entity => entity.ProcessedAtUtc)
            .HasColumnName("processed_on_utc")
            .IsRequired(false);
    }
}