using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.TotalAmount)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.ShippingTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingDistrict)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingFullAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
