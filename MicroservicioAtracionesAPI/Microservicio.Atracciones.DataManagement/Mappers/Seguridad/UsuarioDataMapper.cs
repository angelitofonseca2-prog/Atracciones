using Microservicio.Atracciones.DataManagement.Models.Seguridad;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;

namespace Microservicio.Atracciones.DataManagement.Mappers.Seguridad
{
    public static class UsuarioDataMapper
    {
        public static UsuarioDataModel? ToDataModel(UsuarioEntity? entity)
        {
            if (entity is null) return null;

            return new UsuarioDataModel
            {
                UsuId = entity.UsuId,
                UsuGuid = entity.UsuGuid,
                UsuLogin = entity.UsuLogin,
                UsuPasswordHash = entity.UsuPasswordHash,
                UsuEstado = entity.UsuEstado,
                UsuFechaRegistro = entity.UsuFechaRegistro,
                UsuUsuarioRegistro = entity.UsuUsuarioRegistro,
                UsuIpRegistro = entity.UsuIpRegistro,
                UsuFechaMod = entity.UsuFechaMod,
                UsuUsuarioMod = entity.UsuUsuarioMod,
                UsuIpMod = entity.UsuIpMod,
                CliId = entity.Cliente?.CliId, // E-05: Mapeamos CliId
                Roles = entity.UsuariosRoles
                                         .Where(ur => ur.UsuRolEstado == 'A')
                                         .Select(ur => { 
                                             var rolModel = RolDataMapper.ToDataModel(ur.RolEntity)!;
                                             rolModel.RolDescripcion = rolModel.RolDescripcion.ToUpper(); // E-06
                                             return rolModel;
                                         })
                                         .ToList()
            };
        }

        public static void ApplyToEntity(UsuarioDataModel model, UsuarioEntity entity)
        {
            entity.UsuLogin = model.UsuLogin;
            entity.UsuPasswordHash = model.UsuPasswordHash;
            entity.UsuEstado = model.UsuEstado;
            entity.UsuFechaMod = model.UsuFechaMod;
            entity.UsuUsuarioMod = model.UsuUsuarioMod;
            entity.UsuIpMod = model.UsuIpMod;
        }

        public static UsuarioEntity ToNewEntity(UsuarioDataModel model)
        {
            return new UsuarioEntity
            {
                UsuGuid = Guid.NewGuid(),
                UsuLogin = model.UsuLogin,
                UsuPasswordHash = model.UsuPasswordHash,
                UsuEstado = 'A',
                UsuFechaRegistro = DateTime.UtcNow,
                UsuUsuarioRegistro = model.UsuUsuarioRegistro,
                UsuIpRegistro = model.UsuIpRegistro
            };
        }

        public static LoginDataModel? ToLoginDataModel(UsuarioEntity? entity)
        {
            if (entity is null) return null;

            return new LoginDataModel
            {
                UsuId = entity.UsuId,
                UsuGuid = entity.UsuGuid,
                UsuLogin = entity.UsuLogin,
                UsuPasswordHash = entity.UsuPasswordHash,
                UsuEstado = entity.UsuEstado,
                CliId = entity.Cliente?.CliId, // E-05: Obtenemos el cli_id si existe
                Roles = entity.UsuariosRoles
                                        .Where(ur => ur.UsuRolEstado == 'A')
                                        .Select(ur => ur.RolEntity.RolDescripcion.ToUpper()) // E-06: Forzamos upper case
                                        .ToList()
            };
        }
    }
}
