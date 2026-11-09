using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(movement => movement.Id);
        builder.Property(movement => movement.Id).ValueGeneratedNever();

        builder.Property(movement => movement.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(movement => movement.Reason).HasMaxLength(StockMovement.ReasonMaxLength);

        builder.ComplexProperty(movement => movement.BalanceBefore, ConfigureBalance);
        builder.ComplexProperty(movement => movement.BalanceAfter, ConfigureBalance);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(movement => movement.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(movement => new { movement.ProductId, movement.OccurredAt });
    }

    private static void ConfigureBalance(ComplexPropertyBuilder<StockBalance> balance)
    {
        balance.Property(value => value.Physical);
        balance.Property(value => value.Reserved);
    }
}
