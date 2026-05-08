using Microservicio.Atracciones.DataManagement.Models.Auditoria;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IAuditoriaLogDataService
    {
        Task RegistrarAsync(AuditoriaLogDataModel model);
    }
}
