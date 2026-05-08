using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Mappers.Auditoria;
using Microservicio.Atracciones.DataManagement.Models.Auditoria;
using System;
using System.Collections.Generic;
using System.Text;

using Microservicio.Atracciones.DataAccess.Context;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class AuditoriaLogDataService : IAuditoriaLogDataService
    {
        private readonly AtraccionesDbContext _dbContext;

        public AuditoriaLogDataService(AtraccionesDbContext dbContext) => _dbContext = dbContext;

        public async Task RegistrarAsync(AuditoriaLogDataModel model)
        {
            var entity = AuditoriaLogDataMapper.ToNewEntity(model);
            _dbContext.AuditoriaLogs.Add(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
