export function estadoLabel(codigo: string): string {
  const mapa: Record<string, string> = {
    A: 'Confirmada', P: 'Pendiente', C: 'Cancelada', I: 'Inactiva',
    X: 'Cancelada', F: 'Finalizada', ACTIVA: 'Confirmada', PENDIENTE: 'Pendiente',
    CONFIRMADA: 'Confirmada', PAGADA: 'Pagada', CANCELADA: 'Cancelada',
    INACTIVA: 'Inactiva', FINALIZADA: 'Finalizada', ACTIVE: 'Confirmada',
    CANCELLED: 'Cancelada', COMPLETED: 'Finalizada',
  };
  return mapa[String(codigo).toUpperCase()] ?? codigo ?? '—';
}

export function estadoColor(codigo: string): string {
  const c = String(codigo).toUpperCase();
  if (['A', 'ACTIVA', 'ACTIVE', 'CONFIRMADA', 'PAGADA'].includes(c)) return '#22c55e';
  if (['P', 'PENDIENTE'].includes(c)) return '#06b6d4';
  if (['C', 'CANCELADA', 'CANCELLED', 'X', 'I', 'INACTIVA'].includes(c)) return '#ef4444';
  return '#94a3b8';
}

export function esReservaActiva(codigo: string): boolean {
  const c = String(codigo).toUpperCase();
  return ['P', 'A', 'PENDIENTE', 'CONFIRMADA', 'PAGADA', 'ACTIVA', 'ACTIVE'].includes(c);
}

export function esReservaCancelable(codigo: string): boolean {
  return esReservaActiva(codigo);
}

export function esReservaConfirmada(codigo: string): boolean {
  const c = String(codigo).toUpperCase();
  return ['A', 'ACTIVA', 'ACTIVE', 'CONFIRMADA', 'PAGADA'].includes(c);
}
