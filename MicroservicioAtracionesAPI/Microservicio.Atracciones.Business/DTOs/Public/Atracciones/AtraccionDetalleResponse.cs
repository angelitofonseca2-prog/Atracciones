using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class AtraccionDetalleResponse : AtraccionListadoResponse
    {
        // Descripción completa sin límite
        public string Descripcion { get; set; } = string.Empty;

        // Galería de imágenes HTTPS
        public IList<string> Imagenes { get; set; } = new List<string>();

        // Servicios incluidos y excluidos (prefijo "NO:" en BD)
        public IList<string> Incluye { get; set; } = new List<string>();
        public IList<string> NoIncluye { get; set; } = new List<string>();

        public string? PuntoEncuentro { get; set; }
        public bool IncluyeTransporte { get; set; }
        public bool IncluyeAcompaniante { get; set; }

        // Tipos de ticket disponibles
        public IList<TicketDisponibleResponse> Tickets { get; set; } = new List<TicketDisponibleResponse>();

        // Próximos 7 días con cupos
        public IList<HorarioProximoResponse> HorariosProximos { get; set; } = new List<HorarioProximoResponse>();
    }
}
