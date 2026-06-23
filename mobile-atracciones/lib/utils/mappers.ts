/**
 * Normaliza una reserva del API (ReservaResponseDto / snake_case).
 * El orquestador devuelve PascalCase serializado como snake_case via System.Text.Json
 * con JsonNamingPolicy.SnakeCaseLower, por lo que los campos llegan en snake_case.
 */
export interface ReservaNormalizada {
  rev_guid: string;
  at_guid: string;
  rev_codigo: string;
  atraccion_nombre: string;
  rev_fecha_reserva_utc: string;
  rev_estado: string;
  rev_subtotal: number;
  rev_valor_iva: number;
  rev_total: number;
  fecha_visita: string;
  detalle: Record<string, unknown>[];
}

export function normalizarReserva(raw: Record<string, unknown>): ReservaNormalizada {
  return {
    rev_guid: String(raw.rev_guid ?? raw.revGuid ?? ''),
    at_guid: String(raw.at_guid ?? raw.atGuid ?? ''),
    rev_codigo: String(raw.rev_codigo ?? raw.revCodigo ?? ''),
    atraccion_nombre: String(raw.atraccion_nombre ?? raw.atraccionNombre ?? ''),
    rev_fecha_reserva_utc: String(raw.rev_fecha_reserva_utc ?? raw.revFechaReservaUtc ?? ''),
    rev_estado: String(raw.rev_estado ?? raw.revEstado ?? raw.estado ?? 'P'),
    rev_subtotal: Number(raw.rev_subtotal ?? raw.revSubtotal ?? 0),
    rev_valor_iva: Number(raw.rev_valor_iva ?? raw.revValorIva ?? 0),
    rev_total: Number(raw.rev_total ?? raw.revTotal ?? raw.total ?? raw.total_pagar ?? 0),
    fecha_visita: String(raw.fecha_visita ?? raw.hor_fecha ?? raw.horFecha ?? ''),
    detalle: (Array.isArray(raw.detalle) ? raw.detalle : Array.isArray(raw.lineas) ? raw.lineas : []) as Record<string, unknown>[],
  };
}
