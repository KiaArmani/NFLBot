using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace XurCollector.Services
{
    public class WorkerService
    {
        private readonly BungieService _bungieService;

        public WorkerService(IServiceProvider services)
        {
            _bungieService = services.GetRequiredService<BungieService>();
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (true)
            {
                await _bungieService.GetActivityDataOfClan();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}