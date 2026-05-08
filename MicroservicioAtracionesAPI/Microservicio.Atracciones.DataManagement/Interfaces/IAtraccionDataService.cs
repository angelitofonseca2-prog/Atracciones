using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microservicio.Atracciones.DataManagement.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IAtraccionDataService
    {
        Task<AtraccionDataModel?> ObtenerPorIdAsync(int atId);
        Task<AtraccionDataModel?> ObtenerPorGuidAsync(Guid atGuid);
        Task<DataPagedResult<AtraccionDataModel>> ListarConFiltrosAsync(AtraccionFiltroDataModel filtro);
        Task CrearAsync(AtraccionDataModel model);
        Task CrearConRelacionesAsync(
            AtraccionDataModel model,
            IEnumerable<Guid> categoriaGuids,
            IEnumerable<Guid> idiomaGuids,
            IEnumerable<Guid> imagenGuids,
            IEnumerable<Guid> incluyeGuids,
            string usuarioAccion,
            string ip);
        Task ActualizarAsync(AtraccionDataModel model);
        Task ActualizarConRelacionesAsync(
            AtraccionDataModel model,
            IEnumerable<Guid>? categoriaGuids,
            IEnumerable<Guid>? idiomaGuids,
            IEnumerable<Guid>? imagenGuids,
            IEnumerable<Guid>? incluyeGuids,
            string usuarioAccion,
            string ip);
        Task EliminarLogicoAsync(int atId, string usuarioAccion, string ip);

        // E-03: categorías raíz con hijos para los filtros — consulta eficiente en BD
        Task<IReadOnlyList<CategoriaDataModel>> ObtenerCategoriasPorCiudadAsync(string? ciudad);
        Task AgregarCategoriaAsync(int atId, int catId, string usuarioAccion, string ip);
        Task AgregarIdiomaAsync(int atId, int idId, string usuarioAccion, string ip);
        Task AgregarImagenAsync(int atId, int imgId, string usuarioAccion, string ip);
        Task AgregarIncluyeAsync(int atId, int incId, string usuarioAccion, string ip);
        Task EliminarCategoriaAsync(int atId, int catId, string usuarioAccion, string ip);
        Task EliminarIdiomaAsync(int atId, int idId, string usuarioAccion, string ip);
        Task EliminarImagenAsync(int atId, int imgId, string usuarioAccion, string ip);
        Task EliminarIncluyeAsync(int atId, int incId, string usuarioAccion, string ip);
    }
}
