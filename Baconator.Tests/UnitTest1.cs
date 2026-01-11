using Baconator.Api.Services;
using Baconator.Api.Models;

public class MeatLockerTests
{
    [Fact]
    public void TryFillOrder_ShouldUseOldestBatchFirst()
    {
        // Arrange
        var locker = new MeatLocker();
        locker.AddBatch(new PorkBatch { Supplier = "OLD", WeightLbs = 10, ExpirationDate = DateTime.Now.AddDays(1) });
        locker.AddBatch(new PorkBatch { Supplier = "NEW", WeightLbs = 10, ExpirationDate = DateTime.Now.AddDays(10) });

        // Act
        var result = locker.TryFillOrder(15);

        // Assert
        Assert.True(result.Success);

        // Verify
        var status = locker.GetInventoryStatus();
        
        // 1. Check Total Weight
        Assert.Equal(5, status.TotalPorkLbs);
        
        // 2. Check that the "OLD" batch is now empty (or removed, depending on your logic)
        // If your logic removes 0-weight items, Count should be 1.
        // If it keeps them at 0, Count is 2, but one has 0 weight.
        Assert.Equal(1, status.BatchCount);
        Assert.Equal("NEW", status.Batches[0].Supplier);
    }
}