using Baconator.Api.Services;
using Baconator.Api.Workers;
using Baconator.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register thread-safe services (Singleton because they hold state)
builder.Services.AddSingleton<BaconChannel>();
builder.Services.AddSingleton<MeatLocker>();

// Register background worker (hosted service)
builder.Services.AddHostedService<BaconWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoint to peek inside the meat locker
app.MapGet("/api/inventory", (MeatLocker meatLocker) => 
{
    // Return a breakdown of what is left
    return Results.Ok(meatLocker.GetInventoryStatus());
});

// Minimal API endpoint to receive orders
app.MapPost("/api/orders", async (BaconChannel baconChannel, Baconator.Api.Models.BaconOrder baconOrder) => {
    // Fast handoff
    // (not processing here, just queueing)
    await baconChannel.AddOrderAsync(baconOrder, CancellationToken.None);
    return Results.Accepted(value: new { Status = "Queued", OrderId = baconOrder.id });

});

// Endpoint to receive inventory
app.MapPost("/api/inventory", (MeatLocker meatLocker, Baconator.Api.Models.PorkBatch porkBatch) => {
    meatLocker.AddBatch(porkBatch);
    return Results.Ok(new { Status = "Stored" });
});

app.Run();