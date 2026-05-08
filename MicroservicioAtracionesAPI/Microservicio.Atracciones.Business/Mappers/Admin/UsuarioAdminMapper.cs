using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;
using Microservicio.Atracciones.DataManagement.Models.Seguridad;

namespace Microservicio.Atracciones.Business.Mappers.Admin
{
    public static class UsuarioAdminMapper
    {
        public static UsuarioResponse ToResponse(UsuarioDataModel model)
            => new()
            {
                UsuGuid = model.UsuGuid.ToString(),
                Login = model.UsuLogin,
                Estado = model.UsuEstado,
                Roles = model.Roles.Select(r => r.RolDescripcion).ToList(),
                FechaRegistro = model.UsuFechaRegistro
            };
    }

}
