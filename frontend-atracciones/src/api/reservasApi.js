import { apiClient } from './atraccionesApi'

/**
 * POST /api/v1/reservas
 * Acepta cliente autenticado (token) o invitado (body.cliente_invitado).
 */
export const crearReserva = async (body) => {
  const response = await apiClient.post('/reservas', body)
  return response.data
}

/**
 * GET /api/v1/reservas/{guid} — el cliente solo puede consultar sus reservas.
 */
export const obtenerReserva = async (guid) => {
  const response = await apiClient.get(`/reservas/${guid}`)
  return response.data
}

/**
 * GET /api/v1/reservas (cliente autenticado).
 * Devuelve envelope `{ status, message, data, pagination }`.
 */
export const listarMisReservas = async (params = {}) => {
  const response = await apiClient.get('/reservas', { params })
  return response.data
}

/**
 * PUT /api/v1/reservas/{guid}/cancelar
 * Body: { motivo: string }. El backend marca la reserva con estado 'C'.
 */
export const cancelarReserva = async (guid, motivo) => {
  const response = await apiClient.put(`/reservas/${guid}/cancelar`, { motivo })
  return response.data
}

/**
 * POST /api/v1/reservas/{guid}/confirmar-pago
 * Body: { nombre_receptor, apellido_receptor?, correo_receptor, telefono_receptor?, observacion? }
 * Respuesta: FacturaResponse con fac_guid, fac_numero, rev_codigo, total, etc.
 */
export const confirmarPago = async (revGuid, body) => {
  const response = await apiClient.post(`/reservas/${revGuid}/confirmar-pago`, body)
  return response.data
}
