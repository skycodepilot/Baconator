# CONFIGURATION
$baseUrl = "http://localhost:5231" # <--- Check your port from the 'dotnet run' output! (*This may change every time we run it)

# ---------------------------------------------------------
# CHORUS: The Stress Test (300 Orders @ 1500 lbs each)
# ---------------------------------------------------------
Write-Host "`n🔥 [THE SOLO] Slapping the API with 300 rapid-fire orders..." -ForegroundColor Yellow

# We loop 300 times. fast.
1..300 | ForEach-Object {
    $orderId = [Guid]::NewGuid()
    
    $orderPayload = @{
        id = $orderId
        customer = "Funky Customer #$_"
        amountRequested = 1500
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