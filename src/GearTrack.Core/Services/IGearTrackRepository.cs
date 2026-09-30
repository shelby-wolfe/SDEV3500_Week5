using GearTrack.Core.Entities;

namespace GearTrack.Core.Services;

public interface IGearTrackRepository
{
    Task<List<EquipmentItem>> GetEquipmentAsync(CancellationToken cancellationToken = default);
    Task<EquipmentItem?> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default);
    Task<Reservation?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ActivityRecord>> GetRecentActivityAsync(int count, CancellationToken cancellationToken = default);
    Task<List<ActivityRecord>> GetEquipmentActivityAsync(int equipmentId, CancellationToken cancellationToken = default);
    void AddReservation(Reservation reservation);
    void AddActivity(ActivityRecord activity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
