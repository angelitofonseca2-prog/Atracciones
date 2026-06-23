export function formatearFechaCorta(valor: string): string {
  if (!valor) return '';
  const texto = String(valor).slice(0, 10);
  const [y, m, d] = texto.split('-').map(Number);
  if (!y || !m || !d) return texto;
  const fecha = new Date(Date.UTC(y, m - 1, d));
  return fecha.toLocaleDateString('es-EC', { day: '2-digit', month: 'short', year: 'numeric', timeZone: 'UTC' });
}

export function formatearRangoFechas(fechaInicio?: string, fechaFin?: string): string {
  const ini = formatearFechaCorta(fechaInicio ?? '');
  const fin = formatearFechaCorta(fechaFin ?? '');
  if (!ini) return '';
  if (!fin || fin === ini) return ini;
  return `${ini} — ${fin}`;
}

export function listarDiasEnRango(fechaInicio?: string, fechaFin?: string): string[] {
  const ini = String(fechaInicio || '').slice(0, 10);
  const fin = String(fechaFin || ini).slice(0, 10);
  if (!ini) return [];
  const dias: string[] = [];
  let cursor = new Date(`${ini}T00:00:00Z`);
  const limite = new Date(`${fin}T00:00:00Z`);
  while (cursor <= limite) {
    dias.push(cursor.toISOString().slice(0, 10));
    cursor.setUTCDate(cursor.getUTCDate() + 1);
  }
  return dias;
}

export function hoyLocalIso(): string {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  const d = String(now.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export function listarDiasReservablesEnRango(fechaInicio?: string, fechaFin?: string): string[] {
  const hoy = hoyLocalIso();
  return listarDiasEnRango(fechaInicio, fechaFin).filter((d) => d >= hoy);
}
