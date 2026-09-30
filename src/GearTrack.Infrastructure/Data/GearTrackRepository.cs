using GearTrack.Core.Entities;
using GearTrack.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace GearTrack.Infrastructure.Data;

public sealed class GearTrackRepository(GearTrackDbContext dbContext) : IGearTrackRepository
{
    public Task<List<EquipmentItem>> GetEquipmentAsync(CancellationToken cancellationToken = default) =>
        dbContext.Equipment.AsNoTracking().ToListAsync(cancellationToken);

    public Task<EquipmentItem?> GetEquipmentByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Equipment.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<List<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default) =>
        dbContext.Reservations.AsNoTracking()
            .Include(reservation => reservation.EquipmentItem)
            .OrderByDescending(reservation => reservation.StartDate)
            .ThenByDescending(reservation => reservation.Id)
            .ToListAsync(cancellationToken);

    public Task<Reservation?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Reservations.Include(reservation => reservation.EquipmentItem)
            .FirstOrDefaultAsync(reservation => reservation.Id == id, cancellationToken);

    public Task<List<ActivityRecord>> GetRecentActivityAsync(int count, CancellationToken cancellationToken = default) =>
        dbContext.Activity.AsNoTracking()
            .OrderByDescending(activity => activity.OccurredAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public Task<List<ActivityRecord>> GetEquipmentActivityAsync(int equipmentId, CancellationToken cancellationToken = default) =>
        dbContext.Activity.AsNoTracking()
            .Where(activity => activity.EquipmentItemId == equipmentId)
            .OrderByDescending(activity => activity.OccurredAt)
            .ToListAsync(cancellationToken);

    public void AddReservation(Reservation reservation) => dbContext.Reservations.Add(reservation);

    public void AddActivity(ActivityRecord activity) => dbContext.Activity.Add(activity);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
