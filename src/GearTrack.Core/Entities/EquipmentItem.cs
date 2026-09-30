namespace GearTrack.Core.Entities;

public class EquipmentItem
{
    public int Id { get; set; }
    public required string AssetTag { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public string Location { get; set; } = "Equipment Room";
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    public DateTime PurchasedOn { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<ActivityRecord> Activity { get; set; } = new List<ActivityRecord>();
}
