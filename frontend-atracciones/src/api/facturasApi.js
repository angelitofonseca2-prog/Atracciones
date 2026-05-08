import { apiClient } from './atraccionesApi'

/**
 * GET /api/v1/facturas/mis-facturas (cliente autenticado).
 * Backend: FacturasPublicController. Devuelve `ApiListResponse<FacturaResponse>`.
 */
export const listarMisFacturas = async (params = {}) => {
  const response = await apiClient.get('/facturas/mis-facturas', { params })
  return response.data
}

/**
 * NOTA: el backend público no expone hoy un endpoint para que el cliente
 * genere su propia factura. La generación está restringida a admin
 * (`POST /admin/facturas`). Mientras no haya endpoint público se
 * reserva esta firma sin llamada para no inventar URLs.
 */
export const generarFactura = async () => {
  // eslint-disable-next-line no-throw-literal
  throw new Error(
    'La generación de facturas por cliente aún no está habilitada en el backend.',
  )
}
