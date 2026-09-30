using GearTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearTrack.Infrastructure.Data;

public sealed class GearTrackDbContext(DbContextOptions<GearTrackDbContext> options) : DbContext(options)
{
    public DbSet<EquipmentItem> Equipment => Set<EquipmentItem>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ActivityRecord> Activity => Set<ActivityRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.AssetTag).IsUnique();
            entity.Property(item => item.AssetTag).HasMaxLength(30).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Category).HasMaxLength(60).IsRequired();
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(reservation => reservation.Id);
            entity.Property(reservation => reservation.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(reservation => reservation.RequesterName).HasMaxLength(100).IsRequired();
            entity.Property(reservation => reservation.RequesterEmail).HasMaxLength(160).IsRequired();
            entity.HasOne(reservation => reservation.EquipmentItem)
                .WithMany(item => item.Reservations)
                .HasForeignKey(reservation => reservation.EquipmentItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivityRecord>(entity =>
        {
            entity.HasKey(activity => activity.Id);
            entity.Property(activity => activity.Action).HasMaxLength(60).IsRequired();
            entity.Property(activity => activity.Details).HasMaxLength(400).IsRequired();
            entity.HasOne(activity => activity.EquipmentItem)
                .WithMany(item => item.Activity)
                .HasForeignKey(activity => activity.EquipmentItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(activity => activity.Reservation)
                .WithMany()
                .HasForeignKey(activity => activity.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
