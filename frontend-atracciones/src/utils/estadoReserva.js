/** Traduce código de estado a etiqueta legible */
export function estadoLabel(codigo) {
  const mapa = {
    A: 'Activa',
    P: 'Pendiente',
    C: 'Confirmada',
    X: 'Cancelada',
    F: 'Finalizada',
    // Variantes en texto completo (por si el backend devuelve texto)
    ACTIVA: 'Activa',
    PENDIENTE: 'Pendiente',
    CONFIRMADA: 'Confirmada',
    CANCELADA: 'Cancelada',
    FINALIZADA: 'Finalizada',
    ACTIVE: 'Activa',
    CANCELLED: 'Cancelada',
    COMPLETED: 'Finalizada',
  }
  return mapa[String(codigo).toUpperCase()] ?? codigo ?? '—'
}

export function estadoBadgeClass(codigo) {
  const c = String(codigo).toUpperCase()
  if (c === 'A' || c === 'ACTIVA' || c === 'ACTIVE' || c === 'CONFIRMADA' || c === 'C') return 'badge badge-green'
  if (c === 'X' || c === 'CANCELADA' || c === 'CANCELLED') return 'badge badge-red'
  return 'badge badge-blue'
}
