using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.DataManagement.Mappers.Reservas
{
    public static class ReseniaDataMapper
    {
        public static ReseniaDataModel? ToDataModel(ReseniaEntity? entity)
        {
            if (entity is null) return null;

            return new ReseniaDataModel
            {
                RsnId = entity.RsnId,
                RsnGuid = entity.RsnGuid,
                AtId = entity.AtId,
                RevId = entity.RevId,
                RsnComentario = entity.RsnComentario,
                RsnRating = entity.RsnRating,
                RsnEstado = entity.RsnEstado,
                RsnFechaCreacion = entity.RsnFechaCreacion,
                RsnUsuarioCreacion = entity.RsnUsuarioCreacion,
                RsnIpCreacion = entity.RsnIpCreacion,
                RsnFechaMod = entity.RsnFechaMod,
                RsnUsuarioMod = entity.RsnUsuarioMod,
                RsnIpMod = entity.RsnIpMod,
                RsnFechaEliminacion = entity.RsnFechaEliminacion,
                RsnUsuarioEliminacion = entity.RsnUsuarioEliminacion,
                RsnIpEliminacion = entity.RsnIpEliminacion
            };
        }

        public static ReseniaEntity ToNewEntity(ReseniaDataModel model)
        {
            return new ReseniaEntity
            {
                RsnGuid = Guid.NewGuid(),
                AtId = model.AtId,
                RevId = model.RevId,
                RsnComentario = model.RsnComentario,
                RsnRating = model.RsnRating,
                RsnEstado = 'A',
                RsnFechaCreacion = DateTime.UtcNow,
                RsnUsuarioCreacion = model.RsnUsuarioCreacion,
                RsnIpCreacion = model.RsnIpCreacion
            };
        }

        public static void ApplyToEntity(ReseniaDataModel model, ReseniaEntity entity)
        {
            entity.RsnComentario = model.RsnComentario;
            entity.RsnRating = model.RsnRating;
            entity.RsnEstado = model.RsnEstado;
            entity.RsnFechaMod = DateTime.UtcNow;
            entity.RsnUsuarioMod = model.RsnUsuarioMod;
            entity.RsnIpMod = model.RsnIpMod;
        }
    }
}
