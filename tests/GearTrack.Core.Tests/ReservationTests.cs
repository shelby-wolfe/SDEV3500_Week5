using GearTrack.Core.Entities;
using GearTrack.Core.Models;
using GearTrack.Core.Services;

namespace GearTrack.Core.Tests;



public class ReservationTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public async Task Create_handles_date_boundaries(int endOffset, bool expectedSucceeded)
    {
        var repository = new StarterRepository();
        repository.Equipment.Add(new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" });
        var start = new DateTime(2030, 4, 12);
        var end = start.AddDays(endOffset);
        var request = new ReservationRequest(1, "Sample Borrower", "borrower@example.test", "Field recording", start, end);
        var outcome = await new GearTrackService(repository).CreateReservationAsync(request);

        Assert.Equal(expectedSucceeded, outcome.Result.Succeeded);

        if (expectedSucceeded)
        {
            var reservation = Assert.Single(repository.SavedReservations);
            Assert.Equal(start.Date, reservation.StartDate.Date);
            Assert.Equal(end.Date, reservation.EndDate.Date);
            Assert.Equal(ReservationStatus.Reserved, reservation.Status);
            Assert.Single(repository.SavedActivity);
        }
        else
        {
            Assert.Empty(repository.SavedReservations);
            Assert.Empty(repository.SavedActivity);
        }
    }

    [Fact]
    public async Task Create_rejects_maintenance_without_saving()
    {
        var repository = new StarterRepository();
        var equipment = new EquipmentItem
        {
            Id = 1,
            AssetTag = "LAB-01",
            Name = "Field recorder",
            Category = "Audio",
            Status = EquipmentStatus.Maintenance
        };

        repository.Equipment.Add(equipment);

        var outcome = await new GearTrackService(repository)
            .CreateReservationAsync(
                new ReservationRequest(
                    1,
                    "Sample Borrower",
                    "borrower@example.test",
                    "Field recording",
                    new DateTime(2030, 4, 12),
                    new DateTime(2030, 4, 13)));

        Assert.False(outcome.Result.Succeeded);
        Assert.Null(outcome.Reservation);
        Assert.Empty(repository.SavedReservations);
        Assert.Empty(repository.SavedActivity);
        Assert.Equal(0, repository.SaveChangesCallCount);
        Assert.Equal(EquipmentStatus.Maintenance, equipment.Status);
    }

    [Fact]
    public async Task Create_persists_reservation_and_activity()
    {
        var repository = new StarterRepository();

        var equipment = new EquipmentItem
        {
            Id = 1,
            AssetTag = "LAB-01",
            Name = "Field recorder",
            Category = "Audio"
        };

        repository.Equipment.Add(equipment);

        var outcome = await new GearTrackService(repository)
            .CreateReservationAsync(
                new ReservationRequest(
                    1,
                    "  Sample Borrower  ",
                    "  borrower@example.test  ",
                    "  Field recording  ",
                    new DateTime(2030, 4, 12, 15, 30, 0),
                    new DateTime(2030, 4, 13, 18, 0, 0)));

        Assert.True(outcome.Result.Succeeded);

        var reservation = Assert.Single(repository.SavedReservations);
        Assert.Equal("Sample Borrower", reservation.RequesterName);
        Assert.Equal("borrower@example.test", reservation.RequesterEmail);
        Assert.Equal("Field recording", reservation.Purpose);
        Assert.Equal(new DateTime(2030, 4, 12), reservation.StartDate);
        Assert.Equal(new DateTime(2030, 4, 13), reservation.EndDate);
        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
        Assert.Equal(EquipmentStatus.Available, equipment.Status);

        var activity = Assert.Single(repository.SavedActivity);
        Assert.Equal("Reserved", activity.Action);
        Assert.Equal(reservation.Id, activity.ReservationId);
        Assert.Equal(equipment.Id, activity.EquipmentItemId);

        Assert.Equal(2, repository.SaveChangesCallCount);
    }
}
