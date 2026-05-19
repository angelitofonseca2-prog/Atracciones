/** Traduce código de estado a etiqueta legible (ventas: P, A, C, I). */
export function estadoLabel(codigo) {
  const mapa = {
    A: 'Confirmada',
    P: 'Pendiente',
    C: 'Cancelada',
    I: 'Inactiva',
    X: 'Cancelada',
    F: 'Finalizada',
    ACTIVA: 'Confirmada',
    PENDIENTE: 'Pendiente',
    CONFIRMADA: 'Confirmada',
    CANCELADA: 'Cancelada',
    INACTIVA: 'Inactiva',
    FINALIZADA: 'Finalizada',
    ACTIVE: 'Confirmada',
    CANCELLED: 'Cancelada',
    COMPLETED: 'Finalizada',
  }
  return mapa[String(codigo).toUpperCase()] ?? codigo ?? '—'
}

export function estadoBadgeClass(codigo) {
  const c = String(codigo).toUpperCase()
  if (c === 'A' || c === 'ACTIVA' || c === 'ACTIVE' || c === 'CONFIRMADA') return 'badge badge-green'
  if (c === 'P' || c === 'PENDIENTE') return 'badge badge-blue'
  if (c === 'C' || c === 'CANCELADA' || c === 'CANCELLED' || c === 'X' || c === 'I' || c === 'INACTIVA') {
    return 'badge badge-red'
  }
  if (c === 'F' || c === 'FINALIZADA' || c === 'COMPLETED') return 'badge'
  return 'badge badge-blue'
}

/** Reservas visibles en "Mis reservas": pendiente de pago o confirmada. */
export function esReservaActiva(codigo) {
  const c = String(codigo).toUpperCase()
  return c === 'P' || c === 'A'
}

/** El backend permite anular en P o A. */
export function esReservaCancelable(codigo) {
  return esReservaActiva(codigo)
}

/** Reserva pagada/confirmada: apta para dejar reseña. */
export function esReservaConfirmada(codigo) {
  const c = String(codigo).toUpperCase()
  return c === 'A' || c === 'ACTIVA' || c === 'ACTIVE' || c === 'CONFIRMADA'
}
