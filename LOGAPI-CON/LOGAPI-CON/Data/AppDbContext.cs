using LOGAPI_CON.Models;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DeliveryOrder> DeliveryOrders => Set<DeliveryOrder>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<VehicleHistory> VehicleHistories => Set<VehicleHistory>();
    public DbSet<Load> Loads => Set<Load>();
    public DbSet<LoadOrder> LoadOrders => Set<LoadOrder>();
    public DbSet<PartialDelivery> PartialDeliveries => Set<PartialDelivery>();
    public DbSet<DeliveryEvidence> DeliveryEvidences => Set<DeliveryEvidence>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DeliveryOrder>(entity =>
        {
            entity.ToTable("DELIVERYORDER");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Customer).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Weight).HasPrecision(12, 2);
            entity.Property(x => x.DeliveryAddress).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Sector).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ScheduledDate);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.IsPriority).IsRequired();
            entity.Property(x => x.PriorityReason).HasMaxLength(500).IsRequired(false);
            entity.Property(x => x.ExternalFolio).HasMaxLength(100).IsRequired(false);

            entity.HasIndex(x => x.ScheduledDate);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ExternalFolio).IsUnique().HasFilter("[ExternalFolio] IS NOT NULL");
        });


        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("VEHICLES");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Capacity).HasPrecision(12, 2);
            entity.Property(x => x.Brand).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(150).IsRequired();
            entity.Property(x => x.LicensePlate).HasMaxLength(50).IsRequired();

            entity.HasIndex(x => x.IsAvailable);
        });


        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("ORDERSTATUSHISTORIES");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderId).IsRequired();
            entity.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.ChangedByUserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ChangedAt).IsRequired();
            entity.Property(x => x.TicketReference).HasMaxLength(100).IsRequired(false);
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();

            entity.HasOne<DeliveryOrder>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.ChangedAt);
        });


        modelBuilder.Entity<VehicleHistory>(entity =>
        {
            entity.ToTable("VEHICLEHISTORY");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PropertyName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OldValue).HasMaxLength(500).IsRequired(false);
            entity.Property(x => x.NewValue).HasMaxLength(500).IsRequired(false);
            entity.Property(x => x.ChangeByUser).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ChangedAt).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);

            entity.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.VehicleId);
            entity.HasIndex(x => x.ChangedAt);
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.ToTable("AUTHUSERS");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<Load>(entity =>
        {
            entity.ToTable("LOADS");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RouteDate).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.TotalWeight).HasPrecision(12, 2).IsRequired();
            entity.Property(x => x.CreatedByUserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();

            entity.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {x.VehicleId, x.RouteDate});
        });

        modelBuilder.Entity<LoadOrder>(entity =>
        {
            entity.ToTable("LOADORDERS");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssignedWeight).HasPrecision(12, 2).IsRequired();
            entity.Property(x => x.Sequence).IsRequired();
            entity.HasOne(x => x.Load).WithMany(x => x.Orders).HasForeignKey(x => x.LoadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<DeliveryOrder>().WithMany().HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            { x.LoadId, x.OrderId }).IsUnique();
        });


        modelBuilder.Entity<PartialDelivery>(entity =>
        {
            entity.ToTable("PARTIALDELIVERIES");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderedWeight).HasPrecision(12, 2).IsRequired();
            entity.Property(x => x.DeliveredWeight).HasPrecision(12, 2).IsRequired();
            entity.Property(x => x.PendingWeight).HasPrecision(12, 2).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Comments).HasMaxLength(1000).IsRequired(false);
            entity.Property(x => x.RegisteredByUserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RegisteredAtUtc).IsRequired();

            entity.HasOne<DeliveryOrder>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.OrderId);
        });

        modelBuilder.Entity<DeliveryEvidence>(entity =>
        {
            entity.ToTable("DELIVERYEVIDENCES");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EvidenceType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FileSize).IsRequired();
            entity.Property(x => x.Comments).HasMaxLength(1000).IsRequired(false);
            entity.Property(x => x.UploadedByUserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UploadedAtUtc).IsRequired();

            entity.HasOne<DeliveryOrder>().WithMany().HasForeignKey(x => x.OrderId).
            OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.OrderId);
        });
    }

}