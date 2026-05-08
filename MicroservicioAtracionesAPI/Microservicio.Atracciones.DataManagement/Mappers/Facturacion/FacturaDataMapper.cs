using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;

namespace Microservicio.Atracciones.DataManagement.Mappers.Facturacion
{
    public static class FacturaDataMapper
    {
        public static FacturaDataModel? ToDataModel(FacturaEntity? entity)
        {
            if (entity is null) return null;

            return new FacturaDataModel
            {
                FacId = entity.FacId,
                FacGuid = entity.FacGuid,
                RevId = entity.RevId,
                FacNumero = entity.FacNumero,
                FacFechaEmision = entity.FacFechaEmision,
                FacTotal = entity.FacTotal,
                FacObservacion = entity.FacObservacion,
                FacOrigenCanal = entity.FacOrigenCanal,
                FacEstado = entity.FacEstado,
                FacMotivoInhabilitacion = entity.FacMotivoInhabilitacion,
                FacUsuarioIngreso = entity.FacUsuarioIngreso,
                FacIpIngreso = entity.FacIpIngreso,
                FacFechaMod = entity.FacFechaMod,
                FacUsuarioMod = entity.FacUsuarioMod,
                FacIpMod = entity.FacIpMod,
                FacFechaEliminacion = entity.FacFechaEliminacion,
                FacUsuarioEliminacion = entity.FacUsuarioEliminacion,
                FacIpEliminacion = entity.FacIpEliminacion,
                DatosFacturacion = entity.DatosFacturacion is null
                    ? null
                    : new DatosFacturacionDataModel
                    {
                        DfacId = entity.DatosFacturacion.DfacId,
                        DfacGuid = entity.DatosFacturacion.DfacGuid,
                        FacId = entity.DatosFacturacion.FacId,
                        DfacNombre = entity.DatosFacturacion.DfacNombre,
                        DfacApellido = entity.DatosFacturacion.DfacApellido,
                        DfacCorreo = entity.DatosFacturacion.DfacCorreo,
                        DfacTelefono = entity.DatosFacturacion.DfacTelefono
                    }
            };
        }

        public static FacturaEntity ToNewEntity(FacturaDataModel model)
        {
            return new FacturaEntity
            {
                FacGuid = Guid.NewGuid(),
                RevId = model.RevId,
                FacNumero = model.FacNumero,
                FacFechaEmision = DateTime.UtcNow,
                FacTotal = model.FacTotal,
                FacObservacion = model.FacObservacion,
                FacOrigenCanal = model.FacOrigenCanal,
                FacEstado = 'A',
                FacUsuarioIngreso = model.FacUsuarioIngreso,
                FacIpIngreso = model.FacIpIngreso
            };
        }

        public static DatosFacturacionEntity ToNewDatosEntity(DatosFacturacionDataModel model)
        {
            return new DatosFacturacionEntity
            {
                DfacGuid = Guid.NewGuid(),
                FacId = model.FacId,
                DfacNombre = model.DfacNombre,
                DfacApellido = model.DfacApellido,
                DfacCorreo = model.DfacCorreo,
                DfacTelefono = model.DfacTelefono
            };
        }
    }
}
