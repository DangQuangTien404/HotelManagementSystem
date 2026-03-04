using HotelManagementSystem.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelManagementSystem.Business
{
    public class NoShowSweepService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NoShowSweepService> _logger;
        private readonly IRoomUpdateBroadcaster _broadcaster;
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public NoShowSweepService(IServiceScopeFactory scopeFactory, ILogger<NoShowSweepService> logger, IRoomUpdateBroadcaster broadcaster)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _broadcaster = broadcaster;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SweepAsync();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task SweepAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelManagementDbContext>();

                var now = DateTime.Now;
                var yesterday = DateTime.Today.AddDays(-1);

                var candidates = await context.Reservations
                    .Include(r => r.Room)
                    .Where(r => r.Status == "Confirmed" && r.CheckInDate.Date <= yesterday)
                    .ToListAsync();

                var noShows = candidates
                    .Where(r => now >= r.CheckInDate.Date.AddDays(1).AddHours(14))
                    .ToList();

                if (noShows.Count > 0)
                {
                    foreach (var r in noShows)
                    {
                        r.Status = "NoShow";
                        if (r.Room != null)
                            r.Room.Status = "Available";
                    }
                    await context.SaveChangesAsync();
                    _logger.LogInformation("NoShow sweep: auto-marked {Count} reservation(s) as NoShow.", noShows.Count);

                    foreach (var r in noShows.Where(r => r.Room != null))
                        await _broadcaster.BroadcastRoomStatusAsync(r.Room!.Id, r.Room.RoomNumber, "Available");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automatic no-show sweep.");
            }
        }
    }
}
