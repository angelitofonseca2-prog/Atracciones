using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;
using Microservicio.Atracciones.DataManagement.Models.Clientes;

namespace Microservicio.Atracciones.DataManagement.Mappers.Clientes
{
    public static class ClienteDataMapper
    {
        public static ClienteDataModel? ToDataModel(ClienteEntity? entity)
        {
            if (entity is null) return null;

            return new ClienteDataModel
            {
                CliId = entity.CliId,
                CliGuid = entity.CliGuid,
                UsuId = entity.UsuId,
                CliTipoIdentificacion = entity.CliTipoIdentificacion,
                CliNumeroIdentificacion = entity.CliNumeroIdentificacion,
                CliNombres = entity.CliNombres,
                CliApellidos = entity.CliApellidos,
                CliRazonSocial = entity.CliRazonSocial,
                CliCorreo = entity.CliCorreo,
                CliTelefono = entity.CliTelefono,
                CliDireccion = entity.CliDireccion,
                CliFechaIngreso = entity.CliFechaIngreso,
                CliUsuarioIngreso = entity.CliUsuarioIngreso,
                CliIpIngreso = entity.CliIpIngreso,
                CliFechaEliminacion = entity.CliFechaEliminacion,
                CliUsuarioEliminacion = entity.CliUsuarioEliminacion,
                CliIpEliminacion = entity.CliIpEliminacion,
                CliEstado = entity.CliEstado
            };
        }

        public static void ApplyToEntity(ClienteDataModel model, ClienteEntity entity)
        {
            entity.UsuId = model.UsuId;
            entity.CliTipoIdentificacion = model.CliTipoIdentificacion;
            entity.CliNumeroIdentificacion = model.CliNumeroIdentificacion;
            entity.CliNombres = model.CliNombres;
            entity.CliApellidos = model.CliApellidos;
            entity.CliRazonSocial = model.CliRazonSocial;
            entity.CliCorreo = model.CliCorreo;
            entity.CliTelefono = model.CliTelefono;
            entity.CliDireccion = model.CliDireccion;
            entity.CliEstado = model.CliEstado;
        }

        public static ClienteEntity ToNewEntity(ClienteDataModel model)
        {
            return new ClienteEntity
            {
                CliGuid = Guid.NewGuid(),
                UsuId = model.UsuId,
                CliTipoIdentificacion = model.CliTipoIdentificacion,
                CliNumeroIdentificacion = model.CliNumeroIdentificacion,
                CliNombres = model.CliNombres,
                CliApellidos = model.CliApellidos,
                CliRazonSocial = model.CliRazonSocial,
                CliCorreo = model.CliCorreo,
                CliTelefono = model.CliTelefono,
                CliDireccion = model.CliDireccion,
                CliEstado = 'A',
                CliFechaIngreso = DateTime.UtcNow,
                CliUsuarioIngreso = model.CliUsuarioIngreso,
                CliIpIngreso = model.CliIpIngreso
            };
        }
    }
}
