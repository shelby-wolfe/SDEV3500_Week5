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
    public async Task Create_rejects_maintenance()
    {
        var repository = new StarterRepository();
        repository.Equipment.Add(new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio", Status = EquipmentStatus.Maintenance });
        var outcome = await new GearTrackService(repository).CreateReservationAsync(new ReservationRequest(1, "Sample Borrower", "borrower@example.test", "Field recording", new DateTime(2030, 4, 12), new DateTime(2030, 4, 13)));
        Assert.False(outcome.Result.Succeeded);
    }

    [Fact]
    public async Task Create_persists_reservation_and_activity()
    {
        var repository = new StarterRepository();
        repository.Equipment.Add(new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" });
        await new GearTrackService(repository).CreateReservationAsync(new ReservationRequest(1, "Sample Borrower", "borrower@example.test", "Field recording", new DateTime(2030, 4, 12), new DateTime(2030, 4, 13)));
        Assert.Single(repository.SavedReservations);
        Assert.Single(repository.SavedActivity);
    }
}
