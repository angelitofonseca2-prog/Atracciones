# Placeholder Fase 0: cuando existan .proto, generar stubs C# con Grpc.Tools.
# Ejemplo futuro:
#   dotnet build path/to/Atracciones.Contracts.Protos.csproj
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$protoDir = Join-Path $here ".."
$protos = Get-ChildItem -Path $protoDir -Filter "*.proto" -File -ErrorAction SilentlyContinue
if (-not $protos -or $protos.Count -eq 0) {
    Write-Host "Contracts.Protos: no hay archivos .proto; nada que generar."
    exit 0
}
Write-Warning "Añade un proyecto Grpc.Tools y ejecuta dotnet build desde ese proyecto."
exit 1
