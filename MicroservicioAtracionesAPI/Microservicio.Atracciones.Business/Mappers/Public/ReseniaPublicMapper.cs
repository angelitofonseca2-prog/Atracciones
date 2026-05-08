using Microservicio.Atracciones.Business.DTOs.Public.Resenias;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Mappers.Public
{
    public static class ReseniaPublicMapper
    {
        public static ReseniaResponse ToResponse(ReseniaDataModel model, string atraccionNombre)
            => new()
            {
                RsnGuid = model.RsnGuid.ToString(),
                AtraccionNombre = atraccionNombre,
                Rating = model.RsnRating,
                Comentario = model.RsnComentario,
                FechaCreacion = model.RsnFechaCreacion.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
    }
}
