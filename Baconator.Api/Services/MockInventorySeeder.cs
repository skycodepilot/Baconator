using System;
using System.Collections.Generic;
using Baconator.Api.Models;

namespace Baconator.Api.Services;

public interface InventorySeeder
{
    void Seed(MeatLocker locker);
}

public class MockInventorySeeder : InventorySeeder
{
    public void Seed(MeatLocker locker)
    {
        var realisticBatches = new List<PorkBatch>();
        var random = new Random();

        // 2,000 pallets = 2 Million lbs of initial capacity
        for (int i = 0; i < 2000; i++)
        {
            realisticBatches.Add(new PorkBatch
            {
                id = Guid.NewGuid(),
                Supplier = $"Farm_{random.Next(1, 100)}", 
                WeightLbs = 1000, 
                ExpirationDate = DateTime.UtcNow.AddDays(random.Next(5, 30)), 
                ReceivedDate = DateTime.UtcNow,
                Version = 1
            });
        }

        foreach(var batch in realisticBatches)
        {
            locker.AddBatch(batch); // Adjust to your actual add method
        }
        
        Console.WriteLine($"[SYSTEM] Warehouse seeded with {realisticBatches.Count} realistic pallets.");
    }
}