using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;

namespace Microservicio.Atracciones.DataManagement.Mappers.Seguridad
{
    public static class RolDataMapper
    {
        public static RolDataModel? ToDataModel(RolEntity? entity)
        {
            if (entity is null) return null;

            return new RolDataModel
            {
                RolId = entity.RolId,
                RolGuid = entity.RolGuid,
                RolDescripcion = entity.RolDescripcion,
                RolEstado = entity.RolEstado
            };
        }
    }
}
