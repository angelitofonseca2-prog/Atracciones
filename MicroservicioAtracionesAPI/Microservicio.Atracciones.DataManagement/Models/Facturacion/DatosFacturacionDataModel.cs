using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Facturacion
{
    public class DatosFacturacionDataModel
    {
        public int DfacId { get; set; }
        public Guid DfacGuid { get; set; }
        public int FacId { get; set; }
        public string DfacNombre { get; set; } = string.Empty;
        public string? DfacApellido { get; set; }
        public string DfacCorreo { get; set; } = string.Empty;
        public string? DfacTelefono { get; set; }
    }

}
