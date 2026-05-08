using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.Exceptions
{
    public class ConflictException : BusinessException
    {
        public ConflictException(string message)
            : base(message, 409)
        {
        }
    }
}
