# Smoke manual contrato Booking v2 — ejecutar con stack levantado (docker compose en platform/)
$Base = if ($env:SMOKE_BASE_URL) { $env:SMOKE_BASE_URL } else { "http://localhost:5050/api/v2" }
$ErrorActionPreference = "Continue"

function Test-Get($name, $url, $assertion) {
    try {
        $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30
        $json = $r.Content | ConvertFrom-Json
        $ok = & $assertion $json
        if ($ok) { Write-Host "[OK] $name" -ForegroundColor Green }
        else { Write-Host "[FAIL] $name — assertion" -ForegroundColor Red; Write-Host $r.Content.Substring(0, [Math]::Min(500, $r.Content.Length)) }
    }
    catch {
        Write-Host "[FAIL] $name — $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "Base: $Base"
Test-Get "atracciones listado" "$Base/atracciones?page=1&limit=2" {
    param($j) $j.filterStats -and $j.sorters -and $j.pagination
}
Test-Get "filtros" "$Base/atracciones/filtros" {
    param($j) $null -ne $j.data.destinationFilters
}
Test-Get "horarios tickets items" "$Base/atracciones/00000000-0000-0000-0000-000000000001/horarios/00000000-0000-0000-0000-000000000002/tickets" {
    param($j) $j.data.items -ne $null
}
Write-Host "POST reservas (invitado) y confirmacion: probar manualmente con GUIDs reales de su BD."
