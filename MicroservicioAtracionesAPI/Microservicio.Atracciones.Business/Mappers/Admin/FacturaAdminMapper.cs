using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class FacturaAdminMapper
    {
        public static FacturaResponse ToResponse(FacturaDataModel model, string revCodigo)
            => new()
            {
                FacGuid = model.FacGuid.ToString(),
                FacNumero = model.FacNumero,
                RevCodigo = revCodigo,
                Total = model.FacTotal,
                Moneda = "USD",
                FechaEmision = model.FacFechaEmision,
                Estado = model.FacEstado,
                NombreReceptor = model.DatosFacturacion?.DfacNombre ?? string.Empty,
                CorreoReceptor = model.DatosFacturacion?.DfacCorreo ?? string.Empty
            };
    }
}
