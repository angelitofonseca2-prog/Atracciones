/** Formato corto de fecha (es-EC). */
export function formatearFechaCorta(valor) {
  if (!valor) return ''
  const texto = String(valor).slice(0, 10)
  const [y, m, d] = texto.split('-').map(Number)
  if (!y || !m || !d) return texto
  const fecha = new Date(Date.UTC(y, m - 1, d))
  return fecha.toLocaleDateString('es-EC', { day: '2-digit', month: 'short', year: 'numeric', timeZone: 'UTC' })
}

/** Muestra "15 may 2026" o "15 may 2026 — 20 may 2026" si hay rango. */
export function formatearRangoFechas(fechaInicio, fechaFin) {
  const ini = formatearFechaCorta(fechaInicio)
  const fin = formatearFechaCorta(fechaFin)
  if (!ini) return ''
  if (!fin || fin === ini) return ini
  return `${ini} — ${fin}`
}

/** Lista yyyy-MM-dd entre inicio y fin (inclusive). */
export function listarDiasEnRango(fechaInicio, fechaFin) {
  const ini = String(fechaInicio || '').slice(0, 10)
  const fin = String(fechaFin || ini).slice(0, 10)
  if (!ini) return []
  const dias = []
  let cursor = new Date(`${ini}T00:00:00Z`)
  const limite = new Date(`${fin}T00:00:00Z`)
  while (cursor <= limite) {
    dias.push(cursor.toISOString().slice(0, 10))
    cursor.setUTCDate(cursor.getUTCDate() + 1)
  }
  return dias
}

export function horarioTieneRangoFechas(horario) {
  const ini = String(horario?.fecha ?? '').slice(0, 10)
  const fin = String(horario?.fecha_fin ?? horario?.fecha ?? '').slice(0, 10)
  return Boolean(ini && fin && fin > ini)
}
