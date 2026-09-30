using GearTrack.Core.Entities;
using GearTrack.Core.Models;
using GearTrack.Core.Services;
using GearTrack.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GearTrack.Core.Tests;

public class PersistenceTests
{
    [Fact]
    public async Task Reservation_lifecycle_is_persisted()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<GearTrackDbContext>().UseSqlite(connection).Options;
        await using var context = new GearTrackDbContext(options);
        await context.Database.EnsureCreatedAsync();
        var equipment = new EquipmentItem { AssetTag = "LAB-03", Name = "Tripod", Category = "Video" };
        context.Equipment.Add(equipment);
        await context.SaveChangesAsync();
        var service = new GearTrackService(new GearTrackRepository(context));
        var created = await service.CreateReservationAsync(new ReservationRequest(equipment.Id, "Sample Borrower", "borrower@example.test", "Video practice", new DateTime(2030, 4, 12), new DateTime(2030, 4, 13)));
        Assert.True(created.Result.Succeeded);
        var id = created.Reservation!.Id;
        await service.CheckOutAsync(id);
        await service.ReturnAsync(id);

        var stored = await context.Reservations.FindAsync(id);
        var storedEquipment = await context.Equipment.FindAsync(equipment.Id);
        Assert.Equal(ReservationStatus.Completed, stored!.Status);
        Assert.Equal(EquipmentStatus.Available, storedEquipment!.Status);
        Assert.Equal(3, context.Activity.Local.Count);
    }
}
