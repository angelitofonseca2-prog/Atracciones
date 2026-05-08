using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class AtraccionAdminMapper
    {
        public static AtraccionAdminResponse ToResponse(AtraccionDataModel model)
            => new()
            {
                AtGuid = model.AtGuid.ToString(),
                DestinoGuid = model.DesGuid == Guid.Empty ? string.Empty : model.DesGuid.ToString(),
                NumEstablecimiento = model.AtNumEstablecimiento,
                Nombre = model.AtNombre,
                Ciudad = model.DesNombre,
                Pais = model.DesPais,
                Descripcion = model.AtDescripcion,
                Direccion = model.AtDireccion,
                DuracionMinutos = model.AtDuracionMinutos,
                PuntoEncuentro = model.AtPuntoEncuentro,
                PrecioReferencia = model.AtPrecioReferencia,
                Disponible = model.AtDisponible,
                Estado = model.AtEstado,
                TotalResenias = model.AtTotalResenias,
                FechaIngreso = model.AtFechaIngreso,
                ImagenPrincipal = model.Imagenes.FirstOrDefault()?.ImgUrl,
                CategoriaGuids = model.Categorias.Select(c => c.CatGuid.ToString()).ToList(),
                IdiomaGuids = model.Idiomas.Select(i => i.IdGuid.ToString()).ToList(),
                ImagenGuids = model.Imagenes.Select(i => i.ImgGuid.ToString()).ToList(),
                IncluyeGuids = model.Incluyes.Select(i => i.IncGuid.ToString()).ToList(),
                Idiomas = model.Idiomas.Select(i => i.IdDescripcion).ToList(),
                Categorias = model.Categorias.Select(c => c.CatNombre).ToList(),
                Imagenes = model.Imagenes.Select(i => i.ImgUrl).ToList(),
                Incluyes = model.Incluyes.Select(i => i.IncDescripcion).ToList()
            };
    }
}
