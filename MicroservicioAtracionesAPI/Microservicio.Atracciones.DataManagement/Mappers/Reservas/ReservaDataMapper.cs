using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.DataManagement.Mappers.Reservas
{
    public static class ReservaDataMapper
    {
        public static ReservaDataModel? ToDataModel(ReservaEntity? entity)
        {
            if (entity is null) return null;

            return new ReservaDataModel
            {
                RevId = entity.RevId,
                RevGuid = entity.RevGuid,
                RevCodigo = entity.RevCodigo,
                CliId = entity.CliId,
                HorId = entity.HorId,
                RevFechaReservaUtc = entity.RevFechaReservaUtc,
                RevSubtotal = entity.RevSubtotal,
                RevValorIva = entity.RevValorIva,
                RevTotal = entity.RevTotal,
                RevOrigenCanal = entity.RevOrigenCanal,
                RevEstado = entity.RevEstado,
                RevUsuarioIngreso = entity.RevUsuarioIngreso,
                RevIpIngreso = entity.RevIpIngreso,
                RevFechaMod = entity.RevFechaMod,
                RevUsuarioMod = entity.RevUsuarioMod,
                RevIpMod = entity.RevIpMod,
                RevFechaCancelacion = entity.RevFechaCancelacion,
                RevUsuarioCancelacion = entity.RevUsuarioCancelacion,
                RevIpCancelacion = entity.RevIpCancelacion,
                RevMotivoCancelacion = entity.RevMotivoCancelacion,
                ClienteNombre = ResolverClienteNombre(entity),
                AtraccionNombre = entity.Horario?.Ticket?.Atraccion?.AtNombre ?? string.Empty,
                HorFecha = entity.Horario?.HorFecha,
                HorHoraInicio = entity.Horario?.HorHoraInicio,
                Detalle = entity.ReservasDetalle
                                             .Select(d => ToDetalleDataModel(d))
                                             .ToList()
            };
        }

        private static string ResolverClienteNombre(ReservaEntity entity)
        {
            var cliente = entity.Cliente;
            if (cliente is null) return string.Empty;

            if (!string.IsNullOrWhiteSpace(cliente.CliRazonSocial))
                return cliente.CliRazonSocial;

            var nombreCompleto = string.Join(" ", new[] { cliente.CliNombres, cliente.CliApellidos }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            if (!string.IsNullOrWhiteSpace(nombreCompleto))
                return nombreCompleto;

            return cliente.CliCorreo;
        }

        public static ReservaDetalleDataModel ToDetalleDataModel(ReservaDetalleEntity entity)
        {
            return new ReservaDetalleDataModel
            {
                RdetId = entity.RdetId,
                RdetGuid = entity.RdetGuid,
                RevId = entity.RevId,
                TckId = entity.TckId,
                RdetCantidad = entity.RdetCantidad,
                RdetPrecioUnit = entity.RdetPrecioUnit,
                RdetSubtotal = entity.RdetSubtotal,
                RdetEstado = entity.RdetEstado,
                RdetFechaIngreso = entity.RdetFechaIngreso,
                RdetUsuarioIngreso = entity.RdetUsuarioIngreso,
                RdetIpIngreso = entity.RdetIpIngreso,
                RdetFechaEliminacion = entity.RdetFechaEliminacion,
                RdetUsuarioEliminacion = entity.RdetUsuarioEliminacion,
                RdetIpEliminacion = entity.RdetIpEliminacion,
                TckTipoParticipante = entity.Ticket?.TckTipoParticipante ?? string.Empty
            };
        }

        public static ReservaEntity ToNewEntity(ReservaDataModel model)
        {
            return new ReservaEntity
            {
                RevGuid = Guid.NewGuid(),
                RevCodigo = model.RevCodigo,
                CliId = model.CliId,
                HorId = model.HorId,
                RevFechaReservaUtc = DateTime.UtcNow,
                RevSubtotal = model.RevSubtotal,
                RevValorIva = model.RevValorIva,
                RevTotal = model.RevTotal,
                RevOrigenCanal = model.RevOrigenCanal,
                RevEstado = 'A',
                RevUsuarioIngreso = model.RevUsuarioIngreso,
                RevIpIngreso = model.RevIpIngreso
            };
        }

        public static ReservaDetalleEntity ToNewDetalleEntity(ReservaDetalleDataModel model)
        {
            return new ReservaDetalleEntity
            {
                RdetGuid = Guid.NewGuid(),
                RevId = model.RevId,
                TckId = model.TckId,
                RdetCantidad = model.RdetCantidad,
                RdetPrecioUnit = model.RdetPrecioUnit,
                RdetSubtotal = model.RdetSubtotal,
                RdetEstado = 'A',
                RdetFechaIngreso = DateTime.UtcNow,
                RdetUsuarioIngreso = model.RdetUsuarioIngreso,
                RdetIpIngreso = model.RdetIpIngreso
            };
        }
    }
}
