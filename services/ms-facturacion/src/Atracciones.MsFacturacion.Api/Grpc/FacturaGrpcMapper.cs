using Atracciones.Contracts.Facturacion.V1;
using Atracciones.MsFacturacion.DataManagement.Models;

namespace Atracciones.MsFacturacion.Api.Grpc;

internal static class FacturaGrpcMapper
{
    public static FacturaReply ToReply(FacturaEmitidaDto f) =>
        new()
        {
            FacGuid = f.FacGuid.ToString("D"),
            FacNumero = f.FacNumero,
            RevGuid = f.RevGuid.ToString("D"),
            CliGuid = f.CliGuid.ToString("D"),
            Total = (double)f.Total,
            Moneda = f.Moneda,
            FechaEmisionUtc = f.FechaEmisionUtc.ToString("O"),
            Estado = f.Estado.ToString(),
            NombreReceptor = f.NombreReceptor,
            CorreoReceptor = f.CorreoReceptor,
            RevCodigoSnap = f.RevCodigoSnap,
        };
}
