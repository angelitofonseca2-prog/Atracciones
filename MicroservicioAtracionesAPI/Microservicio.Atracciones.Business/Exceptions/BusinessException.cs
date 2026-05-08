using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.Exceptions
{
    public class BusinessException : Exception
    {
        public int HttpStatusCode { get; }

        public BusinessException(string message, int httpStatusCode = 400)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
