namespace GearTrack.Core.Entities;

public class ActivityRecord
{
    public int Id { get; set; }
    public int EquipmentItemId { get; set; }
    public EquipmentItem? EquipmentItem { get; set; }
    public int? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public required string Action { get; set; }
    public required string Details { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
