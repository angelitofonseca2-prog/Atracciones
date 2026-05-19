# Regenera Endpoints-Booking-Atracciones.docx desde el markdown (Open XML, no requiere Word).
$ErrorActionPreference = 'Stop'
$md = Join-Path $PSScriptRoot 'Endpoints-Booking-Atracciones.md'
$docx = Join-Path $PSScriptRoot 'Endpoints-Booking-Atracciones.docx'
$proj = Join-Path $PSScriptRoot '_docgen\DocGen.csproj'
dotnet run --project $proj -c Release -- $md $docx
Write-Host "Generado: $docx"
