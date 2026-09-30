namespace GearTrack.Core.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int EquipmentItemId { get; set; }
    public EquipmentItem? EquipmentItem { get; set; }
    public required string RequesterName { get; set; }
    public required string RequesterEmail { get; set; }
    public required string Purpose { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Reserved;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
}
