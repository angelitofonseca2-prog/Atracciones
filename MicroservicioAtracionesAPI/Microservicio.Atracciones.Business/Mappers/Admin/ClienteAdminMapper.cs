using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Clientes;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class ClienteAdminMapper
    {
        public static ClienteResponse ToResponse(ClienteDataModel model)
            => new()
            {
                CliGuid = model.CliGuid.ToString(),
                TipoIdentificacion = model.CliTipoIdentificacion,
                NumeroIdentificacion = model.CliNumeroIdentificacion,
                Nombres = model.CliNombres,
                Apellidos = model.CliApellidos,
                RazonSocial = model.CliRazonSocial,
                Correo = model.CliCorreo,
                Telefono = model.CliTelefono,
                Direccion = model.CliDireccion,
                Estado = model.CliEstado,
                FechaIngreso = model.CliFechaIngreso
            };
    }

}
