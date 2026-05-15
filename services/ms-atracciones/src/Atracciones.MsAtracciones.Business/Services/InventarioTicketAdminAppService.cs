using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;
using Atracciones.MsAtracciones.Business.Exceptions;
using DomainValidationException = Atracciones.MsAtracciones.Business.Exceptions.ValidationException;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.Business.Services;

public sealed class InventarioTicketAdminAppService : IInventarioTicketAdminAppService
{
    private readonly IInventarioRepository _repo;

    public InventarioTicketAdminAppService(IInventarioRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<TicketResponse>> ListarTicketsAsync(CancellationToken ct = default)
    {
        var rows = await _repo.ListarTicketsAdminAsync(ct);
        var list = new List<TicketResponse>();
        foreach (var t in rows)
            list.Add(await ToTicketResponseAsync(t, includeHorarios: false, ct));
        return list;
    }

    public async Task<TicketResponse> ObtenerTicketPorGuidAsync(Guid tckGuid, CancellationToken ct = default)
    {
        var t = await _repo.ObtenerTicketAdminAsync(tckGuid, ct)
            ?? throw new NotFoundException("Ticket", tckGuid);
        return await ToTicketResponseAsync(t, includeHorarios: true, ct);
    }

    public async Task<IReadOnlyList<TicketResponse>> ListarTicketsPorAtraccionAsync(Guid atGuid, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerAtraccionAdminCompletaAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);
        var rows = await _repo.ListarTicketsPorAtraccionAdminAsync(atGuid, ct);
        var list = new List<TicketResponse>();
        foreach (var t in rows)
            list.Add(await ToTicketResponseAsync(t, includeHorarios: false, ct));
        return list;
    }

    public async Task<TicketResponse> CrearTicketAsync(CrearTicketRequest request, string usuario, string ip, CancellationToken ct = default)
    {
        Validar(request);
        _ = await _repo.ObtenerAtraccionAdminCompletaAsync(request.AtGuid, ct)
            ?? throw new NotFoundException("Atracción", request.AtGuid);

        var nuevo = await _repo.CrearTicketAsync(new TicketPersistModel(null, request.AtGuid, request.Titulo, request.Precio, request.TipoParticipante, request.CapacidadMaxima, request.CuposDisponibles, usuario, ip), ct);

        var created = await _repo.ObtenerTicketAdminAsync(nuevo, ct)
            ?? throw new InvalidOperationException("No se pudo leer el ticket creado.");
        return await ToTicketResponseAsync(created, includeHorarios: false, ct);
    }

    public async Task<TicketResponse> ActualizarTicketAsync(Guid tckGuid, ActualizarTicketRequest request, string usuario, string ip, CancellationToken ct = default)
    {
        Validar(request);
        var current = await _repo.ObtenerTicketAdminAsync(tckGuid, ct)
            ?? throw new NotFoundException("Ticket", tckGuid);

        await _repo.ActualizarTicketAsync(new TicketPersistModel(
            tckGuid,
            current.AtGuid,
            request.Titulo ?? current.TckTitulo,
            request.Precio ?? current.TckPrecio,
            current.TckTipoParticipante,
            current.TckCapacidadMaxima,
            request.CuposDisponibles ?? current.TckCuposDisponibles,
            usuario,
            ip), ct);

        var updated = await _repo.ObtenerTicketAdminAsync(tckGuid, ct)
            ?? throw new NotFoundException("Ticket", tckGuid);
        return await ToTicketResponseAsync(updated, includeHorarios: true, ct);
    }

    public async Task EliminarTicketAsync(Guid tckGuid, string usuario, string ip, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerTicketAdminAsync(tckGuid, ct)
            ?? throw new NotFoundException("Ticket", tckGuid);
        await _repo.EliminarTicketLogicoAsync(tckGuid, usuario, ip, ct);
    }

    public async Task<IReadOnlyList<HorarioResponse>> ListarHorariosAsync(CancellationToken ct = default)
    {
        var rows = await _repo.ListarHorariosAdminAsync(ct);
        return rows.Select(ToHorarioResponse).ToList();
    }

    public Task<HorarioResponse> ObtenerHorarioPorGuidAsync(Guid horGuid, CancellationToken ct = default)
        => GetHorarioAsync(horGuid, ct);

    public async Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorTicketAsync(Guid tckGuid, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerTicketAdminAsync(tckGuid, ct)
            ?? throw new NotFoundException("Ticket", tckGuid);
        var rows = await _repo.ListarHorariosPorTicketAdminAsync(tckGuid, ct);
        return rows.Select(ToHorarioResponse).ToList();
    }

    public async Task<IReadOnlyList<HorarioResponse>> ListarHorariosPorAtraccionAsync(Guid atGuid, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerAtraccionAdminCompletaAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);
        var rows = await _repo.ListarHorariosPorAtraccionAdminAsync(atGuid, ct);
        return rows.Select(ToHorarioResponse).ToList();
    }

    public async Task<HorarioResponse> CrearHorarioAsync(CrearHorarioRequest request, string usuario, string ip, CancellationToken ct = default)
    {
        Validar(request);
        _ = await _repo.ObtenerTicketAdminAsync(request.TckGuid, ct)
            ?? throw new NotFoundException("Ticket", request.TckGuid);

        var nuevo = await _repo.CrearHorarioAsync(new HorarioPersistModel(null, request.TckGuid, request.Fecha, request.HoraInicio, request.HoraFin, request.CuposDisponibles, usuario, ip), ct);

        var created = await _repo.ObtenerHorarioAdminAsync(nuevo, ct)
            ?? throw new InvalidOperationException("No se pudo leer el horario creado.");
        return ToHorarioResponse(created);
    }

    public async Task<HorarioResponse> ActualizarHorarioAsync(Guid horGuid, ActualizarHorarioRequest request, string usuario, string ip, CancellationToken ct = default)
    {
        Validar(request);
        var h = await _repo.ObtenerHorarioAdminAsync(horGuid, ct)
            ?? throw new NotFoundException("Horario", horGuid);

        await _repo.ActualizarHorarioAsync(new HorarioPersistModel(
            horGuid,
            h.TckGuid,
            request.Fecha ?? h.HorFecha,
            request.HoraInicio ?? h.HorHoraInicio,
            request.HoraFin ?? h.HorHoraFin,
            request.CuposDisponibles ?? h.HorCuposDisponibles,
            usuario,
            ip), ct);

        return await GetHorarioAsync(horGuid, ct);
    }

    public async Task EliminarHorarioAsync(Guid horGuid, string usuario, string ip, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerHorarioAdminAsync(horGuid, ct)
            ?? throw new NotFoundException("Horario", horGuid);
        await _repo.EliminarHorarioLogicoAsync(horGuid, usuario, ip, ct);
    }

    private async Task<TicketResponse> ToTicketResponseAsync(TicketAdminRow t, bool includeHorarios, CancellationToken ct)
    {
        var horarios = includeHorarios
            ? (await _repo.ListarHorariosPorTicketAdminAsync(t.TckGuid, ct)).Select(ToHorarioResponse).ToList()
            : new List<HorarioResponse>();

        return new TicketResponse
        {
            TckGuid = t.TckGuid.ToString(),
            AtraccionGuid = t.AtGuid.ToString(),
            AtraccionNombre = t.AtNombre,
            Titulo = t.TckTitulo,
            Precio = t.TckPrecio,
            TipoParticipante = t.TckTipoParticipante,
            CapacidadMaxima = t.TckCapacidadMaxima,
            CuposDisponibles = t.TckCuposDisponibles,
            Estado = t.TckEstado,
            FechaIngreso = t.TckFechaIngreso,
            Horarios = horarios,
        };
    }

    private static HorarioResponse ToHorarioResponse(HorarioAdminRow h) =>
        new()
        {
            HorGuid = h.HorGuid.ToString(),
            TckGuid = h.TckGuid.ToString(),
            AtraccionGuid = h.AtGuid.ToString(),
            AtraccionNombre = h.AtNombre,
            TicketTitulo = h.TckTitulo,
            Fecha = h.HorFecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            HoraInicio = h.HorHoraInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
            HoraFin = h.HorHoraFin?.ToString("HH:mm", CultureInfo.InvariantCulture),
            CapacidadMaxima = h.TckCapacidadMaxima,
            CuposDisponibles = h.HorCuposDisponibles,
            Estado = h.HorEstado,
            FechaIngreso = h.HorFechaIngreso,
        };

    private async Task<HorarioResponse> GetHorarioAsync(Guid horGuid, CancellationToken ct)
    {
        var h = await _repo.ObtenerHorarioAdminAsync(horGuid, ct)
            ?? throw new NotFoundException("Horario", horGuid);
        return ToHorarioResponse(h);
    }

    private static void Validar(object o)
    {
        var ctx = new ValidationContext(o);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(o, ctx, results, true))
            throw new DomainValidationException(results.Select(r => r.ErrorMessage ?? "inválido").ToList());
    }
}
