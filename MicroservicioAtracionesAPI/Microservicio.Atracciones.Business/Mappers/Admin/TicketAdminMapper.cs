using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System.Globalization;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class TicketAdminMapper
    {
        public static TicketResponse ToResponse(TicketDataModel model, string atraccionNombre)
            => new()
            {
                TckGuid = model.TckGuid.ToString(),
                AtraccionGuid = model.AtGuid == Guid.Empty ? string.Empty : model.AtGuid.ToString(),
                AtraccionNombre = string.IsNullOrWhiteSpace(atraccionNombre) ? model.AtNombre : atraccionNombre,
                Titulo = model.TckTitulo,
                Precio = model.TckPrecio,
                TipoParticipante = model.TckTipoParticipante,
                CapacidadMaxima = model.TckCapacidadMaxima,
                CuposDisponibles = model.TckCuposDisponibles,
                Estado = model.TckEstado,
                FechaIngreso = model.TckFechaIngreso,
                Horarios = model.Horarios.Select(ToHorarioResponse).ToList()
            };

        public static HorarioResponse ToHorarioResponse(HorarioDataModel horario)
            => new()
            {
                HorGuid = horario.HorGuid.ToString(),
                TckGuid = horario.TckGuid == Guid.Empty ? string.Empty : horario.TckGuid.ToString(),
                AtraccionGuid = horario.AtGuid == Guid.Empty ? string.Empty : horario.AtGuid.ToString(),
                AtraccionNombre = horario.AtNombre,
                TicketTitulo = horario.TckTitulo,
                Fecha = horario.HorFecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                HoraInicio = horario.HorHoraInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
                HoraFin = horario.HorHoraFin?.ToString("HH:mm", CultureInfo.InvariantCulture),
                CapacidadMaxima = horario.TckCapacidadMaxima,
                CuposDisponibles = horario.HorCuposDisponibles,
                Estado = horario.HorEstado,
                FechaIngreso = horario.HorFechaIngreso
            };
    }
}
