using Baconator.Api.Models;
using Baconator.Api.Services;

namespace Baconator.Api.Workers;

public class BaconWorker : BackgroundService 
{
    private readonly BaconChannel _baconChannel;
    private readonly MeatLocker _meatLocker;
    private readonly ILogger<BaconWorker> _logger;

    public BaconWorker(BaconChannel baconChannel, MeatLocker meatLocker, ILogger<BaconWorker> logger) 
    {
        _baconChannel = baconChannel;
        _meatLocker = meatLocker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    {
        _logger.LogInformation("--- BUTCHER IS ON SHIFT ---");

        await foreach (var order in _baconChannel.ReadAllAsync(stoppingToken)) 
        {
            try 
            {
                _logger.LogInformation($"[PROCESSING] Order for {order.Customer}: needs {order.AmountRequested}lbs");

                // SIMULATE REALITY: A database write usually takes 50-100ms
                // (THIS IS NOT NECESSARY and more-clearly illustrates performance-related scenarios by slowing things down)
                await Task.Delay(100, stoppingToken);

                var result = _meatLocker.TryFillOrder(order.AmountRequested); // Don't need to await (TryFillOrder() is synchronous with in-memory lock)
                if (result.Success)
                {
                    _logger.LogInformation($"[FILLED] Order {order.Customer} COMPLETE");
                    foreach (var line in result.Receipt)
                    {
                        _logger.LogInformation($"   -> {line}");
                    }
                }
                else
                {
                    _logger.LogWarning($"[FAILED] Order {order.Customer}: Insufficient Inventory");
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error processing order");
            }
        }
    }
}