/**
 * Normaliza una atracción del listado (AtraccionListadoResponse).
 * Garantiza que los campos siempre estén en snake_case.
 */
export function normalizarAtraccion(raw) {
  if (!raw) return null
  return {
    id: raw.id ?? raw.at_guid,
    nombre: raw.nombre,
    ciudad: raw.ciudad,
    pais: raw.pais,
    precio_desde: raw.precio_desde ?? raw.precioDesde ?? 0,
    imagen_principal: raw.imagen_principal ?? raw.imagenPrincipal ?? null,
    calificacion: raw.calificacion ?? 0,
    total_resenas: raw.total_resenas ?? raw.totalResenas ?? 0,
    idiomas_disponibles: raw.idiomas_disponibles ?? raw.idiomasDisponibles ?? [],
  }
}

/**
 * Normaliza una reserva (ReservaResponse).
 */
export function normalizarReserva(raw) {
  if (!raw) return null
  return {
    rev_guid: raw.rev_guid,
    rev_codigo: raw.rev_codigo,
    atraccion_nombre: raw.atraccion_nombre,
    rev_fecha_reserva_utc: raw.rev_fecha_reserva_utc,
    rev_estado: raw.rev_estado,
    rev_subtotal: raw.rev_subtotal ?? 0,
    rev_valor_iva: raw.rev_valor_iva ?? 0,
    rev_total: raw.rev_total ?? 0,
    detalle: raw.detalle ?? [],
  }
}
