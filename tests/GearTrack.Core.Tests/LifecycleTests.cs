using GearTrack.Core.Entities;
using GearTrack.Core.Services;

namespace GearTrack.Core.Tests;

public class LifecycleTests
{
    [Theory]
    [InlineData(ReservationStatus.Completed)]
    public async Task Checkout_rejects_invalid_status(ReservationStatus status)
    {
        var repository = new StarterRepository();
        var equipment = new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" };
        repository.Equipment.Add(equipment);
        var reservation = new Reservation { EquipmentItemId = 1, RequesterName = "Sample Borrower", RequesterEmail = "borrower@example.test", Purpose = "Field recording", StartDate = new DateTime(2030, 4, 12), EndDate = new DateTime(2030, 4, 13), Status = status };
        repository.AddReservation(reservation);
        var service = new GearTrackService(repository);
        var result = await service.CheckOutAsync(reservation.Id);
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData(ReservationStatus.Completed)]
    public async Task Return_rejects_invalid_status(ReservationStatus status)
    {
        var repository = new StarterRepository();
        var equipment = new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" };
        repository.Equipment.Add(equipment);
        var reservation = new Reservation { EquipmentItemId = 1, RequesterName = "Sample Borrower", RequesterEmail = "borrower@example.test", Purpose = "Field recording", StartDate = new DateTime(2030, 4, 12), EndDate = new DateTime(2030, 4, 13), Status = status };
        repository.AddReservation(reservation);
        var service = new GearTrackService(repository);
        var result = await service.ReturnAsync(reservation.Id);
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Lifecycle_rejects_missing_reservation(bool checkout)
    {
        var service = new GearTrackService(new StarterRepository());
        var result = checkout ? await service.CheckOutAsync(404) : await service.ReturnAsync(404);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Borrower_can_checkout_and_return()
    {
        var repository = new StarterRepository();
        var equipment = new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" };
        repository.Equipment.Add(equipment);
        var reservation = new Reservation { EquipmentItemId = 1, RequesterName = "Sample Borrower", RequesterEmail = "borrower@example.test", Purpose = "Field recording", StartDate = new DateTime(2030, 4, 12), EndDate = new DateTime(2030, 4, 13), Status = ReservationStatus.Reserved };
        repository.AddReservation(reservation);
        var service = new GearTrackService(repository);
        await service.CheckOutAsync(reservation.Id);
        var result = await service.ReturnAsync(reservation.Id);
        Assert.True(result.Succeeded);
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.Equal(EquipmentStatus.Available, equipment.Status);
        Assert.Equal("Equipment returned.", result.Message);
    }

    [Fact]
    public async Task Checkout_for_field_recording()
    {
        var repository = new StarterRepository();
        var equipment = new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" };
        repository.Equipment.Add(equipment);
        var reservation = new Reservation { EquipmentItemId = 1, RequesterName = "Sample Borrower", RequesterEmail = "borrower@example.test", Purpose = "Field recording", StartDate = new DateTime(2030, 4, 12), EndDate = new DateTime(2030, 4, 13), Status = ReservationStatus.Reserved };
        repository.AddReservation(reservation);
        var service = new GearTrackService(repository);
        var result = await service.CheckOutAsync(reservation.Id);
        Assert.True(result.Succeeded);
        Assert.Equal("Equipment checked out.", result.Message);
    }

    [Fact]
    public async Task Checkout_for_class_demonstration()
    {
        var repository = new StarterRepository();
        var equipment = new EquipmentItem { Id = 1, AssetTag = "LAB-01", Name = "Field recorder", Category = "Audio" };
        repository.Equipment.Add(equipment);
        var reservation = new Reservation { EquipmentItemId = 1, RequesterName = "Sample Borrower", RequesterEmail = "borrower@example.test", Purpose = "Class demonstration", StartDate = new DateTime(2030, 4, 12), EndDate = new DateTime(2030, 4, 13), Status = ReservationStatus.Reserved };
        repository.AddReservation(reservation);
        var service = new GearTrackService(repository);
        var result = await service.CheckOutAsync(reservation.Id);
        Assert.True(result.Succeeded);
        Assert.Equal("Equipment checked out.", result.Message);
    }

}
