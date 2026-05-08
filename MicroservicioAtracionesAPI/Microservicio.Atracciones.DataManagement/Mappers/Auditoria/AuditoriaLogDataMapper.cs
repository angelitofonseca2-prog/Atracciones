using Microservicio.Atracciones.DataAccess.Entities.Auditoria;
using Microservicio.Atracciones.DataManagement.Models.Auditoria;

namespace Microservicio.Atracciones.DataManagement.Mappers.Auditoria
{
    public static class AuditoriaLogDataMapper
    {
        public static AuditoriaLogEntity ToNewEntity(AuditoriaLogDataModel model)
        {
            return new AuditoriaLogEntity
            {
                LogGuid = Guid.NewGuid(),
                LogTabla = model.LogTabla,
                LogOperacion = model.LogOperacion,
                LogRegistroId = model.LogRegistroId,
                LogRegistroGuid = model.LogRegistroGuid,
                LogDatosAnteriores = model.LogDatosAnteriores,
                LogDatosNuevos = model.LogDatosNuevos,
                LogFechaUtc = DateTime.UtcNow,
                LogUsuario = model.LogUsuario,
                LogIp = model.LogIp,
                LogOrigenCanal = model.LogOrigenCanal
            };
        }
    }
}
