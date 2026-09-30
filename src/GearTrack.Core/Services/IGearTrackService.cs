using GearTrack.Core.Entities;
using GearTrack.Core.Models;

namespace GearTrack.Core.Services;

public interface IGearTrackService
{
    Task<IReadOnlyList<EquipmentItem>> BrowseEquipmentAsync(EquipmentFilter filter, CancellationToken cancellationToken = default);
    Task<EquipmentItem?> GetEquipmentAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityView>> GetEquipmentHistoryAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task<DashboardSummary> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<(OperationResult Result, Reservation? Reservation)> CreateReservationAsync(ReservationRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> CheckOutAsync(int reservationId, CancellationToken cancellationToken = default);
    Task<OperationResult> ReturnAsync(int reservationId, CancellationToken cancellationToken = default);
}
