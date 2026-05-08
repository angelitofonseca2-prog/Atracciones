using Microservicio.Atracciones.Business.DTOs.Admin.Imagenes;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.Business.Services.Admin
{
    public class ImagenAdminService : IImagenAdminService
    {
        private readonly IImagenDataService _imagenService;

        public ImagenAdminService(IImagenDataService imagenService)
            => _imagenService = imagenService;

        public async Task<IReadOnlyList<ImagenResponse>> ListarAsync()
            => (await _imagenService.ListarActivasAsync()).Select(ToResponse).ToList();

        public async Task<ImagenResponse> CrearAsync(CrearImagenRequest request, string usuarioAccion, string ip)
        {
            ValidarUrl(request.Url);
            var url = request.Url.Trim();

            if (await _imagenService.ObtenerPorUrlAsync(url) is not null)
                throw new ConflictException($"Ya existe una imagen activa con la URL '{url}'.");

            var model = new ImagenDataModel
            {
                ImgUrl = url,
                ImgDescripcion = request.Descripcion?.Trim()
            };

            await _imagenService.CrearAsync(model, usuarioAccion, ip);
            return ToResponse(model);
        }

        public async Task<ImagenResponse> ActualizarAsync(Guid imgGuid, ActualizarImagenRequest request, string usuarioAccion, string ip)
        {
            var model = await _imagenService.ObtenerPorGuidAsync(imgGuid)
                ?? throw new NotFoundException("Imagen", imgGuid);

            if (request.Url is not null)
            {
                ValidarUrl(request.Url);
                var url = request.Url.Trim();
                var duplicada = await _imagenService.ObtenerPorUrlAsync(url);
                if (duplicada is not null && duplicada.ImgId != model.ImgId)
                    throw new ConflictException($"Ya existe una imagen activa con la URL '{url}'.");

                model.ImgUrl = url;
            }

            if (request.Descripcion is not null)
                model.ImgDescripcion = request.Descripcion.Trim();

            await _imagenService.ActualizarAsync(model, usuarioAccion, ip);
            return ToResponse(model);
        }

        public async Task EliminarAsync(Guid imgGuid, string usuarioAccion, string ip)
        {
            var model = await _imagenService.ObtenerPorGuidAsync(imgGuid)
                ?? throw new NotFoundException("Imagen", imgGuid);

            await _imagenService.EliminarLogicoAsync(model.ImgId, usuarioAccion, ip);
        }

        private static void ValidarUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ValidationException(new[] { "La URL de la imagen es obligatoria." });

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ValidationException(new[] { "La URL de la imagen debe ser una URL http o https válida." });
        }

        private static ImagenResponse ToResponse(ImagenDataModel model)
            => new()
            {
                ImgGuid = model.ImgGuid.ToString(),
                Url = model.ImgUrl,
                Descripcion = model.ImgDescripcion,
                Estado = model.ImgEstado,
                FechaIngreso = model.ImgFechaIngreso
            };
    }
}
