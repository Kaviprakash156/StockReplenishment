using Microsoft.EntityFrameworkCore;
using StockReplenishment.Domain.Entities;
using System.Security.Cryptography.X509Certificates;

namespace StockReplenishment.Infrastructure.Data;

public class StockReplenishmentDbContext : DbContext
{
    public StockReplenishmentDbContext(
        DbContextOptions<StockReplenishmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<StockLocation> StockLocations => Set<StockLocation>();

    public DbSet<ReplenishmentRequest> ReplenishmentRequests =>
        Set<ReplenishmentRequest>();

    public DbSet<ReplenishmentRequestItem> ReplenishmentRequestItems =>
        Set<ReplenishmentRequestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockLocation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

        });

        modelBuilder.Entity<ReplenishmentRequest>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Priority)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            entity.Property(x => x.StockValidationStatus)
                .IsRequired();

            entity.Property(x => x.StockValidationMessage)
                .HasMaxLength(500);

            entity.HasOne(x => x.StockLocation)
                .WithMany(x => x.ReplenishmentRequests)
                .HasForeignKey(x => x.StockLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.ReplenishmentRequest)
                .HasForeignKey(x => x.ReplenishmentRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReplenishmentRequestItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ArticleNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.RequestedQuantity)
                .IsRequired();

            entity.Property(x => x.FulfilledQuantity)
                .IsRequired();
        });
    }
}