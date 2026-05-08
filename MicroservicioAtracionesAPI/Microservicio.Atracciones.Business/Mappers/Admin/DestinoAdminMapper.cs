using Microservicio.Atracciones.Business.DTOs.Admin.Destinos;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class DestinoAdminMapper
    {
        public static DestinoResponse ToResponse(DestinoDataModel model)
            => new()
            {
                DesGuid = model.DesGuid.ToString(),
                Nombre = model.DesNombre,
                Pais = model.DesPais,
                ImagenUrl = model.DesImagenUrl,
                Estado = model.DesEstado
            };
    }
}
