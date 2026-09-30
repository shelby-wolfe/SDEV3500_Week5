using GearTrack.Core.Entities;
using GearTrack.Core.Models;
using GearTrack.Core.Services;

namespace GearTrack.Web;

public static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapGearTrackApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api").WithTags("GearTrack API");

        api.MapGet("/dashboard", async (IGearTrackService service, CancellationToken token) =>
            Results.Ok(await service.GetDashboardAsync(token)));

        api.MapGet("/equipment", async (
            string? search,
            string? category,
            string? status,
            IGearTrackService service,
            CancellationToken token) =>
        {
            EquipmentStatus? parsedStatus = Enum.TryParse<EquipmentStatus>(status, true, out var value) ? value : null;
            var items = await service.BrowseEquipmentAsync(new EquipmentFilter(search, category, parsedStatus), token);
            return Results.Ok(items.Select(ToEquipmentResponse));
        });

        api.MapGet("/equipment/{id:int}", async (int id, IGearTrackService service, CancellationToken token) =>
        {
            var item = await service.GetEquipmentAsync(id, token);
            return item is null ? Results.NotFound() : Results.Ok(ToEquipmentResponse(item));
        });

        api.MapGet("/equipment/{id:int}/history", async (int id, IGearTrackService service, CancellationToken token) =>
            Results.Ok(await service.GetEquipmentHistoryAsync(id, token)));

        api.MapGet("/reservations", async (IGearTrackService service, CancellationToken token) =>
        {
            var reservations = await service.GetReservationsAsync(token);
            return Results.Ok(reservations.Select(r => new
            {
                r.Id,
                r.EquipmentItemId,
                EquipmentName = r.EquipmentItem?.Name,
                AssetTag = r.EquipmentItem?.AssetTag,
                r.RequesterName,
                r.RequesterEmail,
                r.Purpose,
                r.StartDate,
                r.EndDate,
                Status = r.Status.ToString(),
                r.CreatedAt,
                r.CheckedOutAt,
                r.ReturnedAt
            }));
        });

        api.MapPost("/reservations", async (ReservationRequest request, IGearTrackService service, CancellationToken token) =>
        {
            var (result, reservation) = await service.CreateReservationAsync(request, token);
            return result.Succeeded
                ? Results.Created($"/api/reservations/{reservation!.Id}", new { reservation.Id, result.Message })
                : Results.BadRequest(new { result.Message });
        });

        api.MapPost("/reservations/{id:int}/checkout", async (int id, IGearTrackService service, CancellationToken token) =>
        {
            var result = await service.CheckOutAsync(id, token);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        });

        api.MapPost("/reservations/{id:int}/return", async (int id, IGearTrackService service, CancellationToken token) =>
        {
            var result = await service.ReturnAsync(id, token);
            return result.Succeeded ? Results.Ok(result) : Results.BadRequest(result);
        });

        return endpoints;
    }

    private static object ToEquipmentResponse(EquipmentItem item) => new
    {
        item.Id,
        item.AssetTag,
        item.Name,
        item.Category,
        item.Manufacturer,
        item.Model,
        item.Description,
        item.Location,
        Status = item.Status.ToString(),
        item.PurchasedOn
    };
}
