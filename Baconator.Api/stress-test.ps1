# CONFIGURATION
$baseUrl = "http://localhost:5231" # <--- Check your port from the 'dotnet run' output! (*This may change every time we run it)

# ---------------------------------------------------------
# VERSE 1: Stock the Fridge
# ---------------------------------------------------------
Write-Host "🎸 [SOUND CHECK] Stocking the fridge..." -ForegroundColor Cyan

$batchId = [Guid]::NewGuid()
$inventoryPayload = @{
    id = $batchId
    supplier = "Bootsy Collins Farms"
    weightLbs = 1000
    expirationDate = (Get-Date).AddDays(30)
    receivedDate = (Get-Date)
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$baseUrl/api/inventory" -Method Post -Body $inventoryPayload -ContentType "application/json"
    Write-Host "✅ Inventory Loaded: 1000lbs of the Funk." -ForegroundColor Green
}
catch {
    Write-Error "❌ Failed to stock inventory. Is the app running?"
    exit
}

# ---------------------------------------------------------
# CHORUS: The Stress Test (50 Orders @ 10lbs each)
# ---------------------------------------------------------
Write-Host "`n🔥 [THE SOLO] Slapping the API with 50 rapid-fire orders..." -ForegroundColor Yellow

# We loop 50 times. fast.
1..50 | ForEach-Object {
    $orderId = [Guid]::NewGuid()
    
    $orderPayload = @{
        id = $orderId
        customer = "Funky Customer #$_"
        amountRequested = 10
        createdAt = (Get-Date)
    } | ConvertTo-Json

    # Send the request
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/orders" -Method Post -Body $orderPayload -ContentType "application/json" | Out-Null
        Write-Host "♪" -NoNewline -ForegroundColor Magenta # Visual feedback
    }
    catch {
        Write-Host "X" -NoNewline -ForegroundColor Red
    }
}

Write-Host "`n`n🎤 Mic Drop. Check your API logs." -ForegroundColor Cyan