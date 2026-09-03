using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FullAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
