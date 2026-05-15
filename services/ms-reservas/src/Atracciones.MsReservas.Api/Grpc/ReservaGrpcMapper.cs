using Atracciones.Contracts.Reservas.V1;
using Atracciones.MsReservas.DataManagement.Models;

namespace Atracciones.MsReservas.Api.Grpc;

internal static class ReservaGrpcMapper
{
    public static ReservaReply ToReply(ReservaDetalladaDto r)
    {
        var reply = new ReservaReply
        {
            RevGuid = r.RevGuid.ToString("D"),
            RevCodigo = r.RevCodigo,
            CliGuid = r.CliGuid.ToString("D"),
            AtGuid = r.AtGuid.ToString("D"),
            HorGuid = r.HorGuid.ToString("D"),
            Estado = r.Estado.ToString(),
            Subtotal = (double)r.Subtotal,
            ValorIva = (double)r.ValorIva,
            Total = (double)r.Total,
            Moneda = r.Moneda,
            OrigenCanal = r.OrigenCanal ?? "",
            RevFechaReservaUtc = r.RevFechaReservaUtc.ToString("O"),
            AtraccionNombreSnap = r.AtraccionNombreSnap,
            HorFechaSnap = r.HorFechaSnap,
            HorHoraInicioSnap = r.HorHoraInicioSnap,
            HorHoraFinSnap = r.HorHoraFinSnap,
        };
        foreach (var d in r.Detalle)
        {
            reply.Detalle.Add(new ReservaDetalleReply
            {
                TckGuid = d.TckGuid.ToString("D"),
                Cantidad = d.Cantidad,
                PrecioUnit = (double)d.PrecioUnit,
                SubtotalLinea = (double)d.SubtotalLinea,
                TipoParticipante = d.TipoParticipante,
            });
        }

        return reply;
    }
}
