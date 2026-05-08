using Microservicio.Atracciones.Business.DTOs.Admin.Resenias;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class ReseniaAdminMapper
    {
        public static ReseniaAdminResponse ToResponse(ReseniaDataModel model, string atraccionNombre, string clienteNombre)
            => new()
            {
                RsnGuid = model.RsnGuid.ToString(),
                AtraccionNombre = atraccionNombre,
                ClienteNombre = clienteNombre,
                Rating = model.RsnRating,
                Comentario = model.RsnComentario,
                Estado = model.RsnEstado,
                FechaCreacion = model.RsnFechaCreacion
            };
    }
}
