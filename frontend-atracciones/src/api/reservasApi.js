import { apiClient } from './atraccionesApi'

/**
 * POST /api/v1/reservas
 * Acepta cliente autenticado (token) o invitado (body.cliente_invitado).
 */
const nuevaIdempotencyKey = () =>
  typeof crypto !== 'undefined' && crypto.randomUUID
    ? crypto.randomUUID()
    : `${Date.now()}-${Math.random().toString(36).slice(2)}`

export const crearReserva = async (body) => {
  const response = await apiClient.post('/reservas', body, {
    headers: { 'Idempotency-Key': nuevaIdempotencyKey() },
  })
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
 * POST /api/v1/pagos/paypal/orders — crea orden PayPal (servidor).
 */
export const crearOrdenPayPal = async (body) => {
  const response = await apiClient.post('/pagos/paypal/orders', body)
  return response.data
}

/**
 * POST /api/v1/pagos/paypal/orders/capture — captura y confirma reserva + factura.
 */
export const capturarOrdenPayPal = async (body) => {
  const response = await apiClient.post('/pagos/paypal/orders/capture', body, {
    headers: { 'Idempotency-Key': nuevaIdempotencyKey() },
  })
  return response.data
}
