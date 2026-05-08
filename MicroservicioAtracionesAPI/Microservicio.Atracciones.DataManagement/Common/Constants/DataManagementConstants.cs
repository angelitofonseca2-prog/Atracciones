namespace Microservicio.Atracciones.DataManagement.Common.Constants;

/// <summary>
/// Constantes compartidas por los DataServices para evitar magic strings.
/// </summary>
public static class DataManagementConstants
{
    // ----------------------------------------------------------------
    //  Nombres de tablas (usados en AuditoriaLog)
    // ----------------------------------------------------------------
    public static class Tablas
    {
        public const string Destino = "DESTINO";
        public const string Categoria = "CATEGORIA";
        public const string Idioma = "IDIOMA";
        public const string Incluye = "INCLUYE";
        public const string Imagen = "IMAGEN";
        public const string Atraccion = "ATRACCION";
        public const string CategoriaAtraccion = "CATEGORIA_ATRACCION";
        public const string IdiomaAtraccion = "IDIOMA_ATRACCION";
        public const string ImagenAtraccion = "IMAGEN_ATRACCION";
        public const string AtraccionIncluye = "ATRACCION_INCLUYE";
        public const string Ticket = "TICKET";
        public const string Horario = "HORARIO";
        public const string Resenia = "RESENIA";
        public const string Usuario = "USUARIO";
        public const string Roles = "ROLES";
        public const string UsuariosXRoles = "USUARIOXROLES";
        public const string Clientes = "CLIENTES";
        public const string Reservas = "RESERVAS";
        public const string ReservaDetalle = "RESERVA_DETALLE";
        public const string Facturas = "FACTURAS";
        public const string DatosFacturacion = "DATOS_FACTURACION";
    }

    // ----------------------------------------------------------------
    //  Operaciones de auditoría
    // ----------------------------------------------------------------
    public static class Operaciones
    {
        public const string Insert = "INSERT";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
    }

    // ----------------------------------------------------------------
    //  Estados
    // ----------------------------------------------------------------
    public static class Estados
    {
        public const char Activo = 'A';
        public const char Inactivo = 'I';
        public const char Cancelado = 'C';  // Solo en RESERVAS
    }

    // ----------------------------------------------------------------
    //  Tipos de participante en Ticket
    // ----------------------------------------------------------------
    public static class TiposParticipante
    {
        public const string Adulto = "Adulto";
        public const string Nino = "Niño";
        public const string Grupo = "Grupo";
        public const string Estudiante = "Estudiante";
        public const string Senior = "Senior";
    }

    // ----------------------------------------------------------------
    //  Ordenamiento (contrato API)
    // ----------------------------------------------------------------
    public static class Ordenamiento
    {
        public const string Trending = "trending";
        public const string PrecioMasBajo = "lowest_price";
        public const string MejorCalificacion = "highest_weighted_rating";
    }

    // ----------------------------------------------------------------
    //  Paginación
    // ----------------------------------------------------------------
    public static class Paginacion
    {
        public const int LimitDefault = 10;
        public const int LimitMaximo = 50;
        public const int PageDefault = 1;
    }
}
