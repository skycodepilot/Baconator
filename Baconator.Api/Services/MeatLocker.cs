using Baconator.Api.Models;

namespace Baconator.Api.Services;

public class MeatLocker 
{
    private readonly List<PorkBatch> _inventory = new();
    private readonly object _lock = new(); // The "bouncer" for our data nightclub

    public void AddBatch(PorkBatch porkBatch) 
    {
        lock (_lock) 
        {
            _inventory.Add(porkBatch);
            Console.WriteLine($"[INVENTORY] Added {porkBatch.WeightLbs}lbs from {porkBatch.Supplier}, expires on {porkBatch.ExpirationDate:M/d}");
        }
    }

    public PorkInventory GetInventoryStatus()
    {
        lock (_lock)
        {
            return new PorkInventory
            { 
                TotalPorkLbs = _inventory.Sum(b => b.WeightLbs),
                BatchCount = _inventory.Count,
                // Create a Deep Copy (Snapshot) so the API response is thread-safe
                Batches = _inventory.Select(b => new PorkBatch 
                {
                    id = b.id,
                    Supplier = b.Supplier,
                    WeightLbs = b.WeightLbs,
                    ExpirationDate = b.ExpirationDate,
                    ReceivedDate = b.ReceivedDate
                }).ToList() 
            };
        }
    }

    // Returns "Success/Fail" and list of "receipts" showing batches where meat was sourced
    public (bool Success, List<string> Receipt) TryFillOrder(double amountNeeded)
    {
        // OPTIMISTIC READ: Snapshot data without locking
        var snapshot = _inventory
            .Where(b => b.ExpirationDate > DateTime.UtcNow)
            .OrderBy(b => b.ExpirationDate)
            .Select(b => new { b.id, b.WeightLbs, b.Version, b.Supplier })
            .ToList();

        if (snapshot.Sum(b => b.WeightLbs) < amountNeeded) {
            return (false, new List<string> { "Insufficient Inventory" });
        }

        // CALCULATE DRAFT ALLOCATION
        var receipt = new List<string>();
        double remainingNeeded = amountNeeded;
        var pendingMutations = new Dictionary<Guid, (double AmountToTake, int ReadVersion)>();

        foreach (var batch in snapshot) {
            if (remainingNeeded <= 0) break;

            double amountToTake = Math.Min(batch.WeightLbs, remainingNeeded);
            pendingMutations.Add(batch.id, (amountToTake, batch.Version));

            remainingNeeded -= amountToTake;
            receipt.Add($"Took {amountToTake}lbs from Batch {batch.Supplier}");
        }

        // 🔥 THE TRAP: Simulate network/DB latency to leave the door open for the monkey
        Thread.Sleep(50); // Just a 50ms pause before we try to commit

        // ATTEMPT COMMIT (with a version check)
        lock (_lock)
        {
            // Audit - did the chaos monkey alter any target batches?
            foreach (var mutation in pendingMutations) {
                var liveBatch = _inventory.FirstOrDefault(b => b.id == mutation.Key);

                // If batch vanished or version incremented, abort and retry
                if (liveBatch == null || liveBatch.Version != mutation.Value.ReadVersion) {
                    Console.WriteLine($"[ALERT] Concurrency conflict on Batch {mutation.Key}! Data shifted. Retrying...");
                    return TryFillOrder(amountNeeded); // Recursive retry
                }
            }

            // Apply mutations
            foreach (var mutation in pendingMutations) {
                var liveBatch = _inventory.First(b => b.id == mutation.Key);
                liveBatch.WeightLbs -= mutation.Value.AmountToTake;
                liveBatch.Version++; // Increment version on successful write
            }

            _inventory.RemoveAll(b => b.WeightLbs <= 0);
            return (true, receipt);
        }
    }

    // Method for chaos monkey
    public void InduceChaos(double variance)
    {
        lock (_lock)
        {
            if (!_inventory.Any()) return;
            
            // Grab the 10 oldest batches (the ones the API is actively trying to buy)
            var frontOfTheLine = _inventory
                .OrderBy(b => b.ExpirationDate)
                .Take(10)
                .ToList();
                
            var random = new Random();
            var target = frontOfTheLine[random.Next(frontOfTheLine.Count)];
            
            target.WeightLbs += variance; 
            target.Version++; 
            
            Console.WriteLine($"[CHAOS] Batch {target.id.ToString()[..4]} shifted by {variance}lbs. New Version: {target.Version}");
        }
    }
}