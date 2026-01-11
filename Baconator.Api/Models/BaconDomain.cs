namespace Baconator.Api.Models;

public class PorkBatch
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string Supplier { get; set; } = string.Empty;
    public double WeightLbs { get; set;} // PorkBatch WAS a record before, and operations on WeightLbs failed - NOW this is mutable
    public DateTime ExpirationDate { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
}

public class PorkInventory
{
    public double TotalPorkLbs { get; set; }
    public int BatchCount { get; set; }
    public List<PorkBatch> Batches { get; set; } = new();
}

public record BaconOrder( // This can stay a record (records are immutable; events don't change once they happen - we'll treat orders as unchanging once received)
    Guid id,
    string Customer,
    double AmountRequested,
    DateTime CreatedAt
);