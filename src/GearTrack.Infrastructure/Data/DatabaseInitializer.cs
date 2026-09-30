using GearTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearTrack.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(GearTrackDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (await dbContext.Equipment.AnyAsync(cancellationToken))
        {
            return;
        }

        var today = DateTime.Today;
        var equipment = new[]
        {
            Item("GT-LAP-001", "Dell Latitude 7450", "Laptops", "Dell", "Latitude 7450", "14-inch Windows laptop with USB-C charger.", EquipmentStatus.Available, "Cabinet A"),
            Item("GT-LAP-002", "MacBook Pro 14", "Laptops", "Apple", "MacBook Pro M4", "14-inch laptop with power adapter.", EquipmentStatus.CheckedOut, "Cabinet A"),
            Item("GT-LAP-003", "Lenovo ThinkPad T14", "Laptops", "Lenovo", "T14 Gen 5", "General-purpose instructional laptop.", EquipmentStatus.Available, "Cabinet A"),
            Item("GT-CAM-001", "Sony Mirrorless Camera", "Cameras", "Sony", "Alpha a6400", "Camera body, kit lens, battery, and case.", EquipmentStatus.Available, "Secure Shelf 1"),
            Item("GT-CAM-002", "Canon DSLR Camera", "Cameras", "Canon", "EOS 90D", "DSLR kit with 18–135 mm lens.", EquipmentStatus.Maintenance, "Repair Bench"),
            Item("GT-CAM-003", "GoPro Action Camera", "Cameras", "GoPro", "HERO12", "Action camera with protective case and mounts.", EquipmentStatus.Available, "Secure Shelf 1"),
            Item("GT-AUD-001", "Rode Wireless Microphone Kit", "Audio", "Rode", "Wireless GO II", "Two transmitters, receiver, and cables.", EquipmentStatus.Available, "Audio Drawer"),
            Item("GT-AUD-002", "Zoom Field Recorder", "Audio", "Zoom", "H6essential", "Portable 32-bit float audio recorder.", EquipmentStatus.CheckedOut, "Audio Drawer"),
            Item("GT-AUD-003", "USB Podcast Microphone", "Audio", "Audio-Technica", "ATR2100x", "Dynamic USB/XLR microphone and desk stand.", EquipmentStatus.Available, "Audio Drawer"),
            Item("GT-PRJ-001", "Epson Portable Projector", "Projectors", "Epson", "PowerLite 1781W", "Portable projector with HDMI cable and remote.", EquipmentStatus.Available, "Large Cabinet"),
            Item("GT-PRJ-002", "Short-Throw Projector", "Projectors", "BenQ", "TH671ST", "Short-throw classroom projector.", EquipmentStatus.Maintenance, "Repair Bench"),
            Item("GT-TAB-001", "iPad Air", "Tablets", "Apple", "iPad Air 11-inch", "Tablet, protective case, and USB-C cable.", EquipmentStatus.Available, "Cabinet B"),
            Item("GT-TAB-002", "Samsung Galaxy Tab", "Tablets", "Samsung", "Galaxy Tab S9 FE", "Android tablet with stylus.", EquipmentStatus.Available, "Cabinet B"),
            Item("GT-ADP-001", "USB-C Presentation Adapter", "Adapters", "Anker", "PowerExpand 5-in-1", "USB-C adapter with HDMI and USB-A.", EquipmentStatus.Available, "Adapter Bins"),
            Item("GT-ADP-002", "HDMI Capture Adapter", "Adapters", "Elgato", "Cam Link 4K", "USB HDMI capture adapter.", EquipmentStatus.Available, "Adapter Bins"),
            Item("GT-ADP-003", "Thunderbolt Dock", "Adapters", "CalDigit", "TS4", "Thunderbolt dock and power supply.", EquipmentStatus.Available, "Adapter Bins"),
            Item("GT-LGT-001", "LED Video Light Kit", "Lighting", "Neewer", "660 LED", "Two dimmable panels, stands, and cases.", EquipmentStatus.Available, "Large Cabinet"),
            Item("GT-TRI-001", "Video Tripod", "Supports", "Manfrotto", "MVK500AM", "Fluid-head tripod and bag.", EquipmentStatus.Available, "Tripod Rack")
        };
        dbContext.Equipment.AddRange(equipment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var reservations = new[]
        {
            Reservation(equipment[1], "Maya Chen", "maya.chen@example.edu", "Senior project field interview", today.AddDays(-1), today.AddDays(3), ReservationStatus.CheckedOut, today.AddDays(-4), today.AddDays(-1)),
            Reservation(equipment[7], "Jordan Patel", "jordan.patel@example.edu", "Podcast recording session", today.AddDays(-2), today.AddDays(1), ReservationStatus.CheckedOut, today.AddDays(-6), today.AddDays(-2)),
            Reservation(equipment[3], "Alex Rivera", "alex.rivera@example.edu", "Campus event photos", today.AddDays(2), today.AddDays(4), ReservationStatus.Reserved, today.AddDays(-2)),
            Reservation(equipment[9], "Sam Okafor", "sam.okafor@example.edu", "Guest lecture", today.AddDays(5), today.AddDays(5), ReservationStatus.Reserved, today.AddDays(-1)),
            Reservation(equipment[6], "Taylor Brooks", "taylor.brooks@example.edu", "Oral history interview", today.AddDays(7), today.AddDays(9), ReservationStatus.Reserved, today),
            Reservation(equipment[11], "Morgan Lee", "morgan.lee@example.edu", "Usability lab session", today.AddDays(-14), today.AddDays(-12), ReservationStatus.Completed, today.AddDays(-18), today.AddDays(-14), today.AddDays(-12)),
            Reservation(equipment[0], "Jamie Wilson", "jamie.wilson@example.edu", "Programming workshop", today.AddDays(-30), today.AddDays(-28), ReservationStatus.Completed, today.AddDays(-35), today.AddDays(-30), today.AddDays(-28)),
            Reservation(equipment[12], "Casey Nguyen", "casey.nguyen@example.edu", "Mobile UI demonstration", today.AddDays(-10), today.AddDays(-8), ReservationStatus.Completed, today.AddDays(-12), today.AddDays(-10), today.AddDays(-8))
        };
        dbContext.Reservations.AddRange(reservations);
        await dbContext.SaveChangesAsync(cancellationToken);

        var activity = new[]
        {
            Activity(reservations[2], "Reserved", "Reserved for campus event photography.", today.AddDays(-2).AddHours(15)),
            Activity(reservations[1], "Checked out", "Checked out to Jordan Patel.", today.AddDays(-2).AddHours(9)),
            Activity(reservations[0], "Checked out", "Checked out to Maya Chen.", today.AddDays(-1).AddHours(8)),
            Activity(reservations[3], "Reserved", "Reserved for a guest lecture.", today.AddDays(-1).AddHours(13)),
            Activity(reservations[4], "Reserved", "Reserved for an oral history interview.", today.AddHours(8)),
            Activity(reservations[5], "Returned", "Returned by Morgan Lee.", today.AddDays(-12).AddHours(16)),
            Activity(reservations[6], "Returned", "Returned by Jamie Wilson.", today.AddDays(-28).AddHours(17)),
            Activity(reservations[7], "Returned", "Returned by Casey Nguyen.", today.AddDays(-8).AddHours(14)),
            new ActivityRecord { EquipmentItem = equipment[4], Action = "Maintenance", Details = "Lens mount inspection requested after checkout.", OccurredAt = today.AddDays(-3).AddHours(11) },
            new ActivityRecord { EquipmentItem = equipment[10], Action = "Maintenance", Details = "Lamp-hours warning reported during setup.", OccurredAt = today.AddDays(-5).AddHours(10) }
        };
        dbContext.Activity.AddRange(activity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EquipmentItem Item(
        string assetTag,
        string name,
        string category,
        string manufacturer,
        string model,
        string description,
        EquipmentStatus status,
        string location) => new()
    {
        AssetTag = assetTag,
        Name = name,
        Category = category,
        Manufacturer = manufacturer,
        Model = model,
        Description = description,
        Status = status,
        Location = location,
        PurchasedOn = DateTime.Today.AddYears(-Random.Shared.Next(1, 5)).AddDays(-Random.Shared.Next(1, 250))
    };

    private static Reservation Reservation(
        EquipmentItem item,
        string name,
        string email,
        string purpose,
        DateTime start,
        DateTime end,
        ReservationStatus status,
        DateTime created,
        DateTime? checkedOut = null,
        DateTime? returned = null) => new()
    {
        EquipmentItem = item,
        RequesterName = name,
        RequesterEmail = email,
        Purpose = purpose,
        StartDate = start,
        EndDate = end,
        Status = status,
        CreatedAt = created,
        CheckedOutAt = checkedOut,
        ReturnedAt = returned
    };

    private static ActivityRecord Activity(Reservation reservation, string action, string details, DateTime occurredAt) => new()
    {
        EquipmentItem = reservation.EquipmentItem,
        Reservation = reservation,
        Action = action,
        Details = details,
        OccurredAt = occurredAt
    };
}
