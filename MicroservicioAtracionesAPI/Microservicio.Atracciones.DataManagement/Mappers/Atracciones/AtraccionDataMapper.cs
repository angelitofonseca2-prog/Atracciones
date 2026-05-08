using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.DataManagement.Mappers.Atracciones
{
    public static class AtraccionDataMapper
    {
        public static AtraccionDataModel? ToDataModel(AtraccionEntity? entity, decimal? precioDesde = null, double? calificacion = null)
        {
            if (entity is null) return null;

            return new AtraccionDataModel
            {
                AtId = entity.AtId,
                AtGuid = entity.AtGuid,
                DesId = entity.DesId,
                AtNumEstablecimiento = entity.AtNumEstablecimiento,
                AtNombre = entity.AtNombre,
                AtDescripcion = entity.AtDescripcion,
                AtTotalResenias = entity.AtTotalResenias,
                AtDireccion = entity.AtDireccion,
                AtDuracionMinutos = entity.AtDuracionMinutos,
                AtPuntoEncuentro = entity.AtPuntoEncuentro,
                AtPrecioReferencia = entity.AtPrecioReferencia,
                AtIncluyeAcompaniante = entity.AtIncluyeAcompaniante,
                AtIncluyeTransporte = entity.AtIncluyeTransporte,
                AtDisponible = entity.AtDisponible,
                AtEstado = entity.AtEstado,
                AtFechaIngreso = entity.AtFechaIngreso,
                AtUsuarioIngreso = entity.AtUsuarioIngreso,
                AtIpIngreso = entity.AtIpIngreso,
                AtFechaMod = entity.AtFechaMod,
                AtUsuarioMod = entity.AtUsuarioMod,
                AtIpMod = entity.AtIpMod,
                AtFechaEliminacion = entity.AtFechaEliminacion,
                AtUsuarioEliminacion = entity.AtUsuarioEliminacion,
                AtIpEliminacion = entity.AtIpEliminacion,

                // Destino desnormalizado
                DesGuid = entity.Destino?.DesGuid ?? Guid.Empty,
                DesNombre = entity.Destino?.DesNombre ?? string.Empty,
                DesPais = entity.Destino?.DesPais ?? string.Empty,

                // Relaciones N:M aplanadas
                Categorias = entity.CategoriasAtracciones
                                   .Where(ca => ca.CaEstado == 'A')
                                   .Select(ca => MapCategoria(ca.Categoria))
                                   .ToList(),

                Idiomas = entity.IdiomasAtracciones
                                   .Where(ia => ia.IaEstado == 'A')
                                   .Select(ia => new IdiomaDataModel
                                   {
                                       IdId = ia.Idioma.IdId,
                                       IdGuid = ia.Idioma.IdGuid,
                                       IdDescripcion = ia.Idioma.IdDescripcion,
                                       IdEstado = ia.Idioma.IdEstado
                                   })
                                   .ToList(),

                Imagenes = entity.ImagenesAtracciones
                                   .Where(ima => ima.ImaEstado == 'A')
                                   .Select(ima => new ImagenDataModel
                                   {
                                       ImgId = ima.Imagen.ImgId,
                                       ImgGuid = ima.Imagen.ImgGuid,
                                       ImgUrl = ima.Imagen.ImgUrl,
                                       ImgDescripcion = ima.Imagen.ImgDescripcion,
                                       ImgEstado = ima.Imagen.ImgEstado
                                   })
                                   .ToList(),

                Incluyes = entity.AtraccionesIncluyen
                                   .Where(ai => ai.AiEstado == 'A')
                                   .Select(ai => new IncluyeDataModel
                                   {
                                       IncId = ai.Incluye.IncId,
                                       IncGuid = ai.Incluye.IncGuid,
                                       IncDescripcion = ai.Incluye.IncDescripcion,
                                       IncEstado = ai.Incluye.IncEstado
                                   })
                                   .ToList(),

                PrecioDesde = precioDesde,
                CalificacionPromedio = calificacion
            };
        }

        public static void ApplyToEntity(AtraccionDataModel model, AtraccionEntity entity)
        {
            entity.DesId = model.DesId;
            entity.AtNumEstablecimiento = model.AtNumEstablecimiento;
            entity.AtNombre = model.AtNombre;
            entity.AtDescripcion = model.AtDescripcion;
            entity.AtDireccion = model.AtDireccion;
            entity.AtDuracionMinutos = model.AtDuracionMinutos;
            entity.AtPuntoEncuentro = model.AtPuntoEncuentro;
            entity.AtPrecioReferencia = model.AtPrecioReferencia;
            entity.AtIncluyeAcompaniante = model.AtIncluyeAcompaniante;
            entity.AtIncluyeTransporte = model.AtIncluyeTransporte;
            entity.AtDisponible = model.AtDisponible;
            entity.AtEstado = model.AtEstado;
            entity.AtFechaMod = DateTime.UtcNow;
            entity.AtUsuarioMod = model.AtUsuarioMod;
            entity.AtIpMod = model.AtIpMod;
        }

        public static AtraccionEntity ToNewEntity(AtraccionDataModel model)
        {
            return new AtraccionEntity
            {
                AtGuid = Guid.NewGuid(),
                DesId = model.DesId,
                AtNumEstablecimiento = model.AtNumEstablecimiento,
                AtNombre = model.AtNombre,
                AtDescripcion = model.AtDescripcion,
                AtTotalResenias = 0,
                AtDireccion = model.AtDireccion,
                AtDuracionMinutos = model.AtDuracionMinutos,
                AtPuntoEncuentro = model.AtPuntoEncuentro,
                AtPrecioReferencia = model.AtPrecioReferencia,
                AtIncluyeAcompaniante = model.AtIncluyeAcompaniante,
                AtIncluyeTransporte = model.AtIncluyeTransporte,
                AtDisponible = true,
                AtEstado = 'A',
                AtFechaIngreso = DateTime.UtcNow,
                AtUsuarioIngreso = model.AtUsuarioIngreso,
                AtIpIngreso = model.AtIpIngreso
            };
        }

        // Mapea categoría con su padre si existe (para tipo/subtipo)
        private static CategoriaDataModel MapCategoria(CategoriaEntity cat)
        {
            return new CategoriaDataModel
            {
                CatId = cat.CatId,
                CatGuid = cat.CatGuid,
                CatParentId = cat.CatParentId,
                CatNombre = cat.CatNombre,
                CatEstado = cat.CatEstado,
                ParentGuid = cat.Parent?.CatGuid,
                ParentNombre = cat.Parent?.CatNombre
            };
        }
    }
}
