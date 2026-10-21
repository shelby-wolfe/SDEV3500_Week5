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
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GearTrackDbContext>()
            .UseSqlite(connection)
            .Options;

        int equipmentId;
        int reservationId;

        await using (var writeContext = new GearTrackDbContext(options))
        {
            await writeContext.Database.EnsureCreatedAsync();

            var equipment = new EquipmentItem
            {
                AssetTag = "LAB-03",
                Name = "Tripod",
                Category = "Video"
            };

            writeContext.Equipment.Add(equipment);
            await writeContext.SaveChangesAsync();
            equipmentId = equipment.Id;

            var service = new GearTrackService(
                new GearTrackRepository(writeContext));

            var created = await service.CreateReservationAsync(
                new ReservationRequest(
                    equipmentId,
                    "Sample Borrower",
                    "borrower@example.test",
                    "Video practice",
                    new DateTime(2030, 4, 12),
                    new DateTime(2030, 4, 13)));

            Assert.True(created.Result.Succeeded);
            reservationId = created.Reservation!.Id;

            var checkout = await service.CheckOutAsync(reservationId);
            Assert.True(checkout.Succeeded);

            var returned = await service.ReturnAsync(reservationId);
            Assert.True(returned.Succeeded);
        }

        await using (var readContext = new GearTrackDbContext(options))
        {
            var storedReservation = await readContext.Reservations
                .AsNoTracking()
                .SingleAsync(r => r.Id == reservationId);

            var storedEquipment = await readContext.Equipment
                .AsNoTracking()
                .SingleAsync(e => e.Id == equipmentId);

            var activities = await readContext.Activity
                .AsNoTracking()
                .Where(a => a.ReservationId == reservationId)
                .OrderBy(a => a.Id)
                .ToListAsync();

            Assert.Equal(ReservationStatus.Completed, storedReservation.Status);
            Assert.Equal(EquipmentStatus.Available, storedEquipment.Status);
            Assert.NotNull(storedReservation.CheckedOutAt);
            Assert.NotNull(storedReservation.ReturnedAt);

            Assert.Collection(
                activities,
                activity => Assert.Equal("Reserved", activity.Action),
                activity => Assert.Equal("Checked out", activity.Action),
                activity => Assert.Equal("Returned", activity.Action));

            Assert.All(
                activities,
                activity => Assert.Equal(equipmentId, activity.EquipmentItemId));
        }
    }
}
