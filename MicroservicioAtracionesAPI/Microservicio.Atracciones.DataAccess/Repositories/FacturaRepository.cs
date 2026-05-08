using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using Microservicio.Atracciones.DataAccess.Entities.Auditoria;
using Microservicio.Atracciones.DataAccess.Repositories.Interfaces;
namespace Microservicio.Atracciones.DataAccess.Repositories
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly AtraccionesDbContext _context;
        public FacturaRepository(AtraccionesDbContext context) => _context = context;

        public async Task<FacturaEntity?> ObtenerPorIdAsync(int facId)
            => await _context.Facturas
                .Include(x => x.DatosFacturacion)
                .FirstOrDefaultAsync(x => x.FacId == facId && x.FacEstado == 'A');

        public async Task<FacturaEntity?> ObtenerPorGuidAsync(Guid facGuid)
            => await _context.Facturas
                .Include(x => x.DatosFacturacion)
                .FirstOrDefaultAsync(x => x.FacGuid == facGuid && x.FacEstado == 'A');

        public async Task<FacturaEntity?> ObtenerPorReservaAsync(int revId)
            => await _context.Facturas
                .Include(x => x.DatosFacturacion)
                .FirstOrDefaultAsync(x => x.RevId == revId);

        public async Task AgregarAsync(FacturaEntity factura)
            => await _context.Facturas.AddAsync(factura);

        public void Actualizar(FacturaEntity factura)
            => _context.Facturas.Update(factura);

        public async Task AgregarDatosFacturacionAsync(DatosFacturacionEntity datos)
            => await _context.DatosFacturacion.AddAsync(datos);
    }
}
