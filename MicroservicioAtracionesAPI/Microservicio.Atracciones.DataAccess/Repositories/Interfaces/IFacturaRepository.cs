using Microservicio.Atracciones.DataAccess.Entities.Facturacion;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IFacturaRepository
    {
        Task<FacturaEntity?> ObtenerPorIdAsync(int facId);
        Task<FacturaEntity?> ObtenerPorGuidAsync(Guid facGuid);
        Task<FacturaEntity?> ObtenerPorReservaAsync(int revId);
        Task AgregarAsync(FacturaEntity factura);
        void Actualizar(FacturaEntity factura);
        Task AgregarDatosFacturacionAsync(DatosFacturacionEntity datos);
    }
}
