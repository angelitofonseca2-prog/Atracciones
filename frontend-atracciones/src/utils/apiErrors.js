/**
 * Extrae mensaje legible de respuestas de error del API (snake_case: error, details).
 */
export function mensajeErrorRespuesta(err, fallback = 'No se pudo completar la operación.') {
  const body = err?.response?.data
  if (!body) return err?.message || fallback
  const detalle =
    Array.isArray(body.details) && body.details.length ? body.details.join(' ') : null
  return detalle || body.error || body.message || fallback
}
