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

export function hoyUtcIso() {
  return new Date().toISOString().slice(0, 10)
}

/** Fecha de hoy en la zona local del navegador (YYYY-MM-DD).
 *  Se usa para comparar con las fechas de los horarios que vienen en hora local (Ecuador UTC-5).
 *  Usando toISOString() se obtendría UTC y a las 19:00 local ya sería "mañana" en UTC. */
export function hoyLocalIso() {
  const now = new Date()
  const y = now.getFullYear()
  const m = String(now.getMonth() + 1).padStart(2, '0')
  const d = String(now.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

/** Días del rango que aún no han pasado (>= hoy en hora LOCAL). */
export function listarDiasReservablesEnRango(fechaInicio, fechaFin) {
  const hoy = hoyLocalIso()
  return listarDiasEnRango(fechaInicio, fechaFin).filter((d) => d >= hoy)
}

export function etiquetaHorarioReserva(horario) {
  if (!horario) return ''
  const hora = `${horario.hora_inicio || ''}${horario.hora_fin ? `–${horario.hora_fin}` : ''}`
  const ticket = horario.ticket_titulo ? ` — ${horario.ticket_titulo}` : ''
  const cupos = (horario.cupos ?? horario.cupos_disponibles) != null
    ? ` · ${horario.cupos ?? horario.cupos_disponibles} cupos`
    : ''
  return `${hora}${ticket}${cupos}`
}
