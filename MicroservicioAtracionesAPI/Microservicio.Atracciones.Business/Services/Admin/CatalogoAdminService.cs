using Microservicio.Atracciones.Business.DTOs.Admin.Catalogos;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class CatalogoAdminService : ICatalogoAdminService
    {
        private readonly ICategoriaDataService _categoriaService;
        private readonly IIdiomaDataService _idiomaService;
        private readonly IIncluyeDataService _incluyeService;

        public CatalogoAdminService(
            ICategoriaDataService categoriaService,
            IIdiomaDataService idiomaService,
            IIncluyeDataService incluyeService)
        {
            _categoriaService = categoriaService;
            _idiomaService = idiomaService;
            _incluyeService = incluyeService;
        }

        public async Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync()
            => (await _categoriaService.ListarActivasAsync()).Select(ToCategoriaResponse).ToList();

        public async Task<CategoriaResponse> CrearCategoriaAsync(CrearCategoriaRequest request, string usuarioAccion, string ip)
        {
            ValidarTexto(request.Nombre, "El nombre de la categoría es obligatorio.");

            var model = new CategoriaDataModel
            {
                CatNombre = request.Nombre.Trim(),
                ParentGuid = request.ParentGuid
            };

            try
            {
                await _categoriaService.CrearAsync(model, usuarioAccion, ip);
            }
            catch (InvalidOperationException ex)
            {
                throw new NotFoundException("Categoría padre", ex.Message);
            }

            return ToCategoriaResponse(model);
        }

        public async Task<CategoriaResponse> ActualizarCategoriaAsync(Guid catGuid, ActualizarCategoriaRequest request, string usuarioAccion, string ip)
        {
            ValidarTexto(request.Nombre, "El nombre de la categoría es obligatorio.");

            var model = await _categoriaService.ObtenerPorGuidAsync(catGuid)
                ?? throw new NotFoundException("Categoría", catGuid);

            model.CatNombre = request.Nombre.Trim();
            model.ParentGuid = request.ParentGuid;

            try
            {
                await _categoriaService.ActualizarAsync(model, usuarioAccion, ip);
            }
            catch (InvalidOperationException ex)
            {
                throw new NotFoundException("Categoría padre", ex.Message);
            }

            return ToCategoriaResponse(await _categoriaService.ObtenerPorGuidAsync(catGuid) ?? model);
        }

        public async Task EliminarCategoriaAsync(Guid catGuid, string usuarioAccion, string ip)
        {
            var model = await _categoriaService.ObtenerPorGuidAsync(catGuid)
                ?? throw new NotFoundException("Categoría", catGuid);

            await _categoriaService.EliminarLogicoAsync(model.CatId, usuarioAccion, ip);
        }

        public async Task<IReadOnlyList<IdiomaResponse>> ListarIdiomasAsync()
            => (await _idiomaService.ListarActivosAsync()).Select(ToIdiomaResponse).ToList();

        public async Task<IdiomaResponse> CrearIdiomaAsync(CrearIdiomaRequest request, string usuarioAccion, string ip)
        {
            ValidarTexto(request.Descripcion, "La descripción del idioma es obligatoria.");
            var descripcion = request.Descripcion.Trim();

            await ValidarIdiomaDuplicadoAsync(descripcion);

            var model = new IdiomaDataModel { IdDescripcion = descripcion };
            await _idiomaService.CrearAsync(model, usuarioAccion, ip);
            return ToIdiomaResponse(model);
        }

        public async Task<IdiomaResponse> ActualizarIdiomaAsync(Guid idGuid, ActualizarIdiomaRequest request, string usuarioAccion, string ip)
        {
            ValidarTexto(request.Descripcion, "La descripción del idioma es obligatoria.");
            var descripcion = request.Descripcion.Trim();

            var model = await _idiomaService.ObtenerPorGuidAsync(idGuid)
                ?? throw new NotFoundException("Idioma", idGuid);

            await ValidarIdiomaDuplicadoAsync(descripcion, model.IdId);
            model.IdDescripcion = descripcion;
            await _idiomaService.ActualizarAsync(model, usuarioAccion, ip);
            return ToIdiomaResponse(model);
        }

        public async Task EliminarIdiomaAsync(Guid idGuid, string usuarioAccion, string ip)
        {
            var model = await _idiomaService.ObtenerPorGuidAsync(idGuid)
                ?? throw new NotFoundException("Idioma", idGuid);

            await _idiomaService.EliminarLogicoAsync(model.IdId, usuarioAccion, ip);
        }

        public async Task<IReadOnlyList<IncluyeResponse>> ListarIncluyeAsync()
            => (await _incluyeService.ListarActivosAsync()).Select(ToIncluyeResponse).ToList();

        public async Task<IncluyeResponse> CrearIncluyeAsync(CrearIncluyeRequest request)
        {
            ValidarTexto(request.Descripcion, "La descripción del elemento incluido es obligatoria.");
            var model = new IncluyeDataModel { IncDescripcion = request.Descripcion.Trim() };
            await _incluyeService.CrearAsync(model);
            return ToIncluyeResponse(model);
        }

        public async Task<IncluyeResponse> ActualizarIncluyeAsync(Guid incGuid, ActualizarIncluyeRequest request)
        {
            ValidarTexto(request.Descripcion, "La descripción del elemento incluido es obligatoria.");

            var model = await _incluyeService.ObtenerPorGuidAsync(incGuid)
                ?? throw new NotFoundException("Incluye", incGuid);

            model.IncDescripcion = request.Descripcion.Trim();
            await _incluyeService.ActualizarAsync(model);
            return ToIncluyeResponse(model);
        }

        public async Task EliminarIncluyeAsync(Guid incGuid)
        {
            var model = await _incluyeService.ObtenerPorGuidAsync(incGuid)
                ?? throw new NotFoundException("Incluye", incGuid);

            await _incluyeService.EliminarLogicoAsync(model.IncId);
        }

        private async Task ValidarIdiomaDuplicadoAsync(string descripcion, int? idActual = null)
        {
            if (await _idiomaService.ExisteDescripcionAsync(descripcion, idActual))
                throw new ConflictException($"Ya existe un idioma con la descripción '{descripcion}'.");
        }

        private static void ValidarTexto(string? valor, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ValidationException(new[] { mensaje });
        }

        private static CategoriaResponse ToCategoriaResponse(CategoriaDataModel c) => new()
        {
            CatGuid = c.CatGuid.ToString(),
            Nombre = c.CatNombre,
            ParentGuid = c.ParentGuid?.ToString(),
            ParentNombre = c.ParentNombre
        };

        private static IdiomaResponse ToIdiomaResponse(IdiomaDataModel i) => new()
        {
            IdGuid = i.IdGuid.ToString(),
            Descripcion = i.IdDescripcion
        };

        private static IncluyeResponse ToIncluyeResponse(IncluyeDataModel i) => new()
        {
            IncluyeGuid = i.IncGuid.ToString(),
            Descripcion = i.IncDescripcion
        };
    }
}
