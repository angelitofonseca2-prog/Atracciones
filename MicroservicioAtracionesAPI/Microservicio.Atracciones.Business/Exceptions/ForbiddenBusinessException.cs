using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.Exceptions
{
    public class ForbiddenBusinessException : BusinessException
    {
        public ForbiddenBusinessException(string message = "No tienes permisos para realizar esta acción.")
            : base(message, 403)
        {
        }
    }
}
