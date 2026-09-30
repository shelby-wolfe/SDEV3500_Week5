using GearTrack.Core.Entities;
using GearTrack.Core.Services;

namespace GearTrack.Core.Tests;

public sealed class StarterRepository : IGearTrackRepository
{
    public List<EquipmentItem> Equipment { get; } = [];
    public List<Reservation> SavedReservations { get; } = [];
    public List<ActivityRecord> SavedActivity { get; } = [];

    public Task<List<EquipmentItem>> GetEquipmentAsync(CancellationToken cancellationToken = default) => Task.FromResult(Equipment.ToList());
    public Task<EquipmentItem?> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Equipment.SingleOrDefault(x => x.Id == id));
    public Task<List<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(SavedReservations.ToList());
    public Task<Reservation?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(SavedReservations.SingleOrDefault(x => x.Id == id));
    public Task<List<ActivityRecord>> GetRecentActivityAsync(int count, CancellationToken cancellationToken = default) => Task.FromResult(SavedActivity.OrderByDescending(x => x.OccurredAt).Take(count).ToList());
    public Task<List<ActivityRecord>> GetEquipmentActivityAsync(int equipmentId, CancellationToken cancellationToken = default) => Task.FromResult(SavedActivity.Where(x => x.EquipmentItemId == equipmentId).ToList());

    public void AddReservation(Reservation reservation)
    {
        reservation.Id = SavedReservations.Count + 1;
        reservation.EquipmentItem = Equipment.Single(x => x.Id == reservation.EquipmentItemId);
        SavedReservations.Add(reservation);
    }

    public void AddActivity(ActivityRecord activity)
    {
        activity.Id = SavedActivity.Count + 1;
        SavedActivity.Add(activity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
