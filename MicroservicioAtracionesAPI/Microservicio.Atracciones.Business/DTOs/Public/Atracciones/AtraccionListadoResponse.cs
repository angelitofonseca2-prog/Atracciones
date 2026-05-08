using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class AtraccionListadoResponse
    {
        // Identificador público — NUNCA at_id
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;

        // Tipo y subtipo (jerarquía CATEGORIA)
        public string TipoTagname { get; set; } = string.Empty;
        public string TipoNombre { get; set; } = string.Empty;
        public string? SubtipoTagname { get; set; }
        public string? SubtipoNombre { get; set; }

        // Etiquetas de inclusiones
        public IList<string> Etiquetas { get; set; } = new List<string>();

        // Descripción truncada a 150 chars
        public string DescripcionCorta { get; set; } = string.Empty;

        // Primera imagen activa (HTTPS)
        public string? ImagenPrincipal { get; set; }   // #11: null si no hay imagen

        public int? DuracionMinutos { get; set; }
        public decimal PrecioDesde { get; set; }
        public string Moneda { get; set; } = "USD";
        public double Calificacion { get; set; }
        public int TotalResenas { get; set; }

        // Idiomas ISO: ["es", "en"]
        public IList<string> IdiomasDisponibles { get; set; } = new List<string>();

        // Disponibilidad en tiempo real — NO cachear
        public DisponibilidadResponse Disponibilidad { get; set; } = new();

        // HATEOAS nivel 3
        public Dictionary<string, string?> Links { get; set; } = new();
    }
}
