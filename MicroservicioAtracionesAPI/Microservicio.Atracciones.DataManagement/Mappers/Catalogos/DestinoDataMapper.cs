using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.DataManagement.Mappers.Catalogos
{
    public static class DestinoDataMapper
    {
        public static DestinoDataModel? ToDataModel(DestinoEntity? entity)
        {
            if (entity is null) return null;

            return new DestinoDataModel
            {
                DesId = entity.DesId,
                DesGuid = entity.DesGuid,
                DesNombre = entity.DesNombre,
                DesPais = entity.DesPais,
                DesImagenUrl = entity.DesImagenUrl,
                DesEstado = entity.DesEstado,
                DesFechaIngreso = entity.DesFechaIngreso,
                DesUsuarioIngreso = entity.DesUsuarioIngreso,
                DesIpIngreso = entity.DesIpIngreso,
                DesFechaMod = entity.DesFechaMod,
                DesUsuarioMod = entity.DesUsuarioMod,
                DesIpMod = entity.DesIpMod,
                DesFechaEliminacion = entity.DesFechaEliminacion,
                DesUsuarioEliminacion = entity.DesUsuarioEliminacion,
                DesIpEliminacion = entity.DesIpEliminacion
            };
        }

        public static void ApplyToEntity(DestinoDataModel model, DestinoEntity entity)
        {
            entity.DesNombre = model.DesNombre;
            entity.DesPais = model.DesPais;
            entity.DesImagenUrl = model.DesImagenUrl;
            entity.DesEstado = model.DesEstado;
            entity.DesFechaMod = DateTime.UtcNow;
            entity.DesUsuarioMod = model.DesUsuarioMod;
            entity.DesIpMod = model.DesIpMod;
        }

        public static DestinoEntity ToNewEntity(DestinoDataModel model)
        {
            return new DestinoEntity
            {
                DesGuid = Guid.NewGuid(),
                DesNombre = model.DesNombre,
                DesPais = model.DesPais,
                DesImagenUrl = model.DesImagenUrl,
                DesEstado = 'A',
                DesFechaIngreso = DateTime.UtcNow,
                DesUsuarioIngreso = model.DesUsuarioIngreso,
                DesIpIngreso = model.DesIpIngreso
            };
        }
    }
}
