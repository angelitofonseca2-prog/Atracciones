using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.Exceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string recurso, object identificador)
            : base($"{recurso} con identificador '{identificador}' no fue encontrado o está inactivo.", 404)
        {
        }
    }
}
