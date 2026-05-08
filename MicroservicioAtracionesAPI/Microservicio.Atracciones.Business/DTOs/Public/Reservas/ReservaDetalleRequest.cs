using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Reservas
{
    public class ReservaDetalleRequest
    {
        [Required(ErrorMessage = "El GUID del ticket es obligatorio.")]
        public Guid TckGuid { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }
}
