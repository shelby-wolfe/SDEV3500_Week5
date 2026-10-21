using GearTrack.Core.Entities;
using GearTrack.Core.Services;

namespace GearTrack.Core.Tests;

public class SharedReservationTests
{
    private static (StarterRepository Repository, Reservation Reservation, GearTrackService Service)
        CreateFixture()
    {
        var repository = new StarterRepository();

        repository.Equipment.Add(new EquipmentItem
        {
            Id = 1,
            AssetTag = "LAB-02",
            Name = "Camera",
            Category = "Video"
        });

        var reservation = new Reservation
        {
            EquipmentItemId = 1,
            RequesterName = "Sample Borrower",
            RequesterEmail = "borrower@example.test",
            Purpose = "Video practice",
            StartDate = new DateTime(2030, 4, 12),
            EndDate = new DateTime(2030, 4, 13)
        };

        repository.AddReservation(reservation);

        var service = new GearTrackService(repository);

        return (repository, reservation, service);
    }

    [Fact]
    public async Task Checkout_provides_feedback()
    {
        var (_, reservation, service) = CreateFixture();

        var result = await service.CheckOutAsync(reservation.Id);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public async Task Checkout_records_activity()
    {
        var (repository, reservation, service) = CreateFixture();

        var result = await service.CheckOutAsync(reservation.Id);

        Assert.True(result.Succeeded);

        var activity = Assert.Single(repository.SavedActivity);
        Assert.Equal("Checked out", activity.Action);
        Assert.Equal(reservation.Id, activity.ReservationId);
        Assert.Equal(reservation.EquipmentItemId, activity.EquipmentItemId);
        Assert.Equal(ReservationStatus.CheckedOut, reservation.Status);
    }
}