using Atracciones.MsClientes.Business.DTOs;
using Atracciones.MsClientes.Business.Exceptions;
using Atracciones.MsClientes.Business.Interfaces;
using Atracciones.MsClientes.DataManagement.Interfaces;

namespace Atracciones.MsClientes.Business.Services;

public sealed class ClientePerfilAppService : IClientePerfilAppService
{
    private readonly IClienteRepository _repo;

    public ClientePerfilAppService(IClienteRepository repo) => _repo = repo;

    public async Task<PerfilClienteResponse> ObtenerAsync(Guid usuGuid, CancellationToken ct = default)
    {
        var c = await _repo.ObtenerActivoPorGuidAsync(usuGuid, ct)
            ?? throw new NotFoundException("Cliente asociado al usuario no encontrado.");
        return Map(c);
    }

    public async Task<PerfilClienteResponse> ActualizarAsync(Guid usuGuid, ActualizarPerfilClienteRequest request, CancellationToken ct = default)
    {
        var errores = new List<string>();
        if (request.Correo is not null && string.IsNullOrWhiteSpace(request.Correo))
            errores.Add("Correo inválido.");
        if (errores.Count > 0)
            throw new ValidationException(errores);

        var dto = new ActualizarClienteInternoDto
        {
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            Telefono = request.Telefono,
        };

        var updated = await _repo.ActualizarCamposAsync(usuGuid, dto, ct)
            ?? throw new NotFoundException("Cliente asociado al usuario no encontrado.");

        return Map(updated);
    }

    private static PerfilClienteResponse Map(DataManagement.Models.ClienteDto c) => new()
    {
        CliGuid = c.CliGuid,
        Nombres = c.Nombres,
        Apellidos = c.Apellidos,
        Correo = c.Correo,
        Telefono = c.Telefono,
        TipoIdentificacion = c.TipoIdentificacion,
        NumeroIdentificacion = c.NumeroIdentificacion,
    };
}
