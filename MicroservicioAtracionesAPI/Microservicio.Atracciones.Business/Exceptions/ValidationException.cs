using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.Exceptions
{
    public class ValidationException : BusinessException
    {
        public IReadOnlyList<string> Details { get; }

        public ValidationException(IReadOnlyList<string> details)
            : base("Los datos enviados no son válidos.", 400)
        {
            Details = details;
        }

        public ValidationException(string detail)
            : this(new List<string> { detail })
        {
        }
    }
}
