using System.Threading.Channels;
using Baconator.Api.Models;

namespace Baconator.Api.Services;

public class BaconChannel {
    // Channel will be bound to prevent out-of-memory errors
    // (a limit pushes a "503 service unavailable" error
    // and is one way of making the client slow down;
    // maybe we'll refactor this later)
    private readonly Channel<BaconOrder> _channel = Channel.CreateBounded<BaconOrder>(new BoundedChannelOptions(100) {
        FullMode = BoundedChannelFullMode.Wait
    });

    // API calls this to drop a ticket
    public async Task AddOrderAsync(BaconOrder order, CancellationToken ct) {
        await _channel.Writer.WriteAsync(order, ct);
    }

    // Background worker calls this to pick up a ticket
    public IAsyncEnumerable<BaconOrder> ReadAllAsync(CancellationToken ct) {
        return _channel.Reader.ReadAllAsync(ct);
    }
}