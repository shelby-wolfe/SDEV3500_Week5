using GearTrack.Core.Services;
using GearTrack.Infrastructure.Data;
using GearTrack.Web;
using GearTrack.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
var defaultConnection = $"Data Source={Path.Combine(dataDirectory, "geartrack.db")}";
var connectionString = builder.Configuration.GetConnectionString("GearTrack") ?? defaultConnection;

// The provider choice is isolated here so a PostgreSQL provider can be added later
// without changing the domain, service, repository, API, or UI projects.
builder.Services.AddDbContext<GearTrackDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IGearTrackRepository, GearTrackRepository>();
builder.Services.AddScoped<IGearTrackService, GearTrackService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGearTrackApi();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GearTrackDbContext>();
    await DatabaseInitializer.InitializeAsync(dbContext);
}

app.Run();

public partial class Program;
