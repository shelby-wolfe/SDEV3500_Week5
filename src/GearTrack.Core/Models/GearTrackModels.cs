using GearTrack.Core.Entities;

namespace GearTrack.Core.Models;

public sealed record ReservationRequest(
    int EquipmentItemId,
    string RequesterName,
    string RequesterEmail,
    string Purpose,
    DateTime StartDate,
    DateTime EndDate);

public sealed record DashboardSummary(
    int TotalEquipment,
    int Available,
    int CheckedOut,
    int Maintenance,
    int UpcomingReservations,
    IReadOnlyList<ActivityView> RecentActivity);

public sealed record ActivityView(
    int Id,
    int EquipmentItemId,
    string EquipmentName,
    string AssetTag,
    string Action,
    string Details,
    DateTime OccurredAt);

public sealed record OperationResult(bool Succeeded, string Message)
{
    public static OperationResult Success(string message) => new(true, message);
    public static OperationResult Failure(string message) => new(false, message);
}

public sealed record EquipmentFilter(string? Search, string? Category, EquipmentStatus? Status);
