using Atracciones.MsIdentidad.Business.Auth;
using Atracciones.MsIdentidad.Business.Interfaces;
using Atracciones.MsIdentidad.DataManagement.Interfaces;
using Atracciones.MsIdentidad.DataManagement.Models;

namespace Atracciones.MsIdentidad.Business.Services;

public sealed class UsuarioProvisioningService : IUsuarioProvisioningService
{
    private readonly IIdentidadUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioProvisioningService(IIdentidadUsuarioRepository usuarios, IPasswordHasher passwordHasher)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
    }

    public Task<(int usuId, Guid usuGuid)> CrearUsuarioAsync(
        string login,
        string passwordPlain,
        IReadOnlyList<string> roles,
        string creadoPor,
        string ipCreador,
        CancellationToken ct = default)
    {
        var dto = new NuevoUsuarioDto
        {
            Login = login.Trim(),
            PasswordHash = _passwordHasher.Hash(passwordPlain),
            CreadoPor = creadoPor,
            IpCreador = ipCreador,
            Roles = roles,
        };
        return _usuarios.CrearUsuarioConRolesAsync(dto, ct);
    }

    public Task<bool> EliminarUsuarioAsync(Guid usuGuid, CancellationToken ct = default)
        => _usuarios.MarcarInactivoPorGuidAsync(usuGuid, ct);
}
