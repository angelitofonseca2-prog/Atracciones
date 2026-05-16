using Atracciones.Contracts.Inventario.V1;
using Atracciones.MsAtracciones.Business.Services;
using Grpc.Core;

namespace Atracciones.MsAtracciones.Api.Grpc;

public sealed class InventarioGrpcService : AtraccionInventarioService.AtraccionInventarioServiceBase
{
    private readonly IInventarioCupoAppService _cupos;

    public InventarioGrpcService(IInventarioCupoAppService cupos) => _cupos = cupos;

    public override async Task<GetTicketPrecioResponse> GetTicketPrecio(
        GetTicketPrecioRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.TckGuid, out var tckGuid))
        {
            return new GetTicketPrecioResponse { Ok = false, Mensaje = "tck_guid inválido." };
        }

        var (ok, msg, precio, tipo, atGuid) = await _cupos.GetTicketPrecioAsync(tckGuid, context.CancellationToken);
        return new GetTicketPrecioResponse
        {
            Ok = ok,
            Mensaje = msg,
            Precio = precio,
            TipoParticipante = tipo,
            AtGuid = atGuid,
        };
    }

    public override async Task<ObtenerHorarioParaReservaResponse> ObtenerHorarioParaReserva(
        ObtenerHorarioParaReservaRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.HorGuid, out var horGuid) || !Guid.TryParse(request.AtGuid, out var atGuid))
        {
            return new ObtenerHorarioParaReservaResponse { Ok = false, Mensaje = "hor_guid o at_guid inválido." };
        }

        var (ok, msg, nombre, fecha, ini, fin, tckGuid) = await _cupos.ObtenerHorarioParaReservaAsync(horGuid, atGuid, context.CancellationToken);
        return new ObtenerHorarioParaReservaResponse
        {
            Ok = ok,
            Mensaje = msg,
            AtraccionNombre = nombre,
            HorFecha = fecha,
            HorHoraInicio = ini,
            HorHoraFin = fin,
            TckGuid = tckGuid,
        };
    }

    public override async Task<CupoOperacionResponse> ValidarYReservarCupo(
        ValidarYReservarCupoRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.HorGuid, out var horGuid))
        {
            return new CupoOperacionResponse { Ok = false, Mensaje = "hor_guid inválido.", CuposDisponiblesTrasOperacion = 0 };
        }

        var (ok, msg, cupos) = await _cupos.ValidarYReservarAsync(horGuid, request.CantidadPersonas, context.CancellationToken);
        return new CupoOperacionResponse
        {
            Ok = ok,
            Mensaje = msg,
            CuposDisponiblesTrasOperacion = cupos,
        };
    }

    public override async Task<CupoOperacionResponse> LiberarCupo(
        LiberarCupoRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.HorGuid, out var horGuid))
        {
            return new CupoOperacionResponse { Ok = false, Mensaje = "hor_guid inválido.", CuposDisponiblesTrasOperacion = 0 };
        }

        var (ok, msg, cupos) = await _cupos.LiberarAsync(horGuid, request.CantidadPersonas, context.CancellationToken);
        return new CupoOperacionResponse
        {
            Ok = ok,
            Mensaje = msg,
            CuposDisponiblesTrasOperacion = cupos,
        };
    }
}
