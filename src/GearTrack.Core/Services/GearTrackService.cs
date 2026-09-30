using GearTrack.Core.Entities;
using GearTrack.Core.Models;

namespace GearTrack.Core.Services;

public sealed class GearTrackService(IGearTrackRepository repository) : IGearTrackService
{
    public async Task<IReadOnlyList<EquipmentItem>> BrowseEquipmentAsync(
        EquipmentFilter filter,
        CancellationToken cancellationToken = default)
    {
        // This deliberately materializes first; the course uses it as a small performance investigation.
        IEnumerable<EquipmentItem> items = await repository.GetEquipmentAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            items = items.Where(item =>
                item.Name.Contains(term, StringComparison.Ordinal) ||
                item.AssetTag.Contains(term, StringComparison.Ordinal) ||
                (item.Manufacturer?.Contains(term, StringComparison.Ordinal) ?? false) ||
                (item.Model?.Contains(term, StringComparison.Ordinal) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            items = items.Where(item => item.Category == filter.Category);
        }

        if (filter.Status.HasValue)
        {
            items = items.Where(item => item.Status == filter.Status.Value);
        }

        return items.OrderBy(item => item.Category).ThenBy(item => item.Name).ToList();
    }

    public Task<EquipmentItem?> GetEquipmentAsync(int id, CancellationToken cancellationToken = default) =>
        repository.GetEquipmentByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default) =>
        await repository.GetReservationsAsync(cancellationToken);

    public async Task<IReadOnlyList<ActivityView>> GetEquipmentHistoryAsync(
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var activity = await repository.GetEquipmentActivityAsync(equipmentId, cancellationToken);
        var item = await repository.GetEquipmentByIdAsync(equipmentId, cancellationToken);
        return activity.Select(entry => ToView(entry, item)).ToList();
    }

    public async Task<DashboardSummary> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var equipment = await repository.GetEquipmentAsync(cancellationToken);
        var reservations = await repository.GetReservationsAsync(cancellationToken);
        var activity = await repository.GetRecentActivityAsync(8, cancellationToken);
        var recent = new List<ActivityView>();

        // Intentionally simple N+1 lookups make a safe later-semester profiling exercise.
        foreach (var entry in activity)
        {
            var item = await repository.GetEquipmentByIdAsync(entry.EquipmentItemId, cancellationToken);
            recent.Add(ToView(entry, item));
        }

        var now = DateTime.Today;
        return new DashboardSummary(
            equipment.Count,
            equipment.Count(item => item.Status == EquipmentStatus.Available),
            equipment.Count(item => item.Status == EquipmentStatus.CheckedOut),
            equipment.Count(item => item.Status == EquipmentStatus.Maintenance),
            reservations.Count(r => r.Status == ReservationStatus.Reserved && r.StartDate >= now),
            recent);
    }

    public async Task<(OperationResult Result, Reservation? Reservation)> CreateReservationAsync(
        ReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var equipment = await repository.GetEquipmentByIdAsync(request.EquipmentItemId, cancellationToken);
        if (equipment is null)
        {
            return (OperationResult.Failure("Equipment was not found."), null);
        }

        if (string.IsNullOrWhiteSpace(request.RequesterName) || string.IsNullOrWhiteSpace(request.RequesterEmail))
        {
            return (OperationResult.Failure("Requester name and email are required."), null);
        }

        if (request.EndDate.Date < request.StartDate.Date)
        {
            return (OperationResult.Failure("End date must be on or after start date."), null);
        }

        if (equipment.Status == EquipmentStatus.Maintenance)
        {
            return (OperationResult.Failure("Equipment is undergoing maintenance."), null);
        }

        // Overlap policy is outside the current reservation contract.
        var reservation = new Reservation
        {
            EquipmentItemId = request.EquipmentItemId,
            RequesterName = request.RequesterName.Trim(),
            RequesterEmail = request.RequesterEmail.Trim(),
            Purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "General course use" : request.Purpose.Trim(),
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Status = ReservationStatus.Reserved,
            CreatedAt = DateTime.UtcNow
        };

        repository.AddReservation(reservation);
        await repository.SaveChangesAsync(cancellationToken);
        repository.AddActivity(new ActivityRecord
        {
            EquipmentItemId = equipment.Id,
            ReservationId = reservation.Id,
            Action = "Reserved",
            Details = $"Reserved for {reservation.RequesterName} from {reservation.StartDate:d} to {reservation.EndDate:d}."
        });
        await repository.SaveChangesAsync(cancellationToken);
        return (OperationResult.Success("Reservation created."), reservation);
    }

    public async Task<OperationResult> CheckOutAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await repository.GetReservationByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return OperationResult.Failure("Reservation was not found.");
        }

        if (reservation.Status != ReservationStatus.Reserved)
        {
            return OperationResult.Failure("Only reserved equipment can be checked out.");
        }

        reservation.Status = ReservationStatus.CheckedOut;
        reservation.CheckedOutAt = DateTime.UtcNow;
        reservation.EquipmentItem!.Status = EquipmentStatus.CheckedOut;
        repository.AddActivity(new ActivityRecord
        {
            EquipmentItemId = reservation.EquipmentItemId,
            ReservationId = reservation.Id,
            Action = "Checked out",
            Details = $"Checked out to {reservation.RequesterName}."
        });
        await repository.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Equipment checked out.");
    }

    public async Task<OperationResult> ReturnAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await repository.GetReservationByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return OperationResult.Failure("Reservation was not found.");
        }

        if (reservation.Status != ReservationStatus.CheckedOut)
        {
            return OperationResult.Failure("Only checked-out equipment can be returned.");
        }

        reservation.Status = ReservationStatus.Completed;
        reservation.ReturnedAt = DateTime.UtcNow;
        reservation.EquipmentItem!.Status = EquipmentStatus.Available;
        repository.AddActivity(new ActivityRecord
        {
            EquipmentItemId = reservation.EquipmentItemId,
            ReservationId = reservation.Id,
            Action = "Returned",
            Details = $"Returned by {reservation.RequesterName}."
        });
        await repository.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Equipment returned.");
    }

    private static ActivityView ToView(ActivityRecord entry, EquipmentItem? item) => new(
        entry.Id,
        entry.EquipmentItemId,
        item?.Name ?? "Unknown equipment",
        item?.AssetTag ?? "Unknown",
        entry.Action,
        entry.Details,
        entry.OccurredAt);
}
