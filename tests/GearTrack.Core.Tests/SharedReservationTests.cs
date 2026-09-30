using GearTrack.Core.Entities;
using GearTrack.Core.Services;

namespace GearTrack.Core.Tests;

public sealed class ReservationFixture
{
    public StarterRepository Repository { get; } = new();
    public Reservation Reservation { get; }
    public GearTrackService Service { get; }

    public ReservationFixture()
    {
        Repository.Equipment.Add(new EquipmentItem { Id = 1, AssetTag = "LAB-02", Name = "Camera", Category = "Video" });
        Reservation = new Reservation { EquipmentItemId = 1, RequesterName = "Sample Borrower", RequesterEmail = "borrower@example.test", Purpose = "Video practice", StartDate = new DateTime(2030, 4, 12), EndDate = new DateTime(2030, 4, 13) };
        Repository.AddReservation(Reservation);
        Service = new GearTrackService(Repository);
    }
}

public class SharedReservationTests(ReservationFixture fixture) : IClassFixture<ReservationFixture>
{
    [Fact]
    public async Task Checkout_provides_feedback()
    {
        var result = await fixture.Service.CheckOutAsync(fixture.Reservation.Id);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public async Task Checkout_records_activity()
    {
        await fixture.Service.CheckOutAsync(fixture.Reservation.Id);
        Assert.Contains(fixture.Repository.SavedActivity, entry => entry.Action == "Checked out");
    }
}
