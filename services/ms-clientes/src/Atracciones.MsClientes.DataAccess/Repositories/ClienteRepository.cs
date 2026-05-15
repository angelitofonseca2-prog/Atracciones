using Atracciones.MsClientes.DataAccess.Context;
using Atracciones.MsClientes.DataAccess.Entities;
using Atracciones.MsClientes.DataManagement.Interfaces;
using Atracciones.MsClientes.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsClientes.DataAccess.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly CrmDbContext _db;

    public ClienteRepository(CrmDbContext db) => _db = db;

    public async Task<ClienteDto?> ObtenerActivoPorGuidAsync(Guid cliGuid, CancellationToken ct = default)
    {
        var x = await _db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(e => e.CliGuid == cliGuid && e.CliEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task<ClienteDto?> ObtenerActivoPorNumeroIdentificacionAsync(string numeroIdentificacion, CancellationToken ct = default)
    {
        var n = numeroIdentificacion.Trim();
        var x = await _db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(e => e.CliNumeroIdentificacion == n && e.CliEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public async Task UpsertMirrorAsync(ClienteMirrorDto dto, CancellationToken ct = default)
    {
        var existing = await _db.Clientes.FirstOrDefaultAsync(x => x.CliGuid == dto.UsuGuid, ct);
        if (existing is null)
        {
            _db.Clientes.Add(new ClienteEntity
            {
                CliGuid = dto.UsuGuid,
                CliTipoIdentificacion = dto.TipoIdentificacion.Trim(),
                CliNumeroIdentificacion = dto.NumeroIdentificacion.Trim(),
                CliNombres = dto.Nombres?.Trim(),
                CliApellidos = dto.Apellidos?.Trim(),
                CliRazonSocial = dto.RazonSocial?.Trim(),
                CliCorreo = dto.Correo.Trim(),
                CliTelefono = dto.Telefono?.Trim(),
                CliDireccion = dto.Direccion?.Trim(),
                CliEstado = 'A',
                CliUsuarioIngreso = dto.CreadoPor,
                CliIpIngreso = dto.IpCreador,
            });
        }
        else
        {
            existing.CliTipoIdentificacion = dto.TipoIdentificacion.Trim();
            existing.CliNumeroIdentificacion = dto.NumeroIdentificacion.Trim();
            existing.CliNombres = dto.Nombres?.Trim();
            existing.CliApellidos = dto.Apellidos?.Trim();
            existing.CliRazonSocial = dto.RazonSocial?.Trim();
            existing.CliCorreo = dto.Correo.Trim();
            existing.CliTelefono = dto.Telefono?.Trim();
            existing.CliDireccion = dto.Direccion?.Trim();
            existing.CliEstado = 'A';
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<ClienteDto> CrearAsync(CrearClienteInternoDto dto, CancellationToken ct = default)
    {
        if (await _db.Clientes.AnyAsync(x => x.CliGuid == dto.CliGuid, ct))
            throw new InvalidOperationException($"Cliente ya existe: {dto.CliGuid}");

        var entity = new ClienteEntity
        {
            CliGuid = dto.CliGuid,
            CliTipoIdentificacion = dto.TipoIdentificacion.Trim(),
            CliNumeroIdentificacion = dto.NumeroIdentificacion.Trim(),
            CliNombres = dto.Nombres?.Trim(),
            CliApellidos = dto.Apellidos?.Trim(),
            CliRazonSocial = dto.RazonSocial?.Trim(),
            CliCorreo = dto.Correo.Trim(),
            CliTelefono = dto.Telefono?.Trim(),
            CliDireccion = dto.Direccion?.Trim(),
            CliEstado = 'A',
            CliUsuarioIngreso = dto.CreadoPor,
            CliIpIngreso = dto.IpCreador,
        };
        _db.Clientes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ClienteDto?> ActualizarCamposAsync(Guid cliGuid, ActualizarClienteInternoDto dto, CancellationToken ct = default)
    {
        var e = await _db.Clientes.FirstOrDefaultAsync(x => x.CliGuid == cliGuid && x.CliEstado == 'A', ct);
        if (e is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.TipoIdentificacion)) e.CliTipoIdentificacion = dto.TipoIdentificacion.Trim();
        if (!string.IsNullOrWhiteSpace(dto.NumeroIdentificacion)) e.CliNumeroIdentificacion = dto.NumeroIdentificacion.Trim();
        if (dto.Nombres is not null) e.CliNombres = string.IsNullOrWhiteSpace(dto.Nombres) ? null : dto.Nombres.Trim();
        if (dto.Apellidos is not null) e.CliApellidos = string.IsNullOrWhiteSpace(dto.Apellidos) ? null : dto.Apellidos.Trim();
        if (dto.RazonSocial is not null) e.CliRazonSocial = string.IsNullOrWhiteSpace(dto.RazonSocial) ? null : dto.RazonSocial.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Correo)) e.CliCorreo = dto.Correo.Trim();
        if (dto.Telefono is not null) e.CliTelefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        if (dto.Direccion is not null) e.CliDireccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();

        await _db.SaveChangesAsync(ct);
        return Map(e);
    }

    public async Task<bool> MarcarInactivoAsync(Guid cliGuid, CancellationToken ct = default)
    {
        var e = await _db.Clientes.FirstOrDefaultAsync(x => x.CliGuid == cliGuid, ct);
        if (e is null) return false;
        e.CliEstado = 'I';
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ClienteDto Map(ClienteEntity x) => new()
    {
        CliGuid = x.CliGuid,
        TipoIdentificacion = x.CliTipoIdentificacion,
        NumeroIdentificacion = x.CliNumeroIdentificacion,
        Nombres = x.CliNombres,
        Apellidos = x.CliApellidos,
        RazonSocial = x.CliRazonSocial,
        Correo = x.CliCorreo,
        Telefono = x.CliTelefono,
        Direccion = x.CliDireccion,
    };
}
