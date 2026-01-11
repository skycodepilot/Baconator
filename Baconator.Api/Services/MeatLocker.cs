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
                Batches = _inventory.ToList() // POSSIBLE issue with referencing same objects in memory (i.e. a deep copy issue), but leave it alone for now...
            };
        }
    }

    // Returns "Success/Fail" and list of "receipts" showing batches where meat was sourced
    public (bool Success, List<string> Receipt) TryFillOrder(double amountNeeded)
    {
        lock (_lock) // We must lock this down to avoid anyone else trying to work with inventory while we calculate
        {
            // Fail out early if we don't have enough meat
            var validBatches = _inventory
                .Where(b => b.ExpirationDate > DateTime.UtcNow)
                .OrderBy(b => b.ExpirationDate) // FEFO Logic = oldest first
                .ToList();

            if (validBatches.Sum(b => b.WeightLbs) < amountNeeded)
            {
                return (false, new List<string> { "Insufficient Inventory" });
            }

            // Still here? Deduct inventory from batch(es)
            var receipt = new List<string>();
            double remainingNeeded = amountNeeded;

            foreach (var validBatch in validBatches)
            {
                if (remainingNeeded <= 0) break;

                // How much can we take from this batch?
                // Either take everything from the batch, or only what we need
                double amountToTake = Math.Min(validBatch.WeightLbs, remainingNeeded);

                // Update batch
                validBatch.WeightLbs -= amountToTake;
                remainingNeeded -= amountToTake;
                receipt.Add($"Took {amountToTake}lbs from Batch {validBatch.Supplier}, expires {validBatch.ExpirationDate:M/d}");

                // TODO: remove batch if it's empty (for now, just leave it at 0)
            }

            // Wrapping up
            // Remove empty batches *here* to keep list clean
            _inventory.RemoveAll(b => b.WeightLbs <= 0);

            return (true, receipt);
        }
    }
}