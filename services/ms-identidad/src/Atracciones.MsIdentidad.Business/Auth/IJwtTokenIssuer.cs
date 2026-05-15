namespace Atracciones.MsIdentidad.Business.Auth;

public sealed record UsuarioParaToken(
    int UsuId,
    Guid UsuGuid,
    string Login,
    int? CliId,
    IReadOnlyList<string> Roles);

public interface IJwtTokenIssuer
{
    (string Token, DateTime ExpiraUtc) Emitir(UsuarioParaToken usuario);
}
