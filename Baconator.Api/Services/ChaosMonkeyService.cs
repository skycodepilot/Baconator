using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baconator.Api.Services;

public class ChaosMonkey : BackgroundService
{
    private readonly MeatLocker _locker;
    private readonly Random _random = new();
    private readonly int ACTIVATION_COST = 7;

    public ChaosMonkey(MeatLocker locker)
    {
        _locker = locker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    Console.WriteLine("[SYSTEM] Chaos Monkey activated. Volatility engine running.");
    
    while (!stoppingToken.IsCancellationRequested)
    {
        // 150ms: Fast enough to cause collisions during a 300-order spike, 
        // slow enough to be a realistic representation of floor scale telemetry.
        await Task.Delay(150, stoppingToken);

        if (_random.Next(1, 10) > ACTIVATION_COST) 
        {
            // Minor floor corrections (spoilage, dropped box, scale recalibration)
            var variance = _random.Next(-30, 30); 
            _locker.InduceChaos(variance);
        }
    }
}
}